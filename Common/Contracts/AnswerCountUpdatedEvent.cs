using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Contracts
{
    public record AnswerCountUpdatedEvent(string QuestionId, int AnswersCount);
}
