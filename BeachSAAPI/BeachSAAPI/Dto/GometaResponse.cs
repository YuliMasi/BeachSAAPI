using System.Text.Json.Serialization;

namespace BeachSAAPI.Dto
{
    public class GometaResponse
    {
        [JsonPropertyName("results")]
        public List<GometaPerson> Results { get; set; } = new();

        [JsonPropertyName("resultcount")]
        public int ResultCount { get; set; }
    }

    public class GometaPerson
    {
        [JsonPropertyName("cedula")]
        public string Cedula { get; set; }

        [JsonPropertyName("fullname")]
        public string FullName { get; set; }

        [JsonPropertyName("firstname")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastname1")]
        public string LastName1 { get; set; }

        [JsonPropertyName("lastname2")]
        public string LastName2 { get; set; }

        [JsonPropertyName("guess_type")]
        public string GuessType { get; set; }
    }
}