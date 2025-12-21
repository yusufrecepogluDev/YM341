using System.Net.Http.Json;
using System.Net.Http.Headers;
using KampusEtkinlik.Data.Models;
using KampusEtkinlik.Data.DTOs;
using Microsoft.Extensions.Configuration;

namespace KampusEtkinlik.Services
{
    public class ClubMembershipService
    {
        private readonly HttpClient _httpClient;
        private readonly TokenService _tokenService;
        private readonly string _baseUrl;

        public ClubMembershipService(HttpClient httpClient, TokenService tokenService, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _baseUrl = $"{configuration["ApiSettings:BaseUrl"]}/api/ClubMemberships";
        }

        private async Task SetAuthorizationHeaderAsync()
        {
            var token = await _tokenService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

     
    // Öğrencinin tüm üyeliklerini getirir
 
        public async Task<List<ClubMembershipResponseDto>> GetStudentMembershipsAsync(int studentId)
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                var url = $"{_baseUrl}/student/{studentId}";
                Console.WriteLine($"GetStudentMembershipsAsync - URL: {url}, StudentID: {studentId}");
                
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GetStudentMembershipsAsync - Status: {response.StatusCode}, Response: {content}");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<List<ClubMembershipResponseDto>>>(content, 
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return result?.Data ?? new List<ClubMembershipResponseDto>();
                }
                
                Console.WriteLine($"GetStudentMembershipsAsync - API hatası: {response.StatusCode}");
                return new List<ClubMembershipResponseDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API bağlantı hatası: {ex.Message}");
                return new List<ClubMembershipResponseDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Beklenmeyen hata: {ex.Message}");
                return new List<ClubMembershipResponseDto>();
            }
        }

     
    // Kulübün onaylanmış üyelerini getirir
 
        public async Task<List<ClubMembershipResponseDto>> GetClubMembersAsync(int clubId)
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<ClubMembershipResponseDto>>>($"{_baseUrl}/club/{clubId}");
                return result?.Data ?? new List<ClubMembershipResponseDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API bağlantı hatası: {ex.Message}");
                return new List<ClubMembershipResponseDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Beklenmeyen hata: {ex.Message}");
                return new List<ClubMembershipResponseDto>();
            }
        }

     
    // Kulübün bekleyen başvurularını getirir
 
        public async Task<List<ClubMembershipResponseDto>> GetPendingApplicationsAsync(int clubId)
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<ClubMembershipResponseDto>>>($"{_baseUrl}/club/{clubId}/pending");
                return result?.Data ?? new List<ClubMembershipResponseDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API bağlantı hatası: {ex.Message}");
                return new List<ClubMembershipResponseDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Beklenmeyen hata: {ex.Message}");
                return new List<ClubMembershipResponseDto>();
            }
        }

     
    // Kulübe üyelik başvurusu yapar
 
        public async Task<(bool Success, string Message)> ApplyToClubAsync(int clubId)
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                var applyDto = new ClubMembershipApplyDto { ClubID = clubId };
                var response = await _httpClient.PostAsJsonAsync(_baseUrl, applyDto);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<ClubMembershipResponseDto>>();
                    return (true, result?.Message ?? "Başvuru başarıyla gönderildi.");
                }
                else
                {
                    var errorResult = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                    return (false, errorResult?.Message ?? "Başvuru gönderilemedi.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Başvuru hatası: {ex.Message}");
                return (false, "Başvuru sırasında bir hata oluştu.");
            }
        }

     
    // Başvuruyu onaylar
 
        public async Task<(bool Success, string Message)> ApproveApplicationAsync(int membershipId)
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                var response = await _httpClient.PutAsync($"{_baseUrl}/{membershipId}/approve", null);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<ClubMembershipResponseDto>>();
                    return (true, result?.Message ?? "Başvuru onaylandı.");
                }
                else
                {
                    var errorResult = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                    return (false, errorResult?.Message ?? "Başvuru onaylanamadı.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Onaylama hatası: {ex.Message}");
                return (false, "Onaylama sırasında bir hata oluştu.");
            }
        }

     
    // Başvuruyu reddeder
 
        public async Task<(bool Success, string Message)> RejectApplicationAsync(int membershipId)
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                var response = await _httpClient.PutAsync($"{_baseUrl}/{membershipId}/reject", null);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                    return (true, result?.Message ?? "Başvuru reddedildi.");
                }
                else
                {
                    var errorResult = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                    return (false, errorResult?.Message ?? "Başvuru reddedilemedi.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reddetme hatası: {ex.Message}");
                return (false, "Reddetme sırasında bir hata oluştu.");
            }
        }

     
    // Üyelikten ayrılır veya üyeyi çıkarır
 
        public async Task<(bool Success, string Message)> LeaveMembershipAsync(int membershipId)
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/{membershipId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                    return (true, result?.Message ?? "Üyelik sonlandırıldı.");
                }
                else
                {
                    var errorResult = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                    return (false, errorResult?.Message ?? "Üyelik sonlandırılamadı.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ayrılma hatası: {ex.Message}");
                return (false, "Ayrılma sırasında bir hata oluştu.");
            }
        }

     
    // Öğrencinin belirli bir kulüpteki üyelik durumunu kontrol eder
 
        public async Task<ClubMembershipStatus> CheckMembershipStatusAsync(int studentId, int clubId)
        {
            try
            {
                var memberships = await GetStudentMembershipsAsync(studentId);
                var membership = memberships.FirstOrDefault(m => m.ClubID == clubId);
                
                if (membership == null)
                    return ClubMembershipStatus.NotMember;
                
                return membership.IsApproved switch
                {
                    null => ClubMembershipStatus.Pending,
                    true => ClubMembershipStatus.Approved,
                    false => ClubMembershipStatus.Rejected
                };
            }
            catch
            {
                return ClubMembershipStatus.NotMember;
            }
        }
    }

 
    /// Üyelik durumu enum'u
    public enum ClubMembershipStatus
    {
        NotMember,  // Üye değil
        Pending,    // Başvuru beklemede
        Approved,   // Onaylandı
        Rejected    // Reddedildi
    }

 
    /// API Response wrapper
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

 
    /// Kulüp üyelik başvuru DTO'su
    public class ClubMembershipApplyDto
    {
        public int ClubID { get; set; }
    }

 
    /// Kulüp üyelik response DTO'su
    public class ClubMembershipResponseDto
    {
        public int MembershipID { get; set; }
        public int StudentID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public int ClubID { get; set; }
        public string ClubName { get; set; } = string.Empty;
        public DateTime? JoinDate { get; set; }
        public bool? IsApproved { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}