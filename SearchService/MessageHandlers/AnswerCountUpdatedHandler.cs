using Common.Contracts;
using SearchService.Models;
using Typesense;

namespace SearchService.MessageHandlers
{
    public class AnswerCountUpdatedHandler(ITypesenseClient typesenseClient)
    {
        public async Task HandleAsync(AnswerCountUpdatedEvent message)
        {
            await typesenseClient.UpdateDocument("questions", message.QuestionId,
                new
                {
                    answerscount = message.AnswersCount
                });
        }
    }
}
