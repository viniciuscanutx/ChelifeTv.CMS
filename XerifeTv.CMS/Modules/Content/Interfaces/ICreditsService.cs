using XerifeTv.CMS.Modules.Common;
using XerifeTv.CMS.Modules.Content.Dtos.Response;

namespace XerifeTv.CMS.Modules.Content.Interfaces;

public interface ICreditsService
{
    Task<Result<CreditsContentV2ResponseDto>> GetMovieCreditsAsync(string movieId);
    Task<Result<CreditsContentV2ResponseDto>> GetSeriesCreditsAsync(string seriesId);
}
