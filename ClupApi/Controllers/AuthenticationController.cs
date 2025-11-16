using ClupApi.DTOs;
using ClupApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClupApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : BaseController
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
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

        [HttpPost("student/singin")]
        public async Task<IActionResult> StudentSingin([FromBody] StudentSinginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return HandleValidationErrors(ModelState);
            }

            var success = await _authenticationService.SinginStudentAsync(request);

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

        [HttpPost("club/singin")]
        public async Task<IActionResult> ClubSingin([FromBody] ClubSinginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return HandleValidationErrors(ModelState);
            }
            var success = await _authenticationService.SinginClubAsync(request);
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
    }
}
