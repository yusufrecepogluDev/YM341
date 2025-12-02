using System.ComponentModel.DataAnnotations;

namespace KampusEtkinlik.Data.DTOs
{
    public class AnnouncementCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string AnnouncementTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(4000)]
        public string AnnouncementContent { get; set; } = string.Empty;

        [Required]
        public int ClubID { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Now;
    }
}
