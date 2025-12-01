using System.ComponentModel.DataAnnotations;

namespace KampusEtkinlik.DTOs
{

    public class StudentLoginRequestDto
    {
        [Required(ErrorMessage = "Öğrenci numarası zorunludur")]
        public long StudentNumber { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur")]
        [MaxLength(64)]
        public string StudentPassword { get; set; } = string.Empty;
    }

    public class StudentLoginResponseDto
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentSurname { get; set; } = string.Empty;
        public string StudentMail { get; set; } = string.Empty;
        public long StudentNumber { get; set; }
        public bool IsActive { get; set; }
        public string Token { get; set; } = string.Empty;
    }

    public class StudentRegisterRequestDto
    {
        [Required(ErrorMessage = "Öğrenci adı zorunludur")]
        [MaxLength(50, ErrorMessage = "Öğrenci adı en fazla 50 karakter olabilir")]
        public string StudentName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Öğrenci soyadı zorunludur")]
        [MaxLength(50, ErrorMessage = "Öğrenci soyadı en fazla 50 karakter olabilir")]
        public string StudentSurname { get; set; } = string.Empty;
        [Required(ErrorMessage = "Öğrenci maili zorunludur")]
        [EmailAddress(ErrorMessage = "Geçersiz mail formatı")]
        public string StudentMail { get; set; } = string.Empty;
        [Required(ErrorMessage = "Öğrenci numarası zorunludur")]
        public long StudentNumber { get; set; }
        [Required(ErrorMessage = "Şifre zorunludur")]
        [MaxLength(64)]
        public string StudentPassword { get; set; } = string.Empty;
    }

    public class StudentRegisterResponseDto
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentSurname { get; set; } = string.Empty;
        public string StudentMail { get; set; } = string.Empty;
        public long StudentNumber { get; set; }
        public bool IsActive { get; set; }
    }

    public class ClubLoginRequestDto
    {
        [Required(ErrorMessage = "Kulüp numarası zorunludur")]
        public long ClubNumber { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur")]
        [MaxLength(64)]
        public string ClubPassword { get; set; } = string.Empty;
    }

    public class ClubLoginResponseDto
    {
        public int ClubID { get; set; }
        public string ClubName { get; set; } = string.Empty;
        public long ClubNumber { get; set; }
        public bool IsActive { get; set; }
        public string Token { get; set; } = string.Empty;
    }

    public class ClubRegisterRequestDto
    {
        [Required(ErrorMessage = "Kulüp adı zorunludur")]
        [MaxLength(100, ErrorMessage = "Kulüp adı en fazla 100 karakter olabilir")]
        public string ClubName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Kulüp numarası zorunludur")]
        public long ClubNumber { get; set; }
        [MaxLength(500, ErrorMessage = "Kulüp açıklaması en fazla 500 karakter olabilir")]
        public string? ClubDescription { get; set; }
        [EmailAddress(ErrorMessage = "Geçersiz mail formatı")]
        public string? ClubEmail { get; set; }
        [Required(ErrorMessage = "Şifre zorunludur")]
        [MaxLength(64)]
        public string ClubPassword { get; set; } = string.Empty;
    }

    public class ClubRegisterResponseDto
    {
        public int ClubID { get; set; }
        public string ClubName { get; set; } = string.Empty;
        public long ClubNumber { get; set; }
        public bool IsActive { get; set; }
    }
}
