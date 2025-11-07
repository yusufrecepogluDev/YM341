using ClupApi;
using ClupApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ClupApi.Repositories
{
    public interface IAnnouncementRepository
    {
        Task<List<Announcement>> GetAllAsync();
        Task<Announcement?> GetByIdAsync(int id);
        Task AddAsync(Announcement announcement);
        Task UpdateAsync(Announcement announcement);
        Task DeleteAsync(int id);
    }

    public class AnnouncementRepository : IAnnouncementRepository
    {
        private readonly AppDbContext _context;

        public AnnouncementRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Announcement>> GetAllAsync()
        {
            return await _context.Announcements
                .Where(a => !a.IsDeleted)
                .ToListAsync();
        }

        public async Task<Announcement?> GetByIdAsync(int id)
        {
            return await _context.Announcements
                .Where(a => a.AnnouncementID == id && !a.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(Announcement announcement)
        {
            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Announcement announcement)
        {
            _context.Announcements.Update(announcement);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement != null && !announcement.IsDeleted)
            {
                announcement.DeletionDate = DateTime.UtcNow;
                announcement.IsDeleted = true;
                _context.Announcements.Update(announcement);
                await _context.SaveChangesAsync();
            }
        }
    }
}
