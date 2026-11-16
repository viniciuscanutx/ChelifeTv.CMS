using XerifeTv.CMS.Modules.Abstractions.Entities;
using XerifeTv.CMS.Modules.Abstractions.ValueObjects;

namespace XerifeTv.CMS.Modules.Series;

public sealed class SeriesEntity : MediaContent
{
    public string ImdbId { get; set; } = string.Empty;
    public string? TmdbId { get; set; }
    // null = creditos ainda nao buscados no TMDB
    public ICollection<CastMember>? Cast { get; set; }
    public ICollection<CrewMember>? Crew { get; set; }
    public string? FranchiseId { get; set; }
    public ICollection<string> Categories { get; set; } = [];
    public string? LogoUrl { get; set; }
    public float Review { get; set; }
    public int NumberSeasons { get; set; } = 1;
    public ICollection<Episode> Episodes { get; set; } = [];
    public bool Disabled { get; set; } = false;
}
