using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClupApi.Models
{
    [Table("Student")]
    public class Student
    {
        [Key]
        public int StudentID { get; set; }

        [Required]
        [MaxLength(50)]
        public string StudentName { get; set; }

        [Required]
        [MaxLength(50)]
        public string StudentSurname { get; set; }

        public long StudentNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public string StudentMail { get; set; }

        [Required]
        [MaxLength(20)]
        public string StudentPassword { get; set; }

        public string? StudentStatus { get; set; }

        public bool IsActive { get; set; }

        public ICollection<ActivityParticipation> ActivityParticipations { get; set; } = new List<ActivityParticipation>();
        public ICollection<ClubMembership> ClubMemberships { get; set; } = new List<ClubMembership>();

    }
}
