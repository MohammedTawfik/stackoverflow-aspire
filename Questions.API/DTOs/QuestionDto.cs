using Questions.API.Validators;
using System.ComponentModel.DataAnnotations;

namespace Questions.API.DTOs
{
    public record QuestionDto
    (
        [Required]string Title,
        [Required]string Content,
        [TagsListValidator(1,5)]List<string> Tags
    );
}
