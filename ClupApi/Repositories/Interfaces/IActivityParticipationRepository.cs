using ClupApi.Models;

namespace ClupApi.Repositories.Interfaces
{
    public interface IActivityParticipationRepository : IBaseRepository<ActivityParticipation>
    {
        Task<IEnumerable<ActivityParticipation>> GetByStudentIdAsync(int studentId);
        Task<IEnumerable<ActivityParticipation>> GetByActivityIdAsync(int activityId);
        Task<ActivityParticipation?> GetParticipationAsync(int studentId, int activityId);
        Task<int> GetParticipantCountAsync(int activityId);
    }
}
