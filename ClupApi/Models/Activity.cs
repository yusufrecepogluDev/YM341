using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClupApi.Models
{
    [Table("Activity")]
    public class Activity
    {
        [Key]
        public int ActivityID { get; set; }

        [Required]
        [MaxLength(100)]
        public string ActivityName { get; set; }

        public string? ActivityDescription { get; set; }

        [ForeignKey("OrganizingClub")]
        public int OrganizingClubID { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int? ParticipantLimit { get; set; }

        public int? NumberOfParticipants { get; set; }

        public DateTime? EvaluationStartDate { get; set; }

        public DateTime? EvaluationEndDate { get; set; }

        public DateTime? CreationDate { get; set; }

        public DateTime? DeletionDate { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsActive { get; set; }

        [MaxLength(200)]
        public string? ImagePath { get; set; }

        public Club OrganizingClub { get; set; }
        public ICollection<ActivityParticipation> ActivityParticipations { get; set; } = new List<ActivityParticipation>();

    }
}
