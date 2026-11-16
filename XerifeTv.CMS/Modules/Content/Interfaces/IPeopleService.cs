using XerifeTv.CMS.Modules.Common;
using XerifeTv.CMS.Modules.Content.Dtos.Response;

namespace XerifeTv.CMS.Modules.Content.Interfaces;

public interface IPeopleService
{
    Task<Result<PersonContentV2ResponseDto?>> GetPersonAsync(int personId);
}
