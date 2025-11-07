using ClupApi.Models;

namespace ClupApi.Repositories.Interfaces
{
    public interface IAnnouncementRepository : IBaseRepository<Announcement>
    {
        Task<IEnumerable<Announcement>> GetActiveAnnouncementsAsync();
        Task<IEnumerable<Announcement>> GetByClubIdAsync(int clubId);
        Task<IEnumerable<Announcement>> GetActiveByClubIdAsync(int clubId);
    }
}