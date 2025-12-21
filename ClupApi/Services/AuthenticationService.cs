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
        private readonly ITokenService _tokenService;
        private readonly ISecurityLogger _securityLogger;

        public AuthenticationService(
            IAuthenticationRepository repository, 
            IConfiguration config,
            ITokenService tokenService,
            ISecurityLogger securityLogger)
        {
            _repository = repository;
            _config = config;
            _tokenService = tokenService;
            _securityLogger = securityLogger;
        }

        public async Task<StudentLoginResponseDto?> AuthenticateStudentAsync(StudentLoginRequestDto request)
        {
            var student = await _repository.GetStudentByNumberAsync(request.StudentNumber);

            if (student == null ||
                student.StudentPassword != request.StudentPassword ||
                !student.IsActive)
            {
                _securityLogger.LogFailedLogin(request.StudentNumber.ToString(), "unknown");
                return null;
            }

            // Use TokenService for token generation
            var accessToken = _tokenService.GenerateAccessToken(student.StudentID, "student", student.StudentNumber.ToString());
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(student.StudentID, "student");

            _securityLogger.LogSuccessfulLogin(student.StudentNumber.ToString(), "unknown", "student");

            return new StudentLoginResponseDto
            {
                StudentID = student.StudentID,
                StudentName = student.StudentName,
                StudentSurname = student.StudentSurname,
                StudentMail = student.StudentMail,
                StudentNumber = student.StudentNumber,
                IsActive = student.IsActive,
                Token = accessToken,
                RefreshToken = refreshToken.Token
            };
        }

        public async Task<ClubLoginResponseDto?> AuthenticateClubAsync(ClubLoginRequestDto request)
        {
            var club = await _repository.GetClubByNumberAsync(request.ClubNumber);

            if (club == null ||
                club.ClubPassword != request.ClubPassword ||
                !club.IsActive)
            {
                _securityLogger.LogFailedLogin(request.ClubNumber.ToString(), "unknown");
                return null;
            }

            // Use TokenService for token generation
            var accessToken = _tokenService.GenerateAccessToken(club.ClubID, "club", club.ClubNumber.ToString());
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(club.ClubID, "club");

            _securityLogger.LogSuccessfulLogin(club.ClubNumber.ToString(), "unknown", "club");

            return new ClubLoginResponseDto
            {
                ClubID = club.ClubID,
                ClubName = club.ClubName,
                ClubNumber = club.ClubNumber,
                IsActive = club.IsActive,
                Token = accessToken,
                RefreshToken = refreshToken.Token
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
