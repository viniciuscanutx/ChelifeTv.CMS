using XerifeTv.CMS.Modules.Abstractions.ValueObjects;

namespace XerifeTv.CMS.Modules.Content.Dtos.Response;

public record CrewMemberContentV2ResponseDto(int? PersonId, string Name, string[] Jobs, string? ProfileURL)
{
    public static CrewMemberContentV2ResponseDto FromValueObject(CrewMember member)
        => new(member.PersonId, member.Name, member.Jobs, member.ProfileUrl);
}
