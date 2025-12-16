using ClupApi.Models;
using ClupApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClupApi.Repositories
{
    public class ActivityParticipationRepository : BaseRepository<ActivityParticipation>, IActivityParticipationRepository
    {
        public ActivityParticipationRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ActivityParticipation>> GetByStudentIdAsync(int studentId)
        {
            return await _dbSet
                .Include(ap => ap.Activity)
                .Include(ap => ap.Student)
                .Where(ap => ap.StudentID == studentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ActivityParticipation>> GetByActivityIdAsync(int activityId)
        {
            return await _dbSet
                .Include(ap => ap.Activity)
                .Include(ap => ap.Student)
                .Where(ap => ap.ActivityID == activityId)
                .ToListAsync();
        }

        public async Task<ActivityParticipation?> GetParticipationAsync(int studentId, int activityId)
        {
            return await _dbSet
                .Include(ap => ap.Activity)
                .Include(ap => ap.Student)
                .Where(ap => ap.StudentID == studentId && ap.ActivityID == activityId)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetParticipantCountAsync(int activityId)
        {
            return await _dbSet
                .Where(ap => ap.ActivityID == activityId)
                .CountAsync();
        }
    }
}