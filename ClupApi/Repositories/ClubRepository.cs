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