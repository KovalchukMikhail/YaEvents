using System.ComponentModel.DataAnnotations;

namespace YaEvents.Data.Dto
{
    public class EventDto
    {
        public int Id { get; init; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
    }
}
