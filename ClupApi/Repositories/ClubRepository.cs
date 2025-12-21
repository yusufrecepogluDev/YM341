using ClupApi.Models;
using ClupApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClupApi.Repositories
{
    public class ClubRepository : BaseRepository<Club>, IClubRepository
    {
        public ClubRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Club>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.ClubMemberships)
                .Include(c => c.Activities)
                .Include(c => c.Announcements)
                .ToListAsync();
        }

        public override async Task<Club?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(c => c.ClubMemberships)
                .Include(c => c.Activities)
                .Include(c => c.Announcements)
                .FirstOrDefaultAsync(c => c.ClubID == id);
        }

        public async Task<Club?> GetByClubNumberAsync(long clubNumber)
        {
            return await _dbSet
                .Where(c => c.ClubNumber == clubNumber)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Club>> GetActiveClubsAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive)
                .ToListAsync();
        }
    }
}