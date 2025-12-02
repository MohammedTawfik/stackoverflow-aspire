using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Questions.API.Data;
using Questions.API.Data.Entities;

namespace Questions.API.Services
{
    public class TagsService(IMemoryCache memoryCache , QuestionsDBContext questionsDBContext)
    {
        public async Task<List<Tag>> GetAllTagsAsync()
        {
            return await memoryCache.GetOrCreateAsync("tags", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                var tags = await questionsDBContext.Tags.AsNoTracking().ToListAsync();
                return tags;
            }) ?? [];
        }

        public async Task<bool> AreTagsValidAsync(List<string> slugs)
        {
            var tags = await GetAllTagsAsync();
            var tagsSet = tags.Select(t => t.Slug).ToHashSet(StringComparer.OrdinalIgnoreCase);
            return slugs.All(slug => tagsSet.Contains(slug));
        }
    }
}
