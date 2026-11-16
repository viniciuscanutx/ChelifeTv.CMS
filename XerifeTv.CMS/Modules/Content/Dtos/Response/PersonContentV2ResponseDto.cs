namespace XerifeTv.CMS.Modules.Content.Dtos.Response;

public record PersonCreditContentV2ResponseDto(
    int TmdbId,
    string MediaType,
    string Title,
    int? Year,
    string? PosterURL,
    string? Character,
    string? Job,
    string? CatalogId);

public record PersonContentV2ResponseDto(
    int PersonId,
    string Name,
    string? Biography,
    string? Birthday,
    string? Deathday,
    string? PlaceOfBirth,
    string? ProfileURL,
    string? KnownForDepartment,
    IReadOnlyList<PersonCreditContentV2ResponseDto> KnownFor,
    IReadOnlyList<PersonCreditContentV2ResponseDto> InCatalog);
