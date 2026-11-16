using XerifeTv.CMS.Modules.Abstractions.Interfaces;
using XerifeTv.CMS.Modules.Abstractions.ValueObjects;
using XerifeTv.CMS.Modules.Common;
using XerifeTv.CMS.Modules.Common.Dtos;
using XerifeTv.CMS.Modules.Movie.Dtos.Request;

namespace XerifeTv.CMS.Modules.Movie.Interfaces;

public interface IMovieRepository : IBaseRepository<MovieEntity>
{
    Task<PagedList<MovieEntity>> GetByFilterAsync(GetMoviesByFilterRequestDto dto);
    Task<IEnumerable<ItemsByCategory<MovieEntity>>> GetGroupByCategoryAsync(GetGroupByCategoryRequestDto dto);
    Task<MovieEntity?> GetByImdbIdAsync(string imdbId);
    Task<ICollection<CategoryCountDto>> GetCategoriesWithCountAsync(bool includeDisabled = true);
    Task<IReadOnlyList<MovieEntity>> GetLatestEnabledAsync(int limit);
    Task<ICollection<MovieEntity>> GetMoviesRecommendedByMovieIdAsync(string movieId, int limit);
    Task<IEnumerable<MovieEntity>> GetMoviesByFranchiseIdAsync(string franchiseId, int limit, string? movieIdIgnore = null);
    Task<long> CountByFranchiseIdAsync(string franchiseId);
    Task<IReadOnlyList<MovieEntity>> GetByVideoHostAsync(string host);
    Task UpdateVideoUrlsAsync(string movieId, Video? video, string? alternativeVideoUrl);
    Task<IReadOnlyList<MovieEntity>> GetEnabledByTmdbIdsOrTitlesAsync(IReadOnlyCollection<string> tmdbIds, IReadOnlyCollection<string> titles);
    Task UpdateCreditsAsync(string movieId, ICollection<CastMember> cast, ICollection<CrewMember> crew, string? tmdbId);
}
