using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampusEtkinlik.Data.Models;


namespace KampusEtkinlik.Models
{
    [Table("Student")]
    public class Student
    {
        [Key]
        public int StudentID { get; set; }

        [Required]
        [MaxLength(50)]
        public string StudentName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string StudentSurname { get; set; } = string.Empty;

        public long StudentNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public string StudentMail { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string StudentPassword { get; set; } = string.Empty;

        public string? StudentStatus { get; set; }

        public bool IsActive { get; set; }


    }
}
