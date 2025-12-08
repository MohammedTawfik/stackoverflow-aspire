using Common.Contracts;
using Typesense;

namespace SearchService.MessageHandlers
{
    public class AnswerAcceptedHandler(ITypesenseClient typesenseClient)
    {
        public async Task HandleAsync(AnswerAcceptedEvent message)
        {
            await typesenseClient.UpdateDocument("questions", message.questionId, new { hasacceptedanswer = true });
        }
    }
}
