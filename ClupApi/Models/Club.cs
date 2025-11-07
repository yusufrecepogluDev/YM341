using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClupApi.Models
{
    [Table("Club")]
    public class Club
    {
        [Key]
        public int ClubID { get; set; }

        [Required]
        [MaxLength(100)]
        public string ClubName { get; set; }

        public long ClubNumber { get; set; }

        [Required]
        [MaxLength(20)]
        public string ClubPassword { get; set; }

        public bool IsActive { get; set; }

        public ICollection<Activity> Activities { get; set; } = new List<Activity>();
        public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
        public ICollection<ClubMembership> ClubMemberships { get; set; } = new List<ClubMembership>();

    }
}
