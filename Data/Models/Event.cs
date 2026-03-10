namespace YaEvents.Data.Models
{
    public class Event
    {
        public int Id { get; init; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
    }
}
