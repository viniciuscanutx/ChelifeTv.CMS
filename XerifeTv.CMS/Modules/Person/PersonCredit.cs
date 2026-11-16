namespace XerifeTv.CMS.Modules.Person;

public record PersonCredit(
  int TmdbId,
  string MediaType,
  string Title,
  string? OriginalTitle = null,
  int? Year = null,
  string? PosterUrl = null,
  string? Character = null,
  string? Job = null,
  int VoteCount = 0);
