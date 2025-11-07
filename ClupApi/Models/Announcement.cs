using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClupApi.Models
{
    [Table("Announcement")]

    public class Announcement
    {
        [Key]
        public int AnnouncementID { get; set; }

        [Required]
        [MaxLength(100)]
        public string AnnouncementTitle { get; set; }

        [Required]
        [MaxLength(4000)]
        public string AnnouncementContent { get; set; }

        [ForeignKey("Club")]
        public int ClubID { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? DeletionDate { get; set; }

        public DateTime? CreationDate { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsActive { get; set; }

        public Club Club { get; set; }
    }
}
