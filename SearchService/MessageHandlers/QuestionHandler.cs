using System.Text.RegularExpressions;
using Typesense;

namespace SearchService.MessageHandlers
{
    public class QuestionHandler(ITypesenseClient typesenseClient)
    {
        public async Task HandleAsync(Common.Contracts.QuestionCreatedEvent questionEvent)
        {
            var questionSearchInfo = new Models.QuestionSearchInfo
            {
                Id = questionEvent.id,
                Title = questionEvent.title,
                Content = StripHtmlTags(questionEvent.content),
                Tags = questionEvent.tags,
                CreatedAt = new DateTimeOffset(questionEvent.createdAt).ToUnixTimeSeconds(),
            };
            await typesenseClient.CreateDocument("questions", questionSearchInfo);
            Console.WriteLine($"Indexed question with ID: {questionSearchInfo.Id} into Typesense.");
        }

        private string StripHtmlTags(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;
            return Regex.Replace(input, "<.*?>", string.Empty);
        }
    }
}
