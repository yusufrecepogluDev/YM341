using ClupApi.DTOs;

namespace ClupApi.Repositories.Interfaces
{
    public interface IAuthenticationService
    {
        Task<StudentLoginResponseDto?> AuthenticateStudentAsync(StudentLoginRequestDto request);
        Task<ClubLoginResponseDto?> AuthenticateClubAsync(ClubLoginRequestDto request);
        Task<bool> RegisterStudentAsync(StudentRegisterRequestDto request);
        Task<bool> RegisterClubAsync(ClubRegisterRequestDto request);
    }
}
