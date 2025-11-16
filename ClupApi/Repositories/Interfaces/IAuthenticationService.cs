using ClupApi.DTOs;

namespace ClupApi.Repositories.Interfaces
{
    public interface IAuthenticationService
    {
        Task<StudentLoginResponseDto?> AuthenticateStudentAsync(StudentLoginRequestDto request);
        Task<ClubLoginResponseDto?> AuthenticateClubAsync(ClubLoginRequestDto request);
        Task<bool> SinginStudentAsync(StudentSinginRequestDto request);
        Task<bool> SinginClubAsync(ClubSinginRequestDto request);
    }
}
