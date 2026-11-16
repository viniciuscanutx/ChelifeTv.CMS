using XerifeTv.CMS.Modules.Integrations.Imdb.Dtos;
using XerifeTv.CMS.Modules.Person;

namespace XerifeTv.CMS.Modules.Content.People;

public static class TmdbPersonMapper
{
    public const string MovieMediaType = "movie";
    public const string TvMediaType = "tv";

    // Talk show (10767) e jornal (10763) nao contam como trabalho da pessoa
    private static readonly int[] IgnoredGenreIds = [10767, 10763];

    public static PersonEntity ToEntity(GetPersonResponseDto dto, string? fallbackBiography)
    {
        var credits = dto.CombinedCredits is null
            ? []
            : dto.CombinedCredits.Cast.Concat(dto.CombinedCredits.Crew)
                .Where(c => c.MediaType is MovieMediaType or TvMediaType)
                .Where(c => !c.GenreIds.Any(genreId => IgnoredGenreIds.Contains(genreId)))
                .Select(ToCredit)
                .Where(c => !string.IsNullOrWhiteSpace(c.Title))
                .ToList();

        return new PersonEntity
        {
            Id = dto.Id.ToString(),
            Name = dto.Name,
            Biography = NullIfEmpty(dto.Biography) ?? NullIfEmpty(fallbackBiography),
            Birthday = NullIfEmpty(dto.Birthday),
            Deathday = NullIfEmpty(dto.Deathday),
            PlaceOfBirth = NullIfEmpty(dto.PlaceOfBirth),
            ProfileUrl = ImageUrl(dto.ProfilePath),
            KnownForDepartment = NullIfEmpty(dto.KnownForDepartment),
            Credits = credits,
            FetchedAt = DateTime.UtcNow
        };
    }

    private static PersonCredit ToCredit(GetPersonResponseDto.CreditDto credit)
    {
        var isMovie = credit.MediaType == MovieMediaType;
        var date = isMovie ? credit.ReleaseDate : credit.FirstAirDate;

        return new PersonCredit(
            credit.Id,
            credit.MediaType,
            (isMovie ? credit.Title : credit.Name)?.Trim() ?? string.Empty,
            NullIfEmpty(isMovie ? credit.OriginalTitle : credit.OriginalName),
            ParseYear(date),
            ImageUrl(credit.PosterPath),
            NullIfEmpty(credit.Character),
            NullIfEmpty(credit.Job),
            credit.VoteCount);
    }

    private static int? ParseYear(string? date)
        => !string.IsNullOrWhiteSpace(date) && date.Length >= 4 && int.TryParse(date[..4], out var year) ? year : null;

    private static string? ImageUrl(string? path)
        => string.IsNullOrWhiteSpace(path) ? null : $"https://image.tmdb.org/t/p/w342{path}";

    private static string? NullIfEmpty(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
