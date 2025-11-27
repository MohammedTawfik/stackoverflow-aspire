using System.ComponentModel.DataAnnotations;

namespace Questions.API.Data.Entities
{
    public class Tag
    {
        [MaxLength(36)]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [MaxLength(50)]
        public string Name { get; set; } = Guid.NewGuid().ToString();
        [MaxLength(50)]
        public string Slug { get; set; } = Guid.NewGuid().ToString();
        [MaxLength(1000)]
        public string Description { get; set; } = Guid.NewGuid().ToString();
    }
}
