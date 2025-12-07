using ClupApi.Controllers;
using ClupApi.DTOs;
using ClupApi.Models;
using ClupApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace ClupApi.Tests
{
    /// <summary>
    /// Unit tests for ChatController
    /// Task 2.5: ChatController endpoints tests
    /// </summary>
    public class ChatControllerTests
    {
        private readonly Mock<IChatService> _mockChatService;
        private readonly Mock<ILogger<ChatController>> _mockLogger;
        private readonly ChatController _controller;
        private const string TestUserId = "test-user-123";
        private const string TestMessage = "Hello, chatbot!";

        public ChatControllerTests()
        {
            _mockChatService = new Mock<IChatService>();
            _mockLogger = new Mock<ILogger<ChatController>>();
            _controller = new ChatController(_mockChatService.Object, _mockLogger.Object);

            // Setup user claims for authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, TestUserId)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task SendMessage_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var request = new ChatRequestDto
            {
                Message = TestMessage,
                SessionId = "session-123"
            };

            var expectedResponse = new ChatResponseDto
            {
                Response = "Hello! How can I help you?",
                Timestamp = DateTime.UtcNow,
                Success = true
            };

            _mockChatService
                .Setup(s => s.SendToN8nAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.SendMessage(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<ChatResponseDto>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.NotNull(apiResponse.Data);
            Assert.Equal(expectedResponse.Response, apiResponse.Data.Response);

            _mockChatService.Verify(
                s => s.SendToN8nAsync(TestMessage, TestUserId, request.SessionId),
                Times.Once);
        }

        [Fact]
        public async Task SendMessage_WithInvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var request = new ChatRequestDto
            {
                Message = "", // Invalid: empty message
                SessionId = "session-123"
            };

            _controller.ModelState.AddModelError("Message", "Message is required");

            // Act
            var result = await _controller.SendMessage(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse>(badRequestResult.Value);
            Assert.False(apiResponse.Success);

            _mockChatService.Verify(
                s => s.SendToN8nAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task SendMessage_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            var request = new ChatRequestDto
            {
                Message = TestMessage,
                SessionId = "session-123"
            };

            // Remove user claims to simulate unauthenticated request
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            // Act
            var result = await _controller.SendMessage(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse>(unauthorizedResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Contains("kimliği bulunamadı", apiResponse.Message, StringComparison.OrdinalIgnoreCase);

            _mockChatService.Verify(
                s => s.SendToN8nAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task SendMessage_WhenChatServiceThrowsTimeout_Returns504()
        {
            // Arrange
            var request = new ChatRequestDto
            {
                Message = TestMessage,
                SessionId = "session-123"
            };

            _mockChatService
                .Setup(s => s.SendToN8nAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new TaskCanceledException("Request timed out"));

            // Act
            var result = await _controller.SendMessage(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(504, statusCodeResult.StatusCode);
            var apiResponse = Assert.IsType<ApiResponse>(statusCodeResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Contains("zaman aşımı", apiResponse.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SendMessage_WhenChatServiceThrowsNetworkError_Returns503()
        {
            // Arrange
            var request = new ChatRequestDto
            {
                Message = TestMessage,
                SessionId = "session-123"
            };

            _mockChatService
                .Setup(s => s.SendToN8nAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            var result = await _controller.SendMessage(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(503, statusCodeResult.StatusCode);
            var apiResponse = Assert.IsType<ApiResponse>(statusCodeResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Contains("Bağlantı", apiResponse.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SendMessage_WhenChatServiceReturnsError_Returns500()
        {
            // Arrange
            var request = new ChatRequestDto
            {
                Message = TestMessage,
                SessionId = "session-123"
            };

            var errorResponse = new ChatResponseDto
            {
                Response = "",
                Success = false,
                ErrorMessage = "N8n webhook returned error"
            };

            _mockChatService
                .Setup(s => s.SendToN8nAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.SendMessage(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var apiResponse = Assert.IsType<ApiResponse>(statusCodeResult.Value);
            Assert.False(apiResponse.Success);
        }

        [Fact]
        public void HealthCheck_ReturnsOkResult()
        {
            // Act
            var result = _controller.HealthCheck();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = okResult.Value as ApiResponse<object>;
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.NotNull(apiResponse.Data);
        }

        [Fact]
        public async Task SendMessage_ExtractsUserIdFromJwtToken()
        {
            // Arrange
            var request = new ChatRequestDto
            {
                Message = TestMessage,
                SessionId = "session-123"
            };

            var expectedResponse = new ChatResponseDto
            {
                Response = "Response",
                Success = true
            };

            string? capturedUserId = null;
            _mockChatService
                .Setup(s => s.SendToN8nAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((msg, userId, sessionId) => capturedUserId = userId)
                .ReturnsAsync(expectedResponse);

            // Act
            await _controller.SendMessage(request);

            // Assert
            Assert.Equal(TestUserId, capturedUserId);
        }
    }
}
