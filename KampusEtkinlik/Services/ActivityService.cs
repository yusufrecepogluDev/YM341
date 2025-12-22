using System.Net.Http.Json;
using System.Net.Http.Headers;
using KampusEtkinlik.Data.Models;
using KampusEtkinlik.Data.DTOs;
using Microsoft.Extensions.Configuration;

namespace KampusEtkinlik.Services
{
    public class ActivityService
    {
        private readonly HttpClient _httpClient;
        private readonly TokenService _tokenService;
        private readonly string _baseUrl;

        public ActivityService(HttpClient httpClient, TokenService tokenService, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _baseUrl = $"{configuration["ApiSettings:BaseUrl"]}/api/Activities";
        }

        private async Task SetAuthorizationHeaderAsync()
        {
            var token = await _tokenService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<Activity>> GetAllAsync()
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                var result = await _httpClient.GetFromJsonAsync<List<Activity>>(_baseUrl);
                return result ?? new List<Activity>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API bağlantı hatası: {ex.Message}");
                return new List<Activity>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Beklenmeyen hata: {ex.Message}");
                return new List<Activity>();
            }
        }

        public async Task<Activity?> GetByIdAsync(int id)
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<Activity>($"{_baseUrl}/{id}");
        }

        public async Task<bool> AddAsync(ActivityCreateDto createDto)
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                var response = await _httpClient.PostAsJsonAsync(_baseUrl, createDto);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Hatası: {error}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ekleme hatası: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(int id, ActivityUpdateDto updateDto)
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/{id}", updateDto);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Hatası: {error}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Güncelleme hatası: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<(bool Success, string? ImagePath, string? ErrorMessage)> UploadImageAsync(Stream fileStream, string fileName)
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                var content = new MultipartFormDataContent();
                var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var streamContent = new StreamContent(memoryStream);
                var extension = Path.GetExtension(fileName).TrimStart('.').ToLower();
                var mimeType = extension switch
                {
                    "jpg" or "jpeg" => "image/jpeg",
                    "png" => "image/png",
                    "gif" => "image/gif",
                    "webp" => "image/webp",
                    _ => "application/octet-stream"
                };
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
                content.Add(streamContent, "file", fileName);

                var response = await _httpClient.PostAsync($"{_baseUrl}/upload-image", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ImageUploadResponse>();
                    return (true, result?.ImagePath, null);
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Upload hatası: {response.StatusCode} - {error}");
                    return (false, null, error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Upload exception: {ex.Message}");
                return (false, null, ex.Message);
            }
        }
    }

    public class ImageUploadResponse
    {
        public string? ImagePath { get; set; }
    }
}
