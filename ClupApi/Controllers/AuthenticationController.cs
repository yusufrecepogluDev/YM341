using ClupApi.DTOs;
using ClupApi.Repositories.Interfaces;
using ClupApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClupApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : BaseController
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ITokenService _tokenService;
        private readonly ISecurityLogger _securityLogger;
        private readonly IValidationService _validationService;

        public AuthenticationController(
            IAuthenticationService authenticationService,
            ITokenService tokenService,
            ISecurityLogger securityLogger,
            IValidationService validationService)
        {
            _authenticationService = authenticationService;
            _tokenService = tokenService;
            _securityLogger = securityLogger;
            _validationService = validationService;
        }

        [HttpPost("student/login")]
        public async Task<IActionResult> StudentLogin([FromBody] StudentLoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return HandleValidationErrors(ModelState);
            }

            var result = await _authenticationService.AuthenticateStudentAsync(request);

            if (result == null)
            {
                return Unauthorized(Models.ApiResponse.ErrorResponse(
                    "Geçersiz kimlik bilgileri",
                    new[] { "Öğrenci numarası veya şifre hatalı" }));
            }

            return Ok(Models.ApiResponse<StudentLoginResponseDto>.SuccessResponse(
                result,
                "Giriş başarılı"));
        }

        [HttpPost("club/login")]
        public async Task<IActionResult> ClubLogin([FromBody] ClubLoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return HandleValidationErrors(ModelState);
            }

            var result = await _authenticationService.AuthenticateClubAsync(request);

            if (result == null)
            {
                return Unauthorized(Models.ApiResponse.ErrorResponse(
                    "Geçersiz kimlik bilgileri",
                    new[] { "Kulüp numarası veya şifre hatalı" }));
            }

            return Ok(Models.ApiResponse<ClubLoginResponseDto>.SuccessResponse(
                result,
                "Giriş başarılı"));
        }

        [HttpPost("student/register")]
        public async Task<IActionResult> StudentRegister([FromBody] StudentRegisterRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return HandleValidationErrors(ModelState);
            }

            // Security validation
            var validationErrors = new List<string>();
            var clientIp = GetClientIp();

            // Validate student number format (8-12 digits)
            var studentNumberValidation = _validationService.ValidateStudentNumber(request.StudentNumber.ToString());
            if (!studentNumberValidation.IsValid)
            {
                validationErrors.AddRange(studentNumberValidation.Errors);
            }

            // Validate email format
            var emailValidation = _validationService.ValidateEmail(request.StudentMail);
            if (!emailValidation.IsValid)
            {
                validationErrors.AddRange(emailValidation.Errors);
            }

            // Validate password strength
            var passwordValidation = _validationService.ValidatePassword(request.StudentPassword);
            if (!passwordValidation.IsValid)
            {
                validationErrors.AddRange(passwordValidation.Errors);
            }

            // Check for SQL injection patterns
            if (_validationService.ContainsSqlInjectionPatterns(request.StudentName) ||
                _validationService.ContainsSqlInjectionPatterns(request.StudentSurname) ||
                _validationService.ContainsSqlInjectionPatterns(request.StudentMail))
            {
                _securityLogger.LogSuspiciousInput(clientIp, "SQL injection pattern detected", "/api/auth/student/register");
                validationErrors.Add("Geçersiz karakter içeren giriş tespit edildi");
            }

            if (validationErrors.Count > 0)
            {
                return BadRequest(Models.ApiResponse.ValidationErrorResponse(validationErrors.ToArray()));
            }

            var success = await _authenticationService.RegisterStudentAsync(request);

            if (!success)
            {
                return BadRequest(Models.ApiResponse.ErrorResponse(
                    "Kayıt başarısız",
                    new[] { "Öğrenci numarası veya mail zaten kayıtlı" }));
            }

            return Ok(Models.ApiResponse<bool>.SuccessResponse(
                success,
                "Kayıt başarılı"));
        }

        [HttpPost("club/register")]
        public async Task<IActionResult> ClubRegister([FromBody] ClubRegisterRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return HandleValidationErrors(ModelState);
            }

            // Security validation
            var validationErrors = new List<string>();
            var clientIp = GetClientIp();

            // Validate password strength
            var passwordValidation = _validationService.ValidatePassword(request.ClubPassword);
            if (!passwordValidation.IsValid)
            {
                validationErrors.AddRange(passwordValidation.Errors);
            }

            // Check for SQL injection patterns
            if (_validationService.ContainsSqlInjectionPatterns(request.ClubName))
            {
                _securityLogger.LogSuspiciousInput(clientIp, "SQL injection pattern detected", "/api/auth/club/register");
                validationErrors.Add("Geçersiz karakter içeren giriş tespit edildi");
            }

            if (validationErrors.Count > 0)
            {
                return BadRequest(Models.ApiResponse.ValidationErrorResponse(validationErrors.ToArray()));
            }

            var success = await _authenticationService.RegisterClubAsync(request);
            if (!success)
            {
                return BadRequest(Models.ApiResponse.ErrorResponse(
                    "Kayıt başarısız",
                    new[] { "Kulüp numarası veya mail zaten kayıtlı" }));
            }
            return Ok(Models.ApiResponse<bool>.SuccessResponse(
                success,
                "Kayıt başarılı"));
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request)
        {
            if (!string.IsNullOrWhiteSpace(request?.SessionId))
            {
                // Sohbet geçmişini temizle
                Controllers.ChatController.ClearSessionData(request.SessionId);
            }

            // Revoke refresh token if provided
            if (!string.IsNullOrWhiteSpace(request?.RefreshToken))
            {
                await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken);
            }

            var userId = GetCurrentUserId();
            var clientIp = GetClientIp();
            _securityLogger.LogLogout(userId, clientIp);

            return Ok(Models.ApiResponse<bool>.SuccessResponse(
                true,
                "Çıkış başarılı"));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return HandleValidationErrors(ModelState);
            }

            // Validate refresh token
            var isValid = await _tokenService.ValidateRefreshTokenAsync(request.RefreshToken);
            if (!isValid)
            {
                return Unauthorized(Models.ApiResponse.ErrorResponse(
                    "Geçersiz veya süresi dolmuş refresh token",
                    new[] { "Lütfen tekrar giriş yapın" }));
            }

            // Get refresh token details
            var refreshToken = await _tokenService.GetRefreshTokenAsync(request.RefreshToken);
            if (refreshToken == null)
            {
                return Unauthorized(Models.ApiResponse.ErrorResponse(
                    "Refresh token bulunamadı",
                    new[] { "Lütfen tekrar giriş yapın" }));
            }

            // Generate new access token
            var newAccessToken = _tokenService.GenerateAccessToken(
                refreshToken.UserId, 
                refreshToken.UserType, 
                refreshToken.UserId.ToString());

            // Optionally rotate refresh token
            var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(
                refreshToken.UserId, 
                refreshToken.UserType);

            // Revoke old refresh token
            await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken);

            var clientIp = GetClientIp();
            _securityLogger.LogTokenRefresh(refreshToken.UserId, clientIp);

            return Ok(Models.ApiResponse<RefreshTokenResponseDto>.SuccessResponse(
                new RefreshTokenResponseDto
                {
                    Token = newAccessToken,
                    RefreshToken = newRefreshToken.Token
                },
                "Token yenilendi"));
        }

        private int GetCurrentUserId()
        {
            // Token'da "userId" claim'i kullanılıyor
            var userIdClaim = User.FindFirst("userId")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private string GetClientIp()
        {
            var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
