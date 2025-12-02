namespace Common.Contracts
{
    public record QuestionCreatedEvent(string id, string title, string content, List<string> tags, DateTime createdAt);
    
}
