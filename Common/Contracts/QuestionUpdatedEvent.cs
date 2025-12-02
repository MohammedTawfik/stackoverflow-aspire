namespace Common.Contracts
{
    public record QuestionUpdatedEvent(string Id, string Title, string Content, List<string> Tags);
}
