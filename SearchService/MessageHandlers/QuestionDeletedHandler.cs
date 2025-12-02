using Common.Contracts;
using SearchService.Models;
using Typesense;

namespace SearchService.MessageHandlers
{
    public class QuestionDeletedHandler(ITypesenseClient typesenseClient)
    {
        public async Task HandleAsync(QuestionDeletedEvent message) 
        {
            await typesenseClient.DeleteDocument<QuestionSearchInfo>("questions", message.Id);
        }
    }
}
