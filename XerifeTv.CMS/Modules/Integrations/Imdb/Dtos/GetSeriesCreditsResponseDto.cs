using Newtonsoft.Json;

namespace XerifeTv.CMS.Modules.Integrations.Imdb.Dtos;

public class GetSeriesCreditsResponseDto
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("cast")]
    public List<CastDto> Cast { get; set; } = [];

    [JsonProperty("crew")]
    public List<CrewDto> Crew { get; set; } = [];

    public class CastDto
    {
        [JsonProperty("id")]
        public int PersonId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("profile_path")]
        public string? ProfilePath { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }

        [JsonProperty("roles")]
        public List<RoleDto> Roles { get; set; } = [];
    }

    public class RoleDto
    {
        [JsonProperty("character")]
        public string? Character { get; set; }

        [JsonProperty("episode_count")]
        public int EpisodeCount { get; set; }
    }

    public class CrewDto
    {
        [JsonProperty("id")]
        public int PersonId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("profile_path")]
        public string? ProfilePath { get; set; }

        [JsonProperty("total_episode_count")]
        public int TotalEpisodeCount { get; set; }

        [JsonProperty("jobs")]
        public List<JobDto> Jobs { get; set; } = [];
    }

    public class JobDto
    {
        [JsonProperty("job")]
        public string Job { get; set; } = string.Empty;
    }
}
