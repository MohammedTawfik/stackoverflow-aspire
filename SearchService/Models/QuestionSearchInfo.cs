using System.Text.Json.Serialization;

namespace SearchService.Models
{
    public class QuestionSearchInfo
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }
        [JsonPropertyName("title")]
        public required string Title { get; set; }
        [JsonPropertyName("content")]
        public required string Content { get; set; }
        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = [];
        [JsonPropertyName("createddate")]
        public long CreatedAt { get; set; }
        [JsonPropertyName("hasacceptedanswer")]
        public bool HasAcceptedAnswer { get; set; }
        [JsonPropertyName("answerscount")]
        public int AnswersCount { get; set; }

    }
}
