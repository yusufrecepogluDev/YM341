using ClupApi.Models;

namespace ClupApi.Repositories.Interfaces
{
    public interface IClubMembershipRepository : IBaseRepository<ClubMembership>
    {
        Task<IEnumerable<ClubMembership>> GetByStudentIdAsync(int studentId);
        Task<IEnumerable<ClubMembership>> GetByClubIdAsync(int clubId);
        Task<ClubMembership?> GetMembershipAsync(int studentId, int clubId);
        Task<IEnumerable<ClubMembership>> GetApprovedMembershipsAsync(int clubId);
        Task<IEnumerable<ClubMembership>> GetPendingMembershipsAsync(int clubId);
    }
}
