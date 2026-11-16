using XerifeTv.CMS.Modules.Common;
using XerifeTv.CMS.Modules.Content.Dtos.Response;
using XerifeTv.CMS.Modules.Content.Interfaces;
using XerifeTv.CMS.Modules.Integrations.Imdb.Services;
using XerifeTv.CMS.Modules.Movie.Interfaces;
using XerifeTv.CMS.Modules.Series;
using XerifeTv.CMS.Modules.Series.Interfaces;

namespace XerifeTv.CMS.Modules.Content.Credits;

// Busca os creditos no TMDB na primeira vez e grava no titulo; falha do TMDB nao grava
public class CreditsService(
    IMovieRepository _movieRepository,
    ISeriesRepository _seriesRepository,
    IImdbService _imdbService) : ICreditsService
{
    public async Task<Result<CreditsContentV2ResponseDto>> GetMovieCreditsAsync(string movieId)
    {
        try
        {
            var movie = await _movieRepository.GetAsync(movieId);

            if (movie is null || movie.Disabled)
                return Result<CreditsContentV2ResponseDto>.Failure(new("404", "Conteudo nao encontrado"));

            if (movie.Cast is not null && movie.Crew is not null)
                return Result<CreditsContentV2ResponseDto>.Success(
                    CreditsContentV2ResponseDto.FromValueObjects(movie.Cast, movie.Crew));

            var externalId = !string.IsNullOrWhiteSpace(movie.TmdbId) ? movie.TmdbId : movie.ImdbId;
            if (string.IsNullOrWhiteSpace(externalId))
                return Result<CreditsContentV2ResponseDto>.Success(CreditsContentV2ResponseDto.Empty);

            var credits = await _imdbService.GetMovieCreditsAsync(externalId.Trim());
            if (credits.IsFailure || credits.Data is null)
                return Result<CreditsContentV2ResponseDto>.Success(CreditsContentV2ResponseDto.Empty);

            var (cast, crew) = TmdbCreditsMapper.FromMovie(credits.Data);
            var tmdbIdToSave = string.IsNullOrWhiteSpace(movie.TmdbId) && credits.Data.Id > 0
                ? credits.Data.Id.ToString()
                : null;

            await _movieRepository.UpdateCreditsAsync(movie.Id, cast, crew, tmdbIdToSave);

            return Result<CreditsContentV2ResponseDto>.Success(CreditsContentV2ResponseDto.FromValueObjects(cast, crew));
        }
        catch (Exception ex)
        {
            return Result<CreditsContentV2ResponseDto>.Failure(new("500", ex.Message));
        }
    }

    public async Task<Result<CreditsContentV2ResponseDto>> GetSeriesCreditsAsync(string seriesId)
    {
        try
        {
            var series = await _seriesRepository.GetAsync(seriesId);

            if (series is null || series.Disabled)
                return Result<CreditsContentV2ResponseDto>.Failure(new("404", "Conteudo nao encontrado"));

            if (series.Cast is not null && series.Crew is not null)
                return Result<CreditsContentV2ResponseDto>.Success(
                    CreditsContentV2ResponseDto.FromValueObjects(series.Cast, series.Crew));

            var tmdbId = await ResolveSeriesTmdbIdAsync(series);
            if (tmdbId is null)
                return Result<CreditsContentV2ResponseDto>.Success(CreditsContentV2ResponseDto.Empty);

            var credits = await _imdbService.GetSeriesCreditsAsync(tmdbId.Value);
            if (credits.IsFailure || credits.Data is null)
                return Result<CreditsContentV2ResponseDto>.Success(CreditsContentV2ResponseDto.Empty);

            var (cast, crew) = TmdbCreditsMapper.FromSeries(credits.Data);
            var tmdbIdToSave = string.IsNullOrWhiteSpace(series.TmdbId) ? tmdbId.Value.ToString() : null;

            await _seriesRepository.UpdateCreditsAsync(series.Id, cast, crew, tmdbIdToSave);

            return Result<CreditsContentV2ResponseDto>.Success(CreditsContentV2ResponseDto.FromValueObjects(cast, crew));
        }
        catch (Exception ex)
        {
            return Result<CreditsContentV2ResponseDto>.Failure(new("500", ex.Message));
        }
    }

    // O aggregate_credits do TMDB so aceita o id do TMDB; sem ele, resolve pelo ImdbId
    private async Task<int?> ResolveSeriesTmdbIdAsync(SeriesEntity series)
    {
        if (int.TryParse(series.TmdbId, out var storedId) && storedId > 0) return storedId;
        if (string.IsNullOrWhiteSpace(series.ImdbId)) return null;

        var found = await _imdbService.GetAllResultsByImdbIdAsync(series.ImdbId.Trim());
        var tvId = found.Data?.TvResults.FirstOrDefault()?.Id ?? 0;

        return tvId > 0 ? tvId : null;
    }
}
