using Newtonsoft.Json;

namespace XerifeTv.CMS.Modules.Integrations.Imdb.Dtos;

public class GetMovieCreditsResponseDto
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

        [JsonProperty("character")]
        public string? Character { get; set; }

        [JsonProperty("profile_path")]
        public string? ProfilePath { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }
    }

    public class CrewDto
    {
        [JsonProperty("id")]
        public int PersonId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("job")]
        public string Job { get; set; } = string.Empty;

        [JsonProperty("profile_path")]
        public string? ProfilePath { get; set; }
    }
}
