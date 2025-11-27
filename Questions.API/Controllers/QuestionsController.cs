using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Questions.API.Data;
using Questions.API.Data.Entities;
using Questions.API.DTOs;
using System.Security.Claims;

namespace Questions.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionsController : ControllerBase
    {
        private readonly QuestionsDBContext _questionsDB;
        public QuestionsController(QuestionsDBContext questionsDB)
        {
            _questionsDB = questionsDB;
        }

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
        public IActionResult Post([FromBody] DTOs.QuestionDto questionDto)
        {

            var validTags = _questionsDB.Tags.Where(t => questionDto.Tags.Contains(t.Slug)).Select(tag => tag.Slug).ToList();
            var missing = questionDto.Tags.Except(validTags).ToList();
            if (missing.Any())
            {
                return BadRequest($"The following tags are invalid: {string.Join(", ", missing)}");
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
            _questionsDB.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = question.Id }, question);
        }

        [Authorize]
        [HttpPut("{id}")]
        public IActionResult Put(string id,[FromBody] QuestionDto question)
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

            var validTags = _questionsDB.Tags.Where(t => question.Tags.Contains(t.Slug)).Select(tag => tag.Slug).ToList();
            var missing = question.Tags.Except(validTags).ToList();
            if (missing.Any())
            {
                return BadRequest($"The following tags are invalid: {string.Join(", ", missing)}");
            }
            existingQuestion.Title = question.Title;
            existingQuestion.Content = question.Content;
            existingQuestion.UpdatedAt = DateTime.UtcNow;
            existingQuestion.Tags = question.Tags;
            _questionsDB.SaveChanges();
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
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
            return NoContent();
        }
    }
}
