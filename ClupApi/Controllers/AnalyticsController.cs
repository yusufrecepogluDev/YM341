using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClupApi.Models;
using ClupApi.Repositories.Interfaces;
using ClupApi.DTOs;

namespace ClupApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : BaseController
    {
        private readonly IActivityParticipationRepository _participationRepository;
        private readonly ClupApi.Repositories.IActivityRepository _activityRepository;

        public AnalyticsController(
            IActivityParticipationRepository participationRepository,
            ClupApi.Repositories.IActivityRepository activityRepository)
        {
            _participationRepository = participationRepository;
            _activityRepository = activityRepository;
        }

        /// <summary>
        /// Kulübün etkinlik istatistiklerini getirir
        /// </summary>
        [HttpGet("club/{clubId}")]
        [Authorize(Policy = "ClubOnly")]
        public async Task<IActionResult> GetClubAnalytics(int clubId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId != clubId)
                return Forbid();

            var allActivities = await _activityRepository.GetAllAsync();
            var clubActivities = allActivities
                .Where(a => a.OrganizingClubID == clubId && !a.IsDeleted)
                .ToList();

            var now = DateTime.UtcNow;
            var activityAnalytics = new List<ActivityAnalyticsDto>();

            int totalParticipants = 0;
            int totalRatings = 0;
            double totalRatingSum = 0;
            double totalOccupancySum = 0;
            int activitiesWithLimit = 0;

            foreach (var activity in clubActivities)
            {
                var participations = await _participationRepository.GetByActivityIdAsync(activity.ActivityID);
                var participantCount = participations.Count();
                var ratings = participations.Where(p => p.Rating.HasValue).Select(p => p.Rating!.Value).ToList();

                var status = GetActivityStatus(activity.StartDate, activity.EndDate, now);
                var occupancyRate = activity.ParticipantLimit.HasValue && activity.ParticipantLimit > 0
                    ? Math.Round((double)participantCount / activity.ParticipantLimit.Value * 100, 1)
                    : 0;

                if (activity.ParticipantLimit.HasValue && activity.ParticipantLimit > 0)
                {
                    totalOccupancySum += occupancyRate;
                    activitiesWithLimit++;
                }

                totalParticipants += participantCount;
                totalRatings += ratings.Count;
                totalRatingSum += ratings.Sum();

                activityAnalytics.Add(new ActivityAnalyticsDto
                {
                    ActivityID = activity.ActivityID,
                    ActivityName = activity.ActivityName,
                    TotalParticipants = participantCount,
                    ParticipantLimit = activity.ParticipantLimit,
                    OccupancyRate = occupancyRate,
                    AverageRating = ratings.Any() ? Math.Round(ratings.Average(), 1) : null,
                    RatingCount = ratings.Count,
                    StartDate = activity.StartDate,
                    EndDate = activity.EndDate,
                    Status = status
                });
            }

            // Aylık istatistikler (son 6 ay)
            var monthlyStats = GetMonthlyStats(clubActivities, now);

            var summary = new ClubAnalyticsSummaryDto
            {
                ClubID = clubId,
                TotalActivities = clubActivities.Count,
                TotalParticipants = totalParticipants,
                UpcomingActivities = clubActivities.Count(a => a.StartDate > now),
                CompletedActivities = clubActivities.Count(a => a.EndDate < now),
                OngoingActivities = clubActivities.Count(a => a.StartDate <= now && a.EndDate >= now),
                AverageOccupancyRate = activitiesWithLimit > 0 ? Math.Round(totalOccupancySum / activitiesWithLimit, 1) : 0,
                OverallAverageRating = totalRatings > 0 ? Math.Round(totalRatingSum / totalRatings, 1) : 0,
                TotalRatings = totalRatings,
                Activities = activityAnalytics.OrderByDescending(a => a.StartDate).ToList(),
                MonthlyStats = monthlyStats
            };

            return Ok(ApiResponse<ClubAnalyticsSummaryDto>.SuccessResponse(summary));
        }

        /// <summary>
        /// Tek bir etkinliğin detaylı istatistiklerini getirir
        /// </summary>
        [HttpGet("activity/{activityId}")]
        [Authorize(Policy = "ClubOnly")]
        public async Task<IActionResult> GetActivityAnalytics(int activityId)
        {
            var activity = await _activityRepository.GetByIdAsync(activityId);
            if (activity == null)
                return NotFound(ApiResponse.ErrorResponse("Etkinlik bulunamadı", Array.Empty<string>()));

            var currentUserId = GetCurrentUserId();
            if (currentUserId != activity.OrganizingClubID)
                return Forbid();

            var participations = await _participationRepository.GetByActivityIdAsync(activityId);
            var participantCount = participations.Count();
            var ratings = participations.Where(p => p.Rating.HasValue).Select(p => p.Rating!.Value).ToList();

            var now = DateTime.UtcNow;
            var status = GetActivityStatus(activity.StartDate, activity.EndDate, now);
            var occupancyRate = activity.ParticipantLimit.HasValue && activity.ParticipantLimit > 0
                ? Math.Round((double)participantCount / activity.ParticipantLimit.Value * 100, 1)
                : 0;

            var analytics = new ActivityAnalyticsDto
            {
                ActivityID = activity.ActivityID,
                ActivityName = activity.ActivityName,
                TotalParticipants = participantCount,
                ParticipantLimit = activity.ParticipantLimit,
                OccupancyRate = occupancyRate,
                AverageRating = ratings.Any() ? Math.Round(ratings.Average(), 1) : null,
                RatingCount = ratings.Count,
                StartDate = activity.StartDate,
                EndDate = activity.EndDate,
                Status = status
            };

            return Ok(ApiResponse<ActivityAnalyticsDto>.SuccessResponse(analytics));
        }

        private string GetActivityStatus(DateTime startDate, DateTime endDate, DateTime now)
        {
            if (now < startDate) return "Yaklaşan";
            if (now >= startDate && now <= endDate) return "Devam Eden";
            return "Tamamlandı";
        }

        private List<MonthlyStatsDto> GetMonthlyStats(List<Activity> activities, DateTime now)
        {
            var stats = new List<MonthlyStatsDto>();
            var turkishMonths = new[] { "", "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", 
                                        "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };

            for (int i = 5; i >= 0; i--)
            {
                var targetDate = now.AddMonths(-i);
                var monthActivities = activities.Where(a => 
                    a.StartDate.Year == targetDate.Year && 
                    a.StartDate.Month == targetDate.Month).ToList();

                stats.Add(new MonthlyStatsDto
                {
                    Year = targetDate.Year,
                    Month = targetDate.Month,
                    MonthName = turkishMonths[targetDate.Month],
                    ActivityCount = monthActivities.Count,
                    ParticipantCount = monthActivities.Sum(a => a.NumberOfParticipants ?? 0)
                });
            }

            return stats;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
