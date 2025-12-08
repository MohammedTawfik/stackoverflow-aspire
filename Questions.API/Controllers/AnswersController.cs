using Common.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Questions.API.Data;
using Questions.API.Data.Entities;
using Questions.API.DTOs;
using System.Security.Claims;
using Wolverine;

namespace Questions.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnswersController(QuestionsDBContext questionsDBContext, IMessageBus messageBus) : ControllerBase
    {
        [Authorize]
        [HttpPost("{questionId}")]
        public async Task<IActionResult> AddAnswer(string questionId, AnswerDTO answer)
        {
            var question = await questionsDBContext.Questions.FindAsync(questionId);
            if (question == null)
            {
                return NotFound(
                    new { message = "Question not found." }
                );
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue("name");
            if (userName is null || userId is null)
            {
                return BadRequest("Invalid user.");
            }

            var dbAnswer = new Answer
            {
                Content = answer.Content,
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                UserName = userName,
                QuestionId = questionId
            };
            question.Answers.Add(dbAnswer);
            question.AnswersCount += 1;
            await questionsDBContext.SaveChangesAsync();

            await messageBus.PublishAsync(new AnswerCountUpdatedEvent(questionId, question.AnswersCount));
            return Created($"questions/{questionId}", dbAnswer);
        }

        [Authorize]
        [HttpPut("{answerId}")]
        public async Task<IActionResult> UpdateAnswer(string answerId, AnswerDTO answer)
        {
            var existingAnswer = await questionsDBContext.Answers.FindAsync(answerId);
            if (existingAnswer == null)
            {
                return NotFound(new { message = "Answer not found." });
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null || userId != existingAnswer.UserId)
            {
                {
                    return BadRequest("Invalid user.");
                }
            }
            existingAnswer.Content = answer.Content;
            existingAnswer.UpdatedAt = DateTime.UtcNow;
            await questionsDBContext.SaveChangesAsync();
            return Ok(existingAnswer);
        }

        [Authorize]
        [HttpDelete("{questionId}/{answerId}")]
        public async Task<IActionResult> DeleteAnswer(string questionId, string answerId)
        {
            var existingAnswer = await questionsDBContext.Answers.FindAsync(answerId);
            if (existingAnswer == null)
            {
                return NotFound(new { message = "Answer not found." });
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null || userId != existingAnswer.UserId)
            {
                {
                    return BadRequest("Invalid user.");
                }
            }
            var question = await questionsDBContext.Questions.FindAsync(existingAnswer.QuestionId);
            if (question != null)
            {
                question.AnswersCount -= 1;
            }
            questionsDBContext.Answers.Remove(existingAnswer);
            await questionsDBContext.SaveChangesAsync();
            if (question != null)
            {
                await messageBus.PublishAsync(new AnswerCountUpdatedEvent(question.Id, question.AnswersCount));
            }
            return NoContent();
        }


        [Authorize]
        [HttpPost("{questionId}/answers/{answerId}/accept")]
        public async Task<IActionResult> AcceptAnswer(string questionId, string answerId)
        {
            var question = await questionsDBContext.Questions.FindAsync(questionId);
            var answer = await questionsDBContext.Answers.FindAsync(answerId);
            if (question is null || answer is null)
            {
                return NotFound(new { message = "Question or Answer not found." });
            }
            if (answer.QuestionId != questionId)
            {
                return BadRequest(new { message = "Answer does not belong to the specified question." });
            }
            if (question.HasAcceptedAnswer)
            {
                return BadRequest(new { message = "Question already has an accepted answer." });
            }
            answer.Accepted = true;
            question.HasAcceptedAnswer = true;
            await questionsDBContext.SaveChangesAsync();
            await messageBus.PublishAsync(new AnswerAcceptedEvent(questionId));
            return Ok(answer);
        }
    }
}
