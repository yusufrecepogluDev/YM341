using System.ComponentModel.DataAnnotations;

namespace ClupApi.DTOs
{
    public class ClubCreateDto
    {
        [Required(ErrorMessage = "Kulüp adı zorunludur")]
        [MaxLength(100, ErrorMessage = "Kulüp adı en fazla 100 karakter olabilir")]
        public string ClubName { get; set; } = string.Empty;
        [Required(ErrorMessage ="Açıklama zorunludur")]
        [MaxLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kulüp numarası zorunludur")]
        public long ClubNumber { get; set; }

        [Required(ErrorMessage = "Kulüp şifresi zorunludur")]
        [MaxLength(20, ErrorMessage = "Şifre en fazla 20 karakter olabilir")]
        public string ClubPassword { get; set; } = string.Empty;
    }

    public class ClubUpdateDto
    {
        [Required(ErrorMessage = "Kulüp adı zorunludur")]
        [MaxLength(100, ErrorMessage = "Kulüp adı en fazla 100 karakter olabilir")]
        public string ClubName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Açıklama zorunludur")]
        [MaxLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string Description { get; set; } = string.Empty;

        [MaxLength(20, ErrorMessage = "Şifre en fazla 20 karakter olabilir")]
        public string? ClubPassword { get; set; }

        public bool? IsActive { get; set; }
    }

    public class ClubResponseDto
    {
        public int ClubID { get; set; }
        public string ClubName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public long ClubNumber { get; set; }
        public bool IsActive { get; set; }
        public int MemberCount { get; set; }
        public int AnnouncementCount { get; set; }
        public int ActivityCount { get; set; }
    }
}
