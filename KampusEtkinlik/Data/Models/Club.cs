using KampusEtkinlik.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KampusEtkinlik.Models
{
    [Table("Club")]
    public class Club
    {
        [Key]
        public int ClubID { get; set; }

        [Required]
        [MaxLength(100)]
        public string ClubName { get; set; } = string.Empty;
        [Required]
        [MaxLength(500)]
        public string _Description { get; set; } = string.Empty;

        public long ClubNumber { get; set; }

        [Required]
        [MaxLength(20)]
        public string ClubPassword { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public ICollection<Activity> Activities { get; set; } = new List<Activity>();
        public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
    }
}
