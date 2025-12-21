using System.ComponentModel.DataAnnotations;

namespace ClupApi.DTOs
{
    public class StudentCreateDto
    {
        [Required(ErrorMessage = "Öğrenci adı zorunludur")]
        [MaxLength(50, ErrorMessage = "Öğrenci adı en fazla 50 karakter olabilir")]
        public string StudentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Öğrenci soyadı zorunludur")]
        [MaxLength(50, ErrorMessage = "Öğrenci soyadı en fazla 50 karakter olabilir")]
        public string StudentSurname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Öğrenci numarası zorunludur")]
        public long StudentNumber { get; set; }

        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [MaxLength(50, ErrorMessage = "E-posta en fazla 50 karakter olabilir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string StudentMail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur")]
        [MaxLength(20, ErrorMessage = "Şifre en fazla 20 karakter olabilir")]
        public string StudentPassword { get; set; } = string.Empty;

        public string? StudentStatus { get; set; }
    }

    public class StudentUpdateDto
    {
        [Required(ErrorMessage = "Öğrenci adı zorunludur")]
        [MaxLength(50, ErrorMessage = "Öğrenci adı en fazla 50 karakter olabilir")]
        public string StudentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Öğrenci soyadı zorunludur")]
        [MaxLength(50, ErrorMessage = "Öğrenci soyadı en fazla 50 karakter olabilir")]
        public string StudentSurname { get; set; } = string.Empty;

        [MaxLength(50, ErrorMessage = "E-posta en fazla 50 karakter olabilir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string? StudentMail { get; set; }

        [MaxLength(20, ErrorMessage = "Şifre en fazla 20 karakter olabilir")]
        public string? StudentPassword { get; set; }

        public string? StudentStatus { get; set; }

        public bool? IsActive { get; set; }
    }

    public class StudentResponseDto
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentSurname { get; set; } = string.Empty;
        public long StudentNumber { get; set; }
        public string StudentMail { get; set; } = string.Empty;
        public string? StudentStatus { get; set; }
        public bool IsActive { get; set; }
        public int ClubMembershipCount { get; set; }
        public int ActivityParticipationCount { get; set; }
    }
}
