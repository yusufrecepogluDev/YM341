using System.ComponentModel.DataAnnotations;

namespace ClupApi.DTOs
{
    public class ActivityCreateDto
    {
        [Required(ErrorMessage = "Etkinlik adı zorunludur")]
        [MaxLength(100, ErrorMessage = "Etkinlik adı en fazla 100 karakter olabilir")]
        public string ActivityName { get; set; } = string.Empty;

        [MaxLength(4000, ErrorMessage = "Açıklama en fazla 4000 karakter olabilir")]
        public string? ActivityDescription { get; set; }

        [Required(ErrorMessage = "Düzenleyen kulüp ID'si zorunludur")]
        public int OrganizingClubID { get; set; }

        [Required(ErrorMessage = "Başlangıç tarihi zorunludur")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Bitiş tarihi zorunludur")]
        public DateTime EndDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Katılımcı limiti pozitif bir sayı olmalıdır")]
        public int? ParticipantLimit { get; set; }

        public DateTime? EvaluationStartDate { get; set; }

        public DateTime? EvaluationEndDate { get; set; }
    }

    public class ActivityUpdateDto
    {
        [Required(ErrorMessage = "Etkinlik adı zorunludur")]
        [MaxLength(100, ErrorMessage = "Etkinlik adı en fazla 100 karakter olabilir")]
        public string ActivityName { get; set; } = string.Empty;

        [MaxLength(4000, ErrorMessage = "Açıklama en fazla 4000 karakter olabilir")]
        public string? ActivityDescription { get; set; }

        [Required(ErrorMessage = "Başlangıç tarihi zorunludur")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Bitiş tarihi zorunludur")]
        public DateTime EndDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Katılımcı limiti pozitif bir sayı olmalıdır")]
        public int? ParticipantLimit { get; set; }

        public DateTime? EvaluationStartDate { get; set; }

        public DateTime? EvaluationEndDate { get; set; }
    }

    public class ActivityResponseDto
    {
        public int ActivityID { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public string? ActivityDescription { get; set; }
        public int OrganizingClubID { get; set; }
        public string OrganizingClubName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? ParticipantLimit { get; set; }
        public int? NumberOfParticipants { get; set; }
        public DateTime? EvaluationStartDate { get; set; }
        public DateTime? EvaluationEndDate { get; set; }
        public DateTime? CreationDate { get; set; }
        public bool IsActive { get; set; }
    }
}