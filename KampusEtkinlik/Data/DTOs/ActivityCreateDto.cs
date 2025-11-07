namespace KampusEtkinlik.Data.DTOs
{
    public class ActivityCreateDto
    {
        public string ActivityName { get; set; } = string.Empty;
        public string? ActivityDescription { get; set; }
        public int OrganizingClubID { get; set; } = 1; // Varsayılan kulüp ID
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(1);
        public int? ParticipantLimit { get; set; }
        public DateTime? EvaluationStartDate { get; set; }
        public DateTime? EvaluationEndDate { get; set; }
    }
}
