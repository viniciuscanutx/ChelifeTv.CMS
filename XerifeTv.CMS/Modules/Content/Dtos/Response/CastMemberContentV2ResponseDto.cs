using XerifeTv.CMS.Modules.Abstractions.ValueObjects;

namespace XerifeTv.CMS.Modules.Content.Dtos.Response;

public record CastMemberContentV2ResponseDto(int? PersonId, string Name, string? Character, string? ProfileURL)
{
    public static CastMemberContentV2ResponseDto FromValueObject(CastMember member)
        => new(member.PersonId, member.Name, member.Character, member.ProfileUrl);
}
