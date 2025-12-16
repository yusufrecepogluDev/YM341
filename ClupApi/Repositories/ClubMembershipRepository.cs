using ClupApi.Models;
using ClupApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClupApi.Repositories
{
    public class ClubMembershipRepository : BaseRepository<ClubMembership>, IClubMembershipRepository
    {
        public ClubMembershipRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ClubMembership>> GetByStudentIdAsync(int studentId)
        {
            return await _dbSet
                .Include(cm => cm.Club)
                .Include(cm => cm.Student)
                .Where(cm => cm.StudentID == studentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClubMembership>> GetByClubIdAsync(int clubId)
        {
            return await _dbSet
                .Include(cm => cm.Club)
                .Include(cm => cm.Student)
                .Where(cm => cm.ClubID == clubId)
                .ToListAsync();
        }

        public async Task<ClubMembership?> GetMembershipAsync(int studentId, int clubId)
        {
            return await _dbSet
                .Include(cm => cm.Club)
                .Include(cm => cm.Student)
                .Where(cm => cm.StudentID == studentId && cm.ClubID == clubId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ClubMembership>> GetApprovedMembershipsAsync(int clubId)
        {
            return await _dbSet
                .Include(cm => cm.Club)
                .Include(cm => cm.Student)
                .Where(cm => cm.ClubID == clubId && cm.IsApproved == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClubMembership>> GetPendingMembershipsAsync(int clubId)
        {
            return await _dbSet
                .Include(cm => cm.Club)
                .Include(cm => cm.Student)
                .Where(cm => cm.ClubID == clubId && cm.IsApproved == null)
                .ToListAsync();
        }
    }
}