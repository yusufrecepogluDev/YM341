using ClupApi.Controllers;
using ClupApi.DTOs;
using ClupApi.Models;
using ClupApi.Repositories.Interfaces;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace ClupApi.Tests
{
    /// <summary>
    /// Property-based tests for ChatController
    /// Uses FsCheck for property testing with 100 iterations
    /// </summary>
    public class ChatControllerPropertyTests
    {
        /// <summary>
        /// Feature: n8n-chatbot-integration, Property 13: JWT token validation
        /// Validates: Requirements 7.2
        /// 
        /// Property: For any incoming chat request to ClupApi, the request should be rejected 
        /// if it lacks a valid JWT token.
        /// </summary>
        [Property(MaxTest = 100)]
        public Property JwtTokenValidation_RequestWithoutToken_ShouldBeRejected()
        {
            return Prop.ForAll(
                Arb.Default.NonEmptyString().Generator.Select(s => s.Get).ToArbitrary(),
                (message) =>
                {
                    // Arrange
                    var mockChatService = new Mock<IChatService>();
                    var mockLogger = new Mock<ILogger<ChatController>>();
                    var controller = new ChatController(mockChatService.Object, mockLogger.Object);

                    // Setup controller without user claims (no JWT token)
                    controller.ControllerContext = new ControllerContext
                    {
                        HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
                    };

                    var request = new ChatRequestDto
                    {
                        Message = message,
                        SessionId = Guid.NewGuid().ToString()
                    };

                    // Act
                    var result = controller.SendMessage(request).Result;

                    // Assert
                    var isUnauthorized = result is UnauthorizedObjectResult;
                    
                    // Verify that ChatService was never called
                    mockChatService.Verify(
                        s => s.SendToN8nAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                        Times.Never);

                    return isUnauthorized.ToProperty();
                });
        }

        /// <summary>
        /// Feature: n8n-chatbot-integration, Property 8: User identity transmission
        /// Validates: Requirements 4.2, 7.3
        /// 
        /// Property: For any request sent to N8n webhook, the request payload should contain 
        /// the authenticated user's ID.
        /// </summary>
        [Property(MaxTest = 100)]
        public Property UserIdentityTransmission_AllRequests_ShouldIncludeUserId()
        {
            return Prop.ForAll(
                Arb.Default.NonEmptyString().Generator.Select(s => s.Get).ToArbitrary(),
                Arb.Default.Guid().Generator.Select(g => g.ToString()).ToArbitrary(),
                (message, userId) =>
                {
                    // Arrange
                    var mockChatService = new Mock<IChatService>();
                    var mockLogger = new Mock<ILogger<ChatController>>();
                    var controller = new ChatController(mockChatService.Object, mockLogger.Object);

                    // Setup user claims with the generated userId
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId)
                    };
                    var identity = new ClaimsIdentity(claims, "TestAuth");
                    var claimsPrincipal = new ClaimsPrincipal(identity);
                    
                    controller.ControllerContext = new ControllerContext
                    {
                        HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                    };

                    var request = new ChatRequestDto
                    {
                        Message = message,
                        SessionId = Guid.NewGuid().ToString()
                    };

                    var expectedResponse = new ChatResponseDto
                    {
                        Response = "Test response",
                        Success = true
                    };

                    string? capturedUserId = null;
                    mockChatService
                        .Setup(s => s.SendToN8nAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                        .Callback<string, string, string>((msg, uid, sessionId) => capturedUserId = uid)
                        .ReturnsAsync(expectedResponse);

                    // Act
                    var result = controller.SendMessage(request).Result;

                    // Assert
                    // Verify that the userId passed to ChatService matches the JWT token userId
                    var userIdMatches = capturedUserId == userId;
                    
                    // Verify that ChatService was called exactly once
                    mockChatService.Verify(
                        s => s.SendToN8nAsync(message, userId, request.SessionId),
                        Times.Once);

                    return userIdMatches.ToProperty();
                });
        }

        /// <summary>
        /// Property: For any valid message and userId combination, the controller should 
        /// successfully call ChatService with those exact parameters.
        /// </summary>
        [Property(MaxTest = 100)]
        public Property SendMessage_WithValidInputs_ShouldCallChatService()
        {
            return Prop.ForAll(
                Arb.Default.NonEmptyString().Generator.Select(s => s.Get).ToArbitrary(),
                Arb.Default.Guid().Generator.Select(g => g.ToString()).ToArbitrary(),
                Arb.Default.Guid().Generator.Select(g => g.ToString()).ToArbitrary(),
                (message, userId, sessionId) =>
                {
                    // Arrange
                    var mockChatService = new Mock<IChatService>();
                    var mockLogger = new Mock<ILogger<ChatController>>();
                    var controller = new ChatController(mockChatService.Object, mockLogger.Object);

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId)
                    };
                    var identity = new ClaimsIdentity(claims, "TestAuth");
                    var claimsPrincipal = new ClaimsPrincipal(identity);
                    
                    controller.ControllerContext = new ControllerContext
                    {
                        HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                    };

                    var request = new ChatRequestDto
                    {
                        Message = message,
                        SessionId = sessionId
                    };

                    var expectedResponse = new ChatResponseDto
                    {
                        Response = "Test response",
                        Success = true
                    };

                    mockChatService
                        .Setup(s => s.SendToN8nAsync(message, userId, sessionId))
                        .ReturnsAsync(expectedResponse);

                    // Act
                    var result = controller.SendMessage(request).Result;

                    // Assert
                    var isOk = result is OkObjectResult;
                    
                    mockChatService.Verify(
                        s => s.SendToN8nAsync(message, userId, sessionId),
                        Times.Once);

                    return isOk.ToProperty();
                });
        }
    }
}
