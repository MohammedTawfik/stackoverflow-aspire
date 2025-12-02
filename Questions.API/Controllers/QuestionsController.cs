using Common.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Questions.API.Data;
using Questions.API.Data.Entities;
using Questions.API.DTOs;
using Questions.API.Services;
using System.Security.Claims;
using System.Threading.Tasks;
using Wolverine;

namespace Questions.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionsController(QuestionsDBContext _questionsDB, IMessageBus messageBus, TagsService tagsService) : ControllerBase
    {
        [HttpGet]
        public IActionResult Get(string? tag)
        {
            var questions = _questionsDB.Questions.AsQueryable();
            if (!string.IsNullOrEmpty(tag))
            {
                questions = questions.Where(q => q.Tags.Contains(tag));
            }
            return Ok(questions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Question>> GetQuestionById(string id)
        {
            var question = await _questionsDB.Questions.FindAsync(id);
            if (question == null)
            {
                return NotFound();
            }
            await _questionsDB.Questions.ExecuteUpdateAsync(q => q.SetProperty(q => q.ViewCount, q => q.ViewCount + 1));
            return Ok(question);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] DTOs.QuestionDto questionDto)
        {
            if (!await tagsService.AreTagsValidAsync(questionDto.Tags))
            {
                return BadRequest("One or more tags are invalid.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue("name");
            if (userId is null || userName is null)
            {
                return BadRequest("Cannot get user details");
            }
            var question = new Question
            {
                Title = questionDto.Title,
                Content = questionDto.Content,
                UserId = userId,
                UserDisplayName = userName,
                Tags = questionDto.Tags,
            };
            _questionsDB.Questions.Add(question);
            await _questionsDB.SaveChangesAsync();
            await messageBus.PublishAsync(new QuestionCreatedEvent
            (
                question.Id,
                question.Title,
                question.Content,
                question.Tags,
                question.CreatedAt
            ));
            return CreatedAtAction(nameof(Get), new { id = question.Id }, question);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] QuestionDto question)
        {
            var existingQuestion = _questionsDB.Questions.Find(id);
            if (existingQuestion == null)
            {
                return NotFound();
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (existingQuestion.UserId != userId)
            {
                return Forbid();
            }

            if (!await tagsService.AreTagsValidAsync(question.Tags))
            {
                return BadRequest("One or more tags are invalid.");
            }
            existingQuestion.Title = question.Title;
            existingQuestion.Content = question.Content;
            existingQuestion.UpdatedAt = DateTime.UtcNow;
            existingQuestion.Tags = question.Tags;
            _questionsDB.SaveChanges();
            await messageBus.PublishAsync(new QuestionUpdatedEvent
            (
                existingQuestion.Id,
                existingQuestion.Title,
                existingQuestion.Content,
                existingQuestion.Tags
            ));
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var question = _questionsDB.Questions.Find(id);
            if (question == null)
            {
                return NotFound();
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (question.UserId != userId)
            {
                return Forbid();
            }
            _questionsDB.Questions.Remove(question);
            _questionsDB.SaveChanges();
            await messageBus.PublishAsync(new QuestionDeletedEvent(id));
            return NoContent();
        }
    }
}
