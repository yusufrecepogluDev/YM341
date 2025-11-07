namespace KampusEtkinlik.Data.DTOs
{
    public class ActivityUpdateDto
    {
        public string ActivityName { get; set; } = string.Empty;
        public string? ActivityDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? ParticipantLimit { get; set; }
        public DateTime? EvaluationStartDate { get; set; }
        public DateTime? EvaluationEndDate { get; set; }
    }
}
