using ClupApi.DTOs;
using ClupApi.Models;
using ClupApi.Repositories;
using ClupApi.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ClupApi.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAuthenticationRepository _repository;
        private readonly IConfiguration _config;

        public AuthenticationService(IAuthenticationRepository repository, IConfiguration config)
        {
            _repository = repository;
            _config = config;
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

            var token = GenerateToken(student.StudentNumber, "student");

            return new StudentLoginResponseDto
            {
                StudentID = student.StudentID,
                StudentName = student.StudentName,
                StudentSurname = student.StudentSurname,
                StudentMail = student.StudentMail,
                StudentNumber = student.StudentNumber,
                IsActive = student.IsActive,
                Token = token
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

            var token = GenerateToken(club.ClubNumber, "club");

            return new ClubLoginResponseDto
            {
                ClubID = club.ClubID,
                ClubName = club.ClubName,
                ClubNumber = club.ClubNumber,
                IsActive = club.IsActive,
                Token = token
            };
        }

        public async Task<bool> RegisterStudentAsync(StudentRegisterRequestDto request)
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

        public async Task<bool> RegisterClubAsync(ClubRegisterRequestDto request)
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
        public string GenerateToken(long userId, string userType)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
        new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
        new Claim("userType", userType), // "student" veya "club"
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var expirationMinutes = Convert.ToDouble(_config["JwtSettings:ExpiryInMinutes"] ?? "60");
            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
