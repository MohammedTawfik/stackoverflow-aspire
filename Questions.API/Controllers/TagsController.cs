using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Questions.API.Data;
using Questions.API.Data.Entities;
using System.Threading.Tasks;

namespace Questions.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagsController(QuestionsDBContext questionsDB) : ControllerBase
    {
        public async Task<ActionResult<List<Tag>>> Get()
        {
            var tags = await questionsDB.Tags.OrderBy(tag => tag.Name).ToListAsync();
            return Ok(tags);
        }
    }
}
