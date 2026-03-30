using System.ComponentModel.DataAnnotations;

namespace YaEvents.Data.Dto
{
    public record EventDto(int Id, string? Title, string? Description, DateTime StartAt, DateTime EndAt);
}
