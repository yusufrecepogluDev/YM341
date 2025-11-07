using ClupApi;
using ClupApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ClupApi.Repositories
{
    public interface IActivityRepository
    {
        Task<List<Activity>> GetAllAsync();
        Task<Activity?> GetByIdAsync(int id);
        Task AddAsync(Activity activity);
        Task UpdateAsync(Activity activity);
        Task DeleteAsync(int id);
    }

    public class ActivityRepository : IActivityRepository
    {
        private readonly AppDbContext _context;

        public ActivityRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Activity>> GetAllAsync()
        {
            return await _context.Activity
                .Where(a => !a.IsDeleted)
                .ToListAsync();
        }

        public async Task<Activity?> GetByIdAsync(int id)
        {
            return await _context.Activity
                .Where(a => a.ActivityID == id && !a.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(Activity activity)
        {
            _context.Activity.Add(activity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Activity activity)
        {
            _context.Activity.Update(activity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var activity = await _context.Activity.FindAsync(id);
            if (activity != null && !activity.IsDeleted)
            {
                activity.DeletionDate = DateTime.UtcNow;
                activity.IsDeleted = true;
                _context.Activity.Update(activity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
