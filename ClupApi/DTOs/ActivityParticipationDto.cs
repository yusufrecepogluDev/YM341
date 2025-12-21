using System.ComponentModel.DataAnnotations;

namespace ClupApi.DTOs
{
    public class ActivityParticipationCreateDto
    {
        [Required(ErrorMessage = "Etkinlik ID'si zorunludur")]
        public int ActivityID { get; set; }

        [Required(ErrorMessage = "Öğrenci ID'si zorunludur")]
        public int StudentID { get; set; }

        public DateTime? JoinDate { get; set; }

        [Range(1, 5, ErrorMessage = "Puan 1-5 arasında olmalıdır")]
        public int? Rating { get; set; }
    }

    public class ActivityParticipationUpdateDto
    {
        [Range(1, 5, ErrorMessage = "Puan 1-5 arasında olmalıdır")]
        public int? Rating { get; set; }
    }

    public class RatingDto
    {
        [Required(ErrorMessage = "Puan zorunludur")]
        [Range(1, 5, ErrorMessage = "Puan 1-5 arasında olmalıdır")]
        public int Rating { get; set; }
    }

    public class ActivityParticipationResponseDto
    {
        public int ParticipationID { get; set; }
        public int ActivityID { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public DateTime ActivityStartDate { get; set; }
        public string ClubName { get; set; } = string.Empty;
        public int StudentID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentSurname { get; set; } = string.Empty;
        public long StudentNumber { get; set; }
        public DateTime? JoinDate { get; set; }
        public int? Rating { get; set; }
    }
}
