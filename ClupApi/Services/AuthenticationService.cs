using ClupApi.DTOs;
using ClupApi.Models;
using ClupApi.Repositories;
using ClupApi.Repositories.Interfaces;

namespace ClupApi.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAuthenticationRepository _repository;

        public AuthenticationService(IAuthenticationRepository repository)
        {
            _repository = repository;
        }

        public async Task<StudentLoginResponseDto?> AuthenticateStudentAsync(StudentLoginRequestDto request)
        {
            var student = await _repository.GetStudentByNumberAsync(request.StudentNumber);

            if (student == null || 
                student.StudentPassword != request.StudentPassword || 
                !student.IsActive)
            {
                return null;
            }

            return new StudentLoginResponseDto
            {
                StudentID = student.StudentID,
                StudentName = student.StudentName,
                StudentSurname = student.StudentSurname,
                StudentMail = student.StudentMail,
                StudentNumber = student.StudentNumber,
                IsActive = student.IsActive
            };
        }

        public async Task<ClubLoginResponseDto?> AuthenticateClubAsync(ClubLoginRequestDto request)
        {
            var club = await _repository.GetClubByNumberAsync(request.ClubNumber);

            if (club == null || 
                club.ClubPassword != request.ClubPassword || 
                !club.IsActive)
            {
                return null;
            }

            return new ClubLoginResponseDto
            {
                ClubID = club.ClubID,
                ClubName = club.ClubName,
                ClubNumber = club.ClubNumber,
                IsActive = club.IsActive
            };
        }

        public async Task<bool> SinginStudentAsync(StudentSinginRequestDto request)
        {
            // Check if student number already exists
            var existingStudentByNumber = await _repository.GetStudentByNumberAsync(request.StudentNumber);
            if (existingStudentByNumber != null)
            {
                return false;
            }

            // Check if email already exists
            var existingStudentByMail = await _repository.GetStudentByMailAsync(request.StudentMail);
            if (existingStudentByMail != null)
            {
                return false;
            }

            var student = new Student
            {
                StudentName = request.StudentName,
                StudentSurname = request.StudentSurname,
                StudentMail = request.StudentMail,
                StudentNumber = request.StudentNumber,
                StudentPassword = request.StudentPassword,
                IsActive = true
            };

            return await _repository.AddStudentAsync(student);
        }

        public async Task<bool> SinginClubAsync(ClubSinginRequestDto request)
        {
            // Check if club number already exists
            var existingClub = await _repository.GetClubByNumberAsync(request.ClubNumber);
            if (existingClub != null)
            {
                return false;
            }

            var club = new Club
            {
                ClubName = request.ClubName,
                ClubNumber = request.ClubNumber,
                ClubPassword = request.ClubPassword,
                IsActive = true
            };

            return await _repository.AddClubAsync(club);
        }
    }
}
