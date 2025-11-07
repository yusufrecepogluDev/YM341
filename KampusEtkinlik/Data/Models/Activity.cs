namespace KampusEtkinlik.Data.Models
{
    public class Activity
    {
        public int ActivityID { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public string? ActivityDescription { get; set; }
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

        // UI için yardımcı özellikler
        public string Title => ActivityName;
        public string Description => ActivityDescription ?? string.Empty;
    }
}
