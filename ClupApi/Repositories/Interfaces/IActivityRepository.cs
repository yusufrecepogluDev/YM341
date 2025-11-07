using ClupApi.Models;

namespace ClupApi.Repositories.Interfaces
{
    public interface IActivityRepository : IBaseRepository<Activity>
    {
        Task<IEnumerable<Activity>> GetActiveActivitiesAsync();
        Task<IEnumerable<Activity>> GetByClubIdAsync(int clubId);
    }
}