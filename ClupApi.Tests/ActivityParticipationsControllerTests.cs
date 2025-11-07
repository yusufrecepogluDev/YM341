using Xunit;
using Moq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ClupApi.Controllers;
using ClupApi.Repositories.Interfaces;
using ClupApi.DTOs;
using ClupApi.Models;
using ClupApi.Mappings;

namespace ClupApi.Tests
{
    public class ActivityParticipationsControllerTests
    {
        private readonly Mock<IActivityParticipationRepository> _mockParticipationRepository;
        private readonly Mock<IActivityRepository> _mockActivityRepository;
        private readonly Mock<IStudentRepository> _mockStudentRepository;
        private readonly IMapper _mapper;
        private readonly ActivityParticipationsController _controller;

        public ActivityParticipationsControllerTests()
        {
            _mockParticipationRepository = new Mock<IActivityParticipationRepository>();
            _mockActivityRepository = new Mock<IActivityRepository>();
            _mockStudentRepository = new Mock<IStudentRepository>();
            
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();
            
            _controller = new ActivityParticipationsController(
                _mockParticipationRepository.Object,
                _mockActivityRepository.Object,
                _mockStudentRepository.Object,
                _mapper);
        }

        [Fact]
        public async Task GetParticipationsByStudent_ReturnsOkResult_WithParticipationList()
        {
            // Arrange
            var studentId = 1;
            var participations = new List<ActivityParticipation>
            {
                new ActivityParticipation 
                { 
                    ParticipationID = 1, 
                    StudentID = studentId, 
                    ActivityID = 1, 
                    JoinDate = DateTime.UtcNow,
                    Rating = 5,
                    Student = new Student { StudentID = studentId, StudentName = "John", StudentSurname = "Doe", StudentNumber = 12345 },
                    Activity = new Activity 
                    { 
                        ActivityID = 1, 
                        ActivityName = "Test Activity", 
                        StartDate = DateTime.UtcNow.AddDays(7),
                        EndDate = DateTime.UtcNow.AddDays(7).AddHours(2),
                        OrganizingClub = new Club { ClubID = 1, ClubName = "Test Club" }
                    }
                }
            };

            _mockStudentRepository.Setup(repo => repo.ExistsAsync(studentId))
                                 .ReturnsAsync(true);
            _mockParticipationRepository.Setup(repo => repo.GetByStudentIdAsync(studentId))
                                       .ReturnsAsync(participations);

            // Act
            var result = await _controller.GetParticipationsByStudent(studentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetParticipationsByStudent_ReturnsNotFound_WhenStudentDoesNotExist()
        {
            // Arrange
            var studentId = 999;
            _mockStudentRepository.Setup(repo => repo.ExistsAsync(studentId))
                                 .ReturnsAsync(false);

            // Act
            var result = await _controller.GetParticipationsByStudent(studentId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetParticipationsByActivity_ReturnsOkResult_WithParticipationList()
        {
            // Arrange
            var activityId = 1;
            var participations = new List<ActivityParticipation>
            {
                new ActivityParticipation 
                { 
                    ParticipationID = 1, 
                    StudentID = 1, 
                    ActivityID = activityId, 
                    JoinDate = DateTime.UtcNow,
                    Rating = 4,
                    Student = new Student { StudentID = 1, StudentName = "John", StudentSurname = "Doe", StudentNumber = 12345 },
                    Activity = new Activity 
                    { 
                        ActivityID = activityId, 
                        ActivityName = "Test Activity", 
                        StartDate = DateTime.UtcNow.AddDays(7),
                        EndDate = DateTime.UtcNow.AddDays(7).AddHours(2),
                        OrganizingClub = new Club { ClubID = 1, ClubName = "Test Club" }
                    }
                }
            };

            _mockActivityRepository.Setup(repo => repo.ExistsAsync(activityId))
                                  .ReturnsAsync(true);
            _mockParticipationRepository.Setup(repo => repo.GetByActivityIdAsync(activityId))
                                       .ReturnsAsync(participations);

            // Act
            var result = await _controller.GetParticipationsByActivity(activityId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task CreateParticipation_ReturnsCreatedResult_WithValidData()
        {
            // Arrange
            var createDto = new ActivityParticipationCreateDto
            {
                StudentID = 1,
                ActivityID = 1
            };

            var createdParticipation = new ActivityParticipation
            {
                ParticipationID = 1,
                StudentID = 1,
                ActivityID = 1,
                JoinDate = DateTime.UtcNow,
                Rating = null,
                Student = new Student { StudentID = 1, StudentName = "John", StudentSurname = "Doe", StudentNumber = 12345 },
                Activity = new Activity 
                { 
                    ActivityID = 1, 
                    ActivityName = "Test Activity", 
                    StartDate = DateTime.UtcNow.AddDays(7),
                    EndDate = DateTime.UtcNow.AddDays(7).AddHours(2),
                    OrganizingClub = new Club { ClubID = 1, ClubName = "Test Club" }
                }
            };

            _mockStudentRepository.Setup(repo => repo.ExistsAsync(createDto.StudentID))
                                 .ReturnsAsync(true);
            _mockActivityRepository.Setup(repo => repo.ExistsAsync(createDto.ActivityID))
                                  .ReturnsAsync(true);
            _mockParticipationRepository.Setup(repo => repo.GetByStudentAndActivityAsync(createDto.StudentID, createDto.ActivityID))
                                       .ReturnsAsync((ActivityParticipation?)null);
            _mockParticipationRepository.Setup(repo => repo.CreateAsync(It.IsAny<ActivityParticipation>()))
                                       .ReturnsAsync(createdParticipation);

            // Act
            var result = await _controller.CreateParticipation(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task CreateParticipation_ReturnsBadRequest_WhenStudentDoesNotExist()
        {
            // Arrange
            var createDto = new ActivityParticipationCreateDto
            {
                StudentID = 999,
                ActivityID = 1
            };

            _mockStudentRepository.Setup(repo => repo.ExistsAsync(createDto.StudentID))
                                 .ReturnsAsync(false);

            // Act
            var result = await _controller.CreateParticipation(createDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task CreateParticipation_ReturnsBadRequest_WhenActivityDoesNotExist()
        {
            // Arrange
            var createDto = new ActivityParticipationCreateDto
            {
                StudentID = 1,
                ActivityID = 999
            };

            _mockStudentRepository.Setup(repo => repo.ExistsAsync(createDto.StudentID))
                                 .ReturnsAsync(true);
            _mockActivityRepository.Setup(repo => repo.ExistsAsync(createDto.ActivityID))
                                  .ReturnsAsync(false);

            // Act
            var result = await _controller.CreateParticipation(createDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task CreateParticipation_ReturnsBadRequest_WhenParticipationAlreadyExists()
        {
            // Arrange
            var createDto = new ActivityParticipationCreateDto
            {
                StudentID = 1,
                ActivityID = 1
            };

            var existingParticipation = new ActivityParticipation
            {
                ParticipationID = 1,
                StudentID = 1,
                ActivityID = 1
            };

            _mockStudentRepository.Setup(repo => repo.ExistsAsync(createDto.StudentID))
                                 .ReturnsAsync(true);
            _mockActivityRepository.Setup(repo => repo.ExistsAsync(createDto.ActivityID))
                                  .ReturnsAsync(true);
            _mockParticipationRepository.Setup(repo => repo.GetByStudentAndActivityAsync(createDto.StudentID, createDto.ActivityID))
                                       .ReturnsAsync(existingParticipation);

            // Act
            var result = await _controller.CreateParticipation(createDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task RateActivity_ReturnsOkResult_WithUpdatedParticipation()
        {
            // Arrange
            var participationId = 1;
            var ratingDto = new ActivityParticipationRatingDto { Rating = 5 };
            var participation = new ActivityParticipation
            {
                ParticipationID = participationId,
                StudentID = 1,
                ActivityID = 1,
                Rating = null,
                Student = new Student { StudentID = 1, StudentName = "John", StudentSurname = "Doe", StudentNumber = 12345 },
                Activity = new Activity 
                { 
                    ActivityID = 1, 
                    ActivityName = "Test Activity", 
                    StartDate = DateTime.UtcNow.AddDays(7),
                    EndDate = DateTime.UtcNow.AddDays(7).AddHours(2),
                    OrganizingClub = new Club { ClubID = 1, ClubName = "Test Club" }
                }
            };

            _mockParticipationRepository.Setup(repo => repo.GetByIdAsync(participationId))
                                       .ReturnsAsync(participation);
            _mockParticipationRepository.Setup(repo => repo.UpdateAsync(It.IsAny<ActivityParticipation>()))
                                       .ReturnsAsync(participation);

            // Act
            var result = await _controller.RateActivity(participationId, ratingDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(5, participation.Rating);
        }

        [Fact]
        public async Task RateActivity_ReturnsNotFound_WhenParticipationDoesNotExist()
        {
            // Arrange
            var participationId = 999;
            var ratingDto = new ActivityParticipationRatingDto { Rating = 5 };
            
            _mockParticipationRepository.Setup(repo => repo.GetByIdAsync(participationId))
                                       .ReturnsAsync((ActivityParticipation?)null);

            // Act
            var result = await _controller.RateActivity(participationId, ratingDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteParticipation_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var participationId = 1;
            _mockParticipationRepository.Setup(repo => repo.DeleteAsync(participationId))
                                       .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteParticipation(participationId);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        [Fact]
        public async Task DeleteParticipation_ReturnsNotFound_WhenParticipationDoesNotExist()
        {
            // Arrange
            var participationId = 999;
            _mockParticipationRepository.Setup(repo => repo.DeleteAsync(participationId))
                                       .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteParticipation(participationId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }
    }
}