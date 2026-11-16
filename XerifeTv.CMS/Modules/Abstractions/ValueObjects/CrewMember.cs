namespace XerifeTv.CMS.Modules.Abstractions.ValueObjects;

public record CrewMember(
  string Name,
  string[] Jobs,
  string? ProfileUrl = null,
  int? PersonId = null);
