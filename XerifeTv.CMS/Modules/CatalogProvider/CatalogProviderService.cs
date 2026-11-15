using XerifeTv.CMS.Modules.Abstractions.ValueObjects;
using XerifeTv.CMS.Modules.CatalogProvider.Dtos.Request;
using XerifeTv.CMS.Modules.CatalogProvider.Dtos.Response;
using XerifeTv.CMS.Modules.CatalogProvider.Interfaces;
using XerifeTv.CMS.Modules.Common;
using XerifeTv.CMS.Modules.Movie.Interfaces;
using XerifeTv.CMS.Modules.Series.Interfaces;

namespace XerifeTv.CMS.Modules.CatalogProvider;

public class CatalogProviderService(
    ICatalogProviderRepository _repository,
    IMovieRepository _movieRepository,
    ISeriesRepository _seriesRepository,
    ILogger<CatalogProviderService> _logger) : ICatalogProviderService
{
    public async Task<Result<IEnumerable<GetCatalogProviderResponseDto>>> GetAllAsync(bool isIncludeDisabled = false)
    {
        try
        {
            var response = await _repository.GetAsync(isIncludeDisabled);

            return Result<IEnumerable<GetCatalogProviderResponseDto>>.Success(
                response
                .OrderBy(p => p.Order)
                .ThenByDescending(p => p.IsDefault)
                .ThenBy(p => p.Name)
                .Select(GetCatalogProviderResponseDto.FromEntity));
        }
        catch (Exception ex)
        {
            var error = new Error("500", ex.InnerException?.Message ?? ex.Message);
            return Result<IEnumerable<GetCatalogProviderResponseDto>>.Failure(error);
        }
    }

    public async Task<Result<GetCatalogProviderResponseDto?>> GetAsync(string id)
    {
        try
        {
            var response = await _repository.GetAsync(id);

            if (response == null)
                return Result<GetCatalogProviderResponseDto?>.Failure(new Error("404", "Provedor de catálogo não encontrado"));

            return Result<GetCatalogProviderResponseDto?>.Success(GetCatalogProviderResponseDto.FromEntity(response));
        }
        catch (Exception ex)
        {
            var error = new Error("500", ex.InnerException?.Message ?? ex.Message);
            return Result<GetCatalogProviderResponseDto?>.Failure(error);
        }
    }

    public async Task<Result<string>> CreateAsync(CreateCatalogProviderRequestDto dto)
    {
        try
        {
            var entity = dto.ToEntity();

            if (entity.IsDefault) await ClearDefaultAsync();

            var response = await _repository.CreateAsync(entity);

            return Result<string>.Success(response);
        }
        catch (Exception ex)
        {
            var error = new Error("500", ex.InnerException?.Message ?? ex.Message);
            return Result<string>.Failure(error);
        }
    }

    public async Task<Result<string>> UpdateAsync(UpdateCatalogProviderRequestDto dto)
    {
        try
        {
            var entity = dto.ToEntity();
            var response = await _repository.GetAsync(dto.Id);

            if (response == null)
                return Result<string>.Failure(new Error("404", "Provedor de catálogo não encontrado"));

            if (entity.IsDefault) await ClearDefaultAsync(exceptId: entity.Id);

            entity.CreateAt = response.CreateAt;
            await _repository.UpdateAsync(entity);

            // Token do provedor girou: leva os links gravados pro BaseUrl atual
            await RebaseContentUrlsAsync(entity.BaseUrl);

            return Result<string>.Success(entity.Id);
        }
        catch (Exception ex)
        {
            var error = new Error("500", ex.InnerException?.Message ?? ex.Message);
            return Result<string>.Failure(error);
        }
    }

    public async Task<Result<bool>> DeleteAsync(string id)
    {
        try
        {
            var response = await _repository.GetAsync(id);

            if (response == null)
                return Result<bool>.Failure(new Error("404", "Provedor de catálogo não encontrado"));

            await _repository.DeleteAsync(id);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            var error = new Error("500", ex.InnerException?.Message ?? ex.Message);
            return Result<bool>.Failure(error);
        }
    }

    private async Task RebaseContentUrlsAsync(string baseUrl)
    {
        var host = CatalogUrlRebaser.GetHost(baseUrl);
        if (host is null) return;

        var moviesUpdated = await RebaseMovieUrlsAsync(host, baseUrl);
        var episodesUpdated = await RebaseEpisodeUrlsAsync(host, baseUrl);

        _logger.LogInformation(
            "Catalog provider {Host}: {Movies} movies and {Episodes} episodes moved to the current base URL",
            host, moviesUpdated, episodesUpdated);
    }

    private async Task<int> RebaseMovieUrlsAsync(string host, string baseUrl)
    {
        var movies = await _movieRepository.GetByVideoHostAsync(host);
        var updatedCount = 0;

        foreach (var movie in movies)
        {
            var video = RebaseVideo(movie.Video, baseUrl);
            var alternativeVideoUrl = CatalogUrlRebaser.Rebase(movie.AlternativeVideoUrl, baseUrl);

            if (video == movie.Video && alternativeVideoUrl == movie.AlternativeVideoUrl) continue;

            await _movieRepository.UpdateVideoUrlsAsync(movie.Id, video, alternativeVideoUrl);
            updatedCount++;
        }

        return updatedCount;
    }

    private async Task<int> RebaseEpisodeUrlsAsync(string host, string baseUrl)
    {
        var seriesList = await _seriesRepository.GetWithEpisodesByVideoHostAsync(host);
        var updatedCount = 0;

        foreach (var series in seriesList)
        {
            var changedEpisodes = series.Episodes
                .Select(episode => (
                    Original: episode,
                    Video: RebaseVideo(episode.Video, baseUrl),
                    AlternativeVideoUrl: CatalogUrlRebaser.Rebase(episode.AlternativeVideoUrl, baseUrl)))
                .Where(x => x.Video != x.Original.Video || x.AlternativeVideoUrl != x.Original.AlternativeVideoUrl)
                .Select(x => (x.Original.Id, x.Video, x.AlternativeVideoUrl))
                .ToList();

            if (changedEpisodes.Count == 0) continue;

            await _seriesRepository.UpdateEpisodesVideoUrlsAsync(series.Id, changedEpisodes);
            updatedCount += changedEpisodes.Count;
        }

        return updatedCount;
    }

    private static Video? RebaseVideo(Video? video, string baseUrl)
        => video is null ? null : video with { Url = CatalogUrlRebaser.Rebase(video.Url, baseUrl) ?? video.Url };

    // Só um provedor pode ser o padrão: ao marcar um, desmarca os demais.
    private async Task ClearDefaultAsync(string? exceptId = null)
    {
        var providers = await _repository.GetAsync(isIncludeDisabled: true);
        foreach (var provider in providers.Where(p => p.IsDefault && p.Id != exceptId))
        {
            provider.IsDefault = false;
            await _repository.UpdateAsync(provider);
        }
    }
}
