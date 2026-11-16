using Newtonsoft.Json;

namespace XerifeTv.CMS.Modules.Integrations.Imdb.Dtos;

public class GetPersonResponseDto
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("biography")]
    public string? Biography { get; set; }

    [JsonProperty("birthday")]
    public string? Birthday { get; set; }

    [JsonProperty("deathday")]
    public string? Deathday { get; set; }

    [JsonProperty("place_of_birth")]
    public string? PlaceOfBirth { get; set; }

    [JsonProperty("profile_path")]
    public string? ProfilePath { get; set; }

    [JsonProperty("known_for_department")]
    public string? KnownForDepartment { get; set; }

    [JsonProperty("combined_credits")]
    public CombinedCreditsDto? CombinedCredits { get; set; }

    public class CombinedCreditsDto
    {
        [JsonProperty("cast")]
        public List<CreditDto> Cast { get; set; } = [];

        [JsonProperty("crew")]
        public List<CreditDto> Crew { get; set; } = [];
    }

    public class CreditDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("media_type")]
        public string MediaType { get; set; } = string.Empty;

        // Filme usa title/release_date; serie usa name/first_air_date
        [JsonProperty("title")]
        public string? Title { get; set; }

        [JsonProperty("original_title")]
        public string? OriginalTitle { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("original_name")]
        public string? OriginalName { get; set; }

        [JsonProperty("release_date")]
        public string? ReleaseDate { get; set; }

        [JsonProperty("first_air_date")]
        public string? FirstAirDate { get; set; }

        [JsonProperty("poster_path")]
        public string? PosterPath { get; set; }

        [JsonProperty("character")]
        public string? Character { get; set; }

        [JsonProperty("job")]
        public string? Job { get; set; }

        [JsonProperty("vote_count")]
        public int VoteCount { get; set; }

        [JsonProperty("genre_ids")]
        public List<int> GenreIds { get; set; } = [];
    }
}
