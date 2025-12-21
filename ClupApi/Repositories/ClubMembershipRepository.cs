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

        public override async Task<IEnumerable<ClubMembership>> GetAllAsync()
        {
            return await _dbSet
                .Include(cm => cm.Club)
                .Include(cm => cm.Student)
                .ToListAsync();
        }

        public override async Task<ClubMembership?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(cm => cm.Club)
                .Include(cm => cm.Student)
                .FirstOrDefaultAsync(cm => cm.MembershipID == id);
        }

        public async Task<IEnumerable<ClubMembership>> GetByStudentIdAsync(int studentId)
        {
            // İlişkili verileri dahil et
            return await _dbSet
                .Include(cm => cm.Club)
                .Include(cm => cm.Student)
                .Where(cm => cm.StudentID == studentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClubMembership>> GetByClubIdAsync(int clubId)
        {
            // İlişkili verileri dahil et
            return await _dbSet
                .Include(cm => cm.Club)
                .Include(cm => cm.Student)
                .Where(cm => cm.ClubID == clubId)
                .ToListAsync();
        }

        public async Task<ClubMembership?> GetMembershipAsync(int studentId, int clubId)
        {
            // Öğrenci ve kulüp eşleşmesini bul
            return await _dbSet
                .Include(cm => cm.Club)
                .Include(cm => cm.Student)
                .Where(cm => cm.StudentID == studentId && cm.ClubID == clubId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ClubMembership>> GetApprovedMembershipsAsync(int clubId)
        {
            // Onaylanmış üyelikleri getir
            return await _dbSet
                .Include(cm => cm.Club)
                .Include(cm => cm.Student)
                .Where(cm => cm.ClubID == clubId && cm.IsApproved == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClubMembership>> GetPendingMembershipsAsync(int clubId)
        {
            // Bekleyen üyelik başvurularını getir
            return await _dbSet
                .Include(cm => cm.Club)
                .Include(cm => cm.Student)
                .Where(cm => cm.ClubID == clubId && cm.IsApproved == null)
                .ToListAsync();
        }
    }
}