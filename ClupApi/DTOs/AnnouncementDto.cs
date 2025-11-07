using System.ComponentModel.DataAnnotations;

namespace ClupApi.DTOs
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

        [Required]
        public DateTime StartDate { get; set; }
    }

    public class AnnouncementUpdateDto
    {
        [Required]
        [MaxLength(100)]
        public string AnnouncementTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(4000)]
        public string AnnouncementContent { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }
    }

    public class AnnouncementResponseDto
    {
        public int AnnouncementID { get; set; }
        public string AnnouncementTitle { get; set; } = string.Empty;
        public string AnnouncementContent { get; set; } = string.Empty;
        public int ClubID { get; set; }
        public string ClubName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? CreationDate { get; set; }
        public bool IsActive { get; set; }
    }
}