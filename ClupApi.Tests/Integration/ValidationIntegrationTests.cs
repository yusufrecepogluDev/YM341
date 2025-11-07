using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;
using ClupApi.Models;
using ClupApi.DTOs;

namespace ClupApi.Tests.Integration
{
    public class ValidationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ValidationIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task CreateClubMembership_WithInvalidStudentId_ReturnsBadRequest()
        {
            // Arrange - First create a club
            var club = new ClubCreateDto
            {
                ClubName = "Test Club",
                ClubNumber = 12345,
                ClubPassword = "password123"
            };

            var clubJson = JsonSerializer.Serialize(club);
            var clubContent = new StringContent(clubJson, Encoding.UTF8, "application/json");
            var clubResponse = await _client.PostAsync("/api/clubs", clubContent);
            
            var clubResponseContent = await clubResponse.Content.ReadAsStringAsync();
            var clubApiResponse = JsonSerializer.Deserialize<ApiResponse<ClubResponseDto>>(clubResponseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // Arrange - Try to create membership with invalid student
            var membership = new ClubMembershipCreateDto
            {
                StudentID = 99999, // Non-existent student
                ClubID = clubApiResponse!.Data!.ClubID
            };

            var json = JsonSerializer.Serialize(membership);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/clubmemberships", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            Assert.NotNull(apiResponse);
            Assert.False(apiResponse.Success);
            Assert.Contains("student", apiResponse.Errors.FirstOrDefault()?.ToLower() ?? "");
        }

        [Fact]
        public async Task CreateActivityParticipation_WithPastActivity_ReturnsBadRequest()
        {
            // Arrange - Create club and student first
            var club = new ClubCreateDto
            {
                ClubName = "Test Club",
                ClubNumber = 12346,
                ClubPassword = "password123"
            };

            var clubJson = JsonSerializer.Serialize(club);
            var clubContent = new StringContent(clubJson, Encoding.UTF8, "application/json");
            var clubResponse = await _client.PostAsync("/api/clubs", clubContent);
            
            var clubResponseContent = await clubResponse.Content.ReadAsStringAsync();
            var clubApiResponse = JsonSerializer.Deserialize<ApiResponse<ClubResponseDto>>(clubResponseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var student = new StudentCreateDto
            {
                StudentName = "Test",
                StudentSurname = "Student",
                StudentNumber = 12345,
                StudentMail = "test@example.com",
                StudentPassword = "password123"
            };

            var studentJson = JsonSerializer.Serialize(student);
            var studentContent = new StringContent(studentJson, Encoding.UTF8, "application/json");
            var studentResponse = await _client.PostAsync("/api/students", studentContent);
            
            var studentResponseContent = await studentResponse.Content.ReadAsStringAsync();
            var studentApiResponse = JsonSerializer.Deserialize<ApiResponse<StudentResponseDto>>(studentResponseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // Create activity with past date
            var activity = new ActivityCreateDto
            {
                ActivityName = "Past Activity",
                StartDate = DateTime.Now.AddDays(-1), // Past date
                EndDate = DateTime.Now.AddDays(-1).AddHours(2),
                OrganizingClubID = clubApiResponse!.Data!.ClubID
            };

            var activityJson = JsonSerializer.Serialize(activity);
            var activityContent = new StringContent(activityJson, Encoding.UTF8, "application/json");

            // Act - Try to create activity with past date
            var response = await _client.PostAsync("/api/activities", activityContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            Assert.NotNull(apiResponse);
            Assert.False(apiResponse.Success);
            Assert.Contains("future", apiResponse.Errors.FirstOrDefault()?.ToLower() ?? "");
        }

        [Fact]
        public async Task CreateAnnouncement_WithInvalidClubId_ReturnsBadRequest()
        {
            // Arrange
            var announcement = new AnnouncementCreateDto
            {
                AnnouncementTitle = "Test Announcement",
                AnnouncementContent = "Test content",
                ClubID = 99999, // Non-existent club
                StartDate = DateTime.Now.AddDays(1)
            };

            var json = JsonSerializer.Serialize(announcement);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/announcements", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            Assert.NotNull(apiResponse);
            Assert.False(apiResponse.Success);
            Assert.Contains("club", apiResponse.Errors.FirstOrDefault()?.ToLower() ?? "");
        }

        [Fact]
        public async Task ValidationErrors_ReturnConsistentFormat()
        {
            // Arrange - Create request with multiple validation errors
            var invalidClub = new ClubCreateDto
            {
                ClubName = "", // Required field empty
                ClubNumber = 0, // Invalid number
                ClubPassword = "" // Required field empty
            };

            var json = JsonSerializer.Serialize(invalidClub);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/clubs", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            Assert.NotNull(apiResponse);
            Assert.False(apiResponse.Success);
            Assert.Equal("Validation failed", apiResponse.Message);
            Assert.NotEmpty(apiResponse.Errors);
            Assert.NotEqual(DateTime.MinValue, apiResponse.Timestamp);
        }
    }
}