using XerifeTv.CMS.Modules.Abstractions.ValueObjects;
using XerifeTv.CMS.Modules.Integrations.Imdb.Dtos;

namespace XerifeTv.CMS.Modules.Content.Credits;

public static class TmdbCreditsMapper
{
    private const int MaxCastMembers = 20;
    private const int MaxCrewMembers = 15;

    // Funcoes da equipe exibidas, na ordem de importancia
    private static readonly string[] RelevantJobs =
    [
        "Director", "Screenplay", "Writer", "Story", "Novel", "Characters",
        "Original Music Composer", "Director of Photography", "Producer", "Executive Producer"
    ];

    public static (ICollection<CastMember> Cast, ICollection<CrewMember> Crew) FromMovie(GetMovieCreditsResponseDto credits)
    {
        ICollection<CastMember> cast = [.. credits.Cast
            .Where(c => !string.IsNullOrWhiteSpace(c.Name))
            .OrderBy(c => c.Order)
            .Take(MaxCastMembers)
            .Select(c => new CastMember(c.Name, NullIfEmpty(c.Character), ProfileUrl(c.ProfilePath), c.PersonId))];

        ICollection<CrewMember> crew = [.. credits.Crew
            .Where(c => !string.IsNullOrWhiteSpace(c.Name) && JobPriority(c.Job) < RelevantJobs.Length)
            .GroupBy(c => c.PersonId)
            .Select(g => new
            {
                Member = g.First(),
                Jobs = g.Select(c => c.Job).Distinct().OrderBy(JobPriority).ToArray()
            })
            .OrderBy(p => JobPriority(p.Jobs[0]))
            .Take(MaxCrewMembers)
            .Select(p => new CrewMember(p.Member.Name, p.Jobs, ProfileUrl(p.Member.ProfilePath), p.Member.PersonId))];

        return (cast, crew);
    }

    public static (ICollection<CastMember> Cast, ICollection<CrewMember> Crew) FromSeries(GetSeriesCreditsResponseDto credits)
    {
        ICollection<CastMember> cast = [.. credits.Cast
            .Where(c => !string.IsNullOrWhiteSpace(c.Name))
            .OrderBy(c => c.Order)
            .Take(MaxCastMembers)
            .Select(c => new CastMember(
                c.Name,
                c.Roles.OrderByDescending(r => r.EpisodeCount).Select(r => NullIfEmpty(r.Character)).FirstOrDefault(r => r is not null),
                ProfileUrl(c.ProfilePath),
                c.PersonId))];

        // Em serie quem participa de mais episodios vem primeiro
        ICollection<CrewMember> crew = [.. credits.Crew
            .Where(c => !string.IsNullOrWhiteSpace(c.Name))
            .Select(c => new
            {
                Member = c,
                Jobs = c.Jobs.Select(j => j.Job).Where(j => JobPriority(j) < RelevantJobs.Length).Distinct().OrderBy(JobPriority).ToArray()
            })
            .Where(p => p.Jobs.Length > 0)
            .OrderByDescending(p => p.Member.TotalEpisodeCount)
            .ThenBy(p => JobPriority(p.Jobs[0]))
            .Take(MaxCrewMembers)
            .Select(p => new CrewMember(p.Member.Name, p.Jobs, ProfileUrl(p.Member.ProfilePath), p.Member.PersonId))];

        return (cast, crew);
    }

    private static int JobPriority(string job)
    {
        var index = Array.IndexOf(RelevantJobs, job);
        return index < 0 ? RelevantJobs.Length : index;
    }

    private static string? ProfileUrl(string? path)
        => string.IsNullOrWhiteSpace(path) ? null : $"https://image.tmdb.org/t/p/w185{path}";

    private static string? NullIfEmpty(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
