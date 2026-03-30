using System.ComponentModel.DataAnnotations;

namespace YaEvents.Data.Dto
{
    public class EventDtoLite
    {
        [Required(ErrorMessage = "Название обязательно для заполнения.")]
        public required string Title { get; set; }
        public required string Description { get; set; }
        [Required(ErrorMessage = "Дата начала события обязательна для заполнения")]
        [Range(typeof(DateTime), "2000-01-01", "2100-12-31", ErrorMessage ="Некорректная дата начала события")]
        public DateTime StartAt { get; set; }
        [Required(ErrorMessage = "Дата окончания события обязательна для заполнения")]
        [Range(typeof(DateTime), "2000-01-01", "2100-12-31", ErrorMessage = "Некорректная дата окончания события")]
        public DateTime EndAt { get; set; }
    }
}
