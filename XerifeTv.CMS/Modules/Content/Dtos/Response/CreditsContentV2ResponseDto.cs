using System.Text.Json.Serialization;
using XerifeTv.CMS.Modules.Abstractions.ValueObjects;

namespace XerifeTv.CMS.Modules.Content.Dtos.Response;

public record CreditsContentV2ResponseDto(
    IReadOnlyList<CastMemberContentV2ResponseDto> Cast,
    IReadOnlyList<CrewMemberContentV2ResponseDto> Crew)
{
    public static CreditsContentV2ResponseDto Empty => new([], []);

    [JsonIgnore]
    public bool IsEmpty => Cast.Count == 0 && Crew.Count == 0;

    public static CreditsContentV2ResponseDto FromValueObjects(IEnumerable<CastMember> cast, IEnumerable<CrewMember> crew)
        => new(
            [.. cast.Select(CastMemberContentV2ResponseDto.FromValueObject)],
            [.. crew.Select(CrewMemberContentV2ResponseDto.FromValueObject)]);
}
