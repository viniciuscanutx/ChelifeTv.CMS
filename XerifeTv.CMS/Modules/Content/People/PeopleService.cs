using XerifeTv.CMS.Modules.Common;
using XerifeTv.CMS.Modules.Content.Dtos.Response;
using XerifeTv.CMS.Modules.Content.Interfaces;
using XerifeTv.CMS.Modules.Integrations.Imdb.Services;
using XerifeTv.CMS.Modules.Movie;
using XerifeTv.CMS.Modules.Movie.Interfaces;
using XerifeTv.CMS.Modules.Person;
using XerifeTv.CMS.Modules.Person.Interfaces;
using XerifeTv.CMS.Modules.Series;
using XerifeTv.CMS.Modules.Series.Interfaces;

namespace XerifeTv.CMS.Modules.Content.People;

public class PeopleService(
    IPersonRepository _personRepository,
    IMovieRepository _movieRepository,
    ISeriesRepository _seriesRepository,
    IImdbService _imdbService) : IPeopleService
{
    private const int MaxKnownFor = 12;
    private static readonly TimeSpan RefreshAfter = TimeSpan.FromDays(30);

    public async Task<Result<PersonContentV2ResponseDto?>> GetPersonAsync(int personId)
    {
        try
        {
            var stored = await _personRepository.GetAsync(personId.ToString());
            var person = stored is not null && DateTime.UtcNow - stored.FetchedAt < RefreshAfter
                ? stored
                : await FetchAndSaveAsync(personId) ?? stored;

            if (person is null)
                return Result<PersonContentV2ResponseDto?>.Failure(new("404", "Pessoa nao encontrada"));

            var movieCredits = person.Credits.Where(c => c.MediaType == TmdbPersonMapper.MovieMediaType).ToList();
            var seriesCredits = person.Credits.Where(c => c.MediaType == TmdbPersonMapper.TvMediaType).ToList();

            var movies = await FindMoviesAsync(movieCredits);
            var series = await FindSeriesAsync(seriesCredits);

            return Result<PersonContentV2ResponseDto?>.Success(ToResponse(person, movies, series));
        }
        catch (Exception ex)
        {
            return Result<PersonContentV2ResponseDto?>.Failure(new("500", ex.Message));
        }
    }

    // Falha do TMDB devolve null para manter a copia antiga, se houver
    private async Task<PersonEntity?> FetchAndSaveAsync(int personId)
    {
        var result = await _imdbService.GetPersonAsync(personId, "pt-BR", includeCredits: true);
        if (result.IsFailure || result.Data is null) return null;

        // Biografia em pt-BR costuma faltar; cai para ingles
        string? fallbackBiography = null;
        if (string.IsNullOrWhiteSpace(result.Data.Biography))
            fallbackBiography = (await _imdbService.GetPersonAsync(personId, "en-US", includeCredits: false)).Data?.Biography;

        var person = TmdbPersonMapper.ToEntity(result.Data, fallbackBiography);
        await _personRepository.UpsertAsync(person);
        return person;
    }

    private async Task<IReadOnlyList<MovieEntity>> FindMoviesAsync(IReadOnlyList<PersonCredit> credits)
    {
        if (credits.Count == 0) return [];

        var candidates = await _movieRepository.GetEnabledByTmdbIdsOrTitlesAsync(
            CatalogCreditMatcher.TmdbIds(credits), CatalogCreditMatcher.Titles(credits));

        return [.. candidates.Where(m => credits.Any(c => CatalogCreditMatcher.Matches(c, m.TmdbId, m.Title, m.ReleaseYear)))];
    }

    private async Task<IReadOnlyList<SeriesEntity>> FindSeriesAsync(IReadOnlyList<PersonCredit> credits)
    {
        if (credits.Count == 0) return [];

        var candidates = await _seriesRepository.GetEnabledByTmdbIdsOrTitlesAsync(
            CatalogCreditMatcher.TmdbIds(credits), CatalogCreditMatcher.Titles(credits));

        return [.. candidates.Where(s => credits.Any(c => CatalogCreditMatcher.Matches(c, s.TmdbId, s.Title, s.ReleaseYear)))];
    }

    private static PersonContentV2ResponseDto ToResponse(PersonEntity person, IReadOnlyList<MovieEntity> movies, IReadOnlyList<SeriesEntity> series)
    {
        var knownFor = person.Credits
            .GroupBy(c => (c.MediaType, c.TmdbId))
            .Select(g => g.First())
            .OrderByDescending(c => c.VoteCount)
            .Take(MaxKnownFor)
            .Select(c => new PersonCreditContentV2ResponseDto(
                c.TmdbId,
                c.MediaType == TmdbPersonMapper.TvMediaType ? "series" : "movie",
                c.Title,
                c.Year,
                c.PosterUrl,
                c.Character,
                c.Job,
                FindCatalogId(c, movies, series)))
            .ToList();

        return new PersonContentV2ResponseDto(
            int.Parse(person.Id),
            person.Name,
            person.Biography,
            person.Birthday,
            person.Deathday,
            person.PlaceOfBirth,
            person.ProfileUrl,
            person.KnownForDepartment,
            knownFor,
            ToCatalogCredits(person.Credits, movies, series));
    }

    // Titulos do catalogo no mesmo formato dos creditos, com titulo/poster do site e o papel da pessoa
    private static IReadOnlyList<PersonCreditContentV2ResponseDto> ToCatalogCredits(
        ICollection<PersonCredit> credits, IReadOnlyList<MovieEntity> movies, IReadOnlyList<SeriesEntity> series)
    {
        var movieItems = movies.Select(m => ToCatalogCredit(
            credits.FirstOrDefault(c => c.MediaType == TmdbPersonMapper.MovieMediaType && CatalogCreditMatcher.Matches(c, m.TmdbId, m.Title, m.ReleaseYear)),
            "movie", m.Id, m.Title, m.ReleaseYear, m.PosterUrl));

        var seriesItems = series.Select(s => ToCatalogCredit(
            credits.FirstOrDefault(c => c.MediaType == TmdbPersonMapper.TvMediaType && CatalogCreditMatcher.Matches(c, s.TmdbId, s.Title, s.ReleaseYear)),
            "series", s.Id, s.Title, s.ReleaseYear, s.PosterUrl));

        return [.. movieItems.Concat(seriesItems).OrderByDescending(c => c.Year)];
    }

    private static PersonCreditContentV2ResponseDto ToCatalogCredit(
        PersonCredit? credit, string mediaType, string catalogId, string title, int year, string posterUrl)
        => new(
            credit?.TmdbId ?? 0,
            mediaType,
            title,
            year,
            string.IsNullOrWhiteSpace(posterUrl) ? credit?.PosterUrl : posterUrl,
            credit?.Character,
            credit?.Job,
            catalogId);

    private static string? FindCatalogId(PersonCredit credit, IReadOnlyList<MovieEntity> movies, IReadOnlyList<SeriesEntity> series)
        => credit.MediaType == TmdbPersonMapper.MovieMediaType
            ? movies.FirstOrDefault(m => CatalogCreditMatcher.Matches(credit, m.TmdbId, m.Title, m.ReleaseYear))?.Id
            : series.FirstOrDefault(s => CatalogCreditMatcher.Matches(credit, s.TmdbId, s.Title, s.ReleaseYear))?.Id;
}
