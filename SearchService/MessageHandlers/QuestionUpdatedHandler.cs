using Common.Contracts;
using SearchService.Models;
using System.Text.RegularExpressions;
using Typesense;

namespace SearchService.MessageHandlers
{
    public class QuestionUpdatedHandler(ITypesenseClient typesenseClient)
    {
        public async Task HandleAsync(QuestionUpdatedEvent message) 
        {
            await typesenseClient.UpdateDocument("questions", message.Id, new {
                message.Title,
                Content = StripHtmlTags(message.Content),
                message.Tags
            });
        }

        private string StripHtmlTags(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;
            return Regex.Replace(input, "<.*?>", string.Empty);
        }
    }
}
