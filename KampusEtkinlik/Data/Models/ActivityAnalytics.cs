namespace KampusEtkinlik.Data.Models
{
    public class ActivityAnalyticsDto
    {
        public int ActivityID { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public int TotalParticipants { get; set; }
        public int? ParticipantLimit { get; set; }
        public double OccupancyRate { get; set; }
        public double? AverageRating { get; set; }
        public int RatingCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ClubAnalyticsSummaryDto
    {
        public int ClubID { get; set; }
        public int TotalActivities { get; set; }
        public int TotalParticipants { get; set; }
        public int UpcomingActivities { get; set; }
        public int CompletedActivities { get; set; }
        public int OngoingActivities { get; set; }
        public double AverageOccupancyRate { get; set; }
        public double OverallAverageRating { get; set; }
        public int TotalRatings { get; set; }
        public List<ActivityAnalyticsDto> Activities { get; set; } = new();
        public List<MonthlyStatsDto> MonthlyStats { get; set; } = new();
    }

    public class MonthlyStatsDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int ActivityCount { get; set; }
        public int ParticipantCount { get; set; }
    }
}
