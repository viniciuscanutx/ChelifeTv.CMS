namespace XerifeTv.CMS.Modules.Abstractions.ValueObjects;

public record CastMember(
  string Name,
  string? Character = null,
  string? ProfileUrl = null,
  int? PersonId = null);
