using XerifeTv.CMS.Modules.Person;

namespace XerifeTv.CMS.Modules.Content.People;

// Titulo do catalogo bate com o credito pelo TmdbId ou, sem ele, por titulo + ano
public static class CatalogCreditMatcher
{
    public static bool Matches(PersonCredit credit, string? catalogTmdbId, string catalogTitle, int catalogYear)
    {
        if (!string.IsNullOrWhiteSpace(catalogTmdbId))
            return catalogTmdbId.Trim() == credit.TmdbId.ToString();

        return credit.Year == catalogYear
            && (SameTitle(catalogTitle, credit.Title) || SameTitle(catalogTitle, credit.OriginalTitle));
    }

    public static IReadOnlyCollection<string> TmdbIds(IEnumerable<PersonCredit> credits)
        => [.. credits.Select(c => c.TmdbId.ToString()).Distinct()];

    public static IReadOnlyCollection<string> Titles(IEnumerable<PersonCredit> credits)
        => [.. credits
            .SelectMany(c => new[] { c.Title, c.OriginalTitle })
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t!.Trim())
            .Distinct()];

    private static bool SameTitle(string catalogTitle, string? creditTitle)
        => creditTitle is not null && string.Equals(catalogTitle.Trim(), creditTitle.Trim(), StringComparison.OrdinalIgnoreCase);
}
