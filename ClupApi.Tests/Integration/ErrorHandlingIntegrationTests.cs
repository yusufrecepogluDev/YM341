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
    public class ErrorHandlingIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ErrorHandlingIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetClub_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/clubs/99999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<ClubResponseDto>>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            Assert.NotNull(apiResponse);
            Assert.False(apiResponse.Success);
            Assert.Contains("not found", apiResponse.Message.ToLower());
        }

        [Fact]
        public async Task CreateClub_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var invalidClub = new ClubCreateDto
            {
                ClubName = "", // Invalid: empty name
                ClubNumber = 0, // Invalid: zero number
                ClubPassword = "" // Invalid: empty password
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
        }

        [Fact]
        public async Task CreateClub_WithDuplicateClubNumber_ReturnsBadRequest()
        {
            // Arrange - Create first club
            var firstClub = new ClubCreateDto
            {
                ClubName = "Test Club 1",
                ClubNumber = 12345,
                ClubPassword = "password123"
            };

            var json1 = JsonSerializer.Serialize(firstClub);
            var content1 = new StringContent(json1, Encoding.UTF8, "application/json");
            await _client.PostAsync("/api/clubs", content1);

            // Arrange - Try to create second club with same number
            var duplicateClub = new ClubCreateDto
            {
                ClubName = "Test Club 2",
                ClubNumber = 12345, // Same number
                ClubPassword = "password456"
            };

            var json2 = JsonSerializer.Serialize(duplicateClub);
            var content2 = new StringContent(json2, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/clubs", content2);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            Assert.NotNull(apiResponse);
            Assert.False(apiResponse.Success);
            Assert.Contains("already exists", apiResponse.Errors.FirstOrDefault() ?? "");
        }

        [Fact]
        public async Task UpdateClub_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var updateDto = new ClubUpdateDto
            {
                ClubName = "Updated Club Name"
            };

            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync("/api/clubs/99999", content);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteClub_WithNonExistentId_ReturnsNotFound()
        {
            // Act
            var response = await _client.DeleteAsync("/api/clubs/99999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateStudent_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var invalidStudent = new StudentCreateDto
            {
                StudentName = "", // Invalid: empty name
                StudentSurname = "",  // Invalid: empty name
                StudentNumber = 0, // Invalid: zero number
                StudentMail = "", // Invalid: empty email
                StudentPassword = "" // Invalid: empty password
            };

            var json = JsonSerializer.Serialize(invalidStudent);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/students", content);

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
        }

        [Fact]
        public async Task CreateActivity_WithInvalidClubId_ReturnsBadRequest()
        {
            // Arrange
            var invalidActivity = new ActivityCreateDto
            {
                ActivityName = "Test Activity",
                StartDate = DateTime.Now.AddDays(7),
                EndDate = DateTime.Now.AddDays(7).AddHours(2),
                OrganizingClubID = 99999 // Non-existent club
            };

            var json = JsonSerializer.Serialize(invalidActivity);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/activities", content);

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
        public async Task ApiResponse_HasConsistentFormat()
        {
            // Act - Make a successful request
            var response = await _client.GetAsync("/api/clubs");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<ClubResponseDto>>>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.NotNull(apiResponse.Message);
            Assert.NotEqual(DateTime.MinValue, apiResponse.Timestamp);
            Assert.NotNull(apiResponse.Data);
        }
    }
}