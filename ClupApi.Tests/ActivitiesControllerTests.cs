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
    public class ActivitiesControllerTests
    {
        private readonly Mock<IActivityRepository> _mockActivityRepository;
        private readonly Mock<IClubRepository> _mockClubRepository;
        private readonly IMapper _mapper;
        private readonly ActivitiesController _controller;

        public ActivitiesControllerTests()
        {
            _mockActivityRepository = new Mock<IActivityRepository>();
            _mockClubRepository = new Mock<IClubRepository>();
            
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();
            
            _controller = new ActivitiesController(_mockActivityRepository.Object, _mockClubRepository.Object, _mapper);
        }

        [Fact]
        public async Task GetAllActivities_ReturnsOkResult_WithActivityList()
        {
            // Arrange
            var activities = new List<Activity>
            {
                new Activity 
                { 
                    ActivityID = 1, 
                    ActivityName = "Test Activity 1", 
                    OrganizingClubID = 1,
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EndDate = DateTime.UtcNow.AddDays(2),
                    IsActive = true,
                    OrganizingClub = new Club { ClubName = "Test Club 1" },
                    ActivityParticipations = new List<ActivityParticipation>()
                },
                new Activity 
                { 
                    ActivityID = 2, 
                    ActivityName = "Test Activity 2", 
                    OrganizingClubID = 2,
                    StartDate = DateTime.UtcNow.AddDays(3),
                    EndDate = DateTime.UtcNow.AddDays(4),
                    IsActive = true,
                    OrganizingClub = new Club { ClubName = "Test Club 2" },
                    ActivityParticipations = new List<ActivityParticipation>()
                }
            };

            _mockActivityRepository.Setup(repo => repo.GetActiveActivitiesAsync())
                                  .ReturnsAsync(activities);

            // Act
            var result = await _controller.GetAllActivities();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetActivityById_ExistingActiveActivity_ReturnsOkResult()
        {
            // Arrange
            var activity = new Activity 
            { 
                ActivityID = 1, 
                ActivityName = "Test Activity", 
                OrganizingClubID = 1,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                IsActive = true,
                OrganizingClub = new Club { ClubName = "Test Club" },
                ActivityParticipations = new List<ActivityParticipation>()
            };

            _mockActivityRepository.Setup(repo => repo.GetByIdAsync(1))
                                  .ReturnsAsync(activity);

            // Act
            var result = await _controller.GetActivityById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetActivityById_NonExistentActivity_ReturnsNotFound()
        {
            // Arrange
            _mockActivityRepository.Setup(repo => repo.GetByIdAsync(999))
                                  .ReturnsAsync((Activity?)null);

            // Act
            var result = await _controller.GetActivityById(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetActivitiesByClub_ExistingClub_ReturnsOkResult()
        {
            // Arrange
            var club = new Club { ClubID = 1, ClubName = "Test Club", IsActive = true };
            var activities = new List<Activity>
            {
                new Activity 
                { 
                    ActivityID = 1, 
                    ActivityName = "Club Activity", 
                    OrganizingClubID = 1,
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EndDate = DateTime.UtcNow.AddDays(2),
                    IsActive = true,
                    OrganizingClub = club,
                    ActivityParticipations = new List<ActivityParticipation>()
                }
            };

            _mockClubRepository.Setup(repo => repo.GetByIdAsync(1))
                              .ReturnsAsync(club);
            _mockActivityRepository.Setup(repo => repo.GetByClubIdAsync(1))
                                  .ReturnsAsync(activities);

            // Act
            var result = await _controller.GetActivitiesByClub(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetActivitiesByClub_NonExistentClub_ReturnsNotFound()
        {
            // Arrange
            _mockClubRepository.Setup(repo => repo.GetByIdAsync(999))
                              .ReturnsAsync((Club?)null);

            // Act
            var result = await _controller.GetActivitiesByClub(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task CreateActivity_ValidData_ReturnsCreatedResult()
        {
            // Arrange
            var activityCreateDto = new ActivityCreateDto
            {
                ActivityName = "New Activity",
                ActivityDescription = "Test Description",
                OrganizingClubID = 1,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                ParticipantLimit = 50
            };

            var club = new Club { ClubID = 1, ClubName = "Test Club", IsActive = true };
            var createdActivity = new Activity
            {
                ActivityID = 1,
                ActivityName = "New Activity",
                ActivityDescription = "Test Description",
                OrganizingClubID = 1,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                ParticipantLimit = 50,
                IsActive = true,
                OrganizingClub = club,
                ActivityParticipations = new List<ActivityParticipation>()
            };

            _mockClubRepository.Setup(repo => repo.GetByIdAsync(1))
                              .ReturnsAsync(club);
            _mockActivityRepository.Setup(repo => repo.CreateAsync(It.IsAny<Activity>()))
                                  .ReturnsAsync(createdActivity);

            // Act
            var result = await _controller.CreateActivity(activityCreateDto);

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task CreateActivity_NonExistentClub_ReturnsBadRequest()
        {
            // Arrange
            var activityCreateDto = new ActivityCreateDto
            {
                ActivityName = "New Activity",
                OrganizingClubID = 999,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2)
            };

            _mockClubRepository.Setup(repo => repo.GetByIdAsync(999))
                              .ReturnsAsync((Club?)null);

            // Act
            var result = await _controller.CreateActivity(activityCreateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task CreateActivity_InvalidDates_ReturnsBadRequest()
        {
            // Arrange
            var activityCreateDto = new ActivityCreateDto
            {
                ActivityName = "New Activity",
                OrganizingClubID = 1,
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(1) // End date before start date
            };

            var club = new Club { ClubID = 1, ClubName = "Test Club", IsActive = true };
            _mockClubRepository.Setup(repo => repo.GetByIdAsync(1))
                              .ReturnsAsync(club);

            // Act
            var result = await _controller.CreateActivity(activityCreateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task UpdateActivity_ExistingActivity_ReturnsNoContent()
        {
            // Arrange
            var activityUpdateDto = new ActivityUpdateDto
            {
                ActivityName = "Updated Activity",
                ActivityDescription = "Updated Description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                ParticipantLimit = 100
            };

            var existingActivity = new Activity
            {
                ActivityID = 1,
                ActivityName = "Original Activity",
                OrganizingClubID = 1,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                IsActive = true,
                NumberOfParticipants = 10,
                OrganizingClub = new Club { ClubName = "Test Club" },
                ActivityParticipations = new List<ActivityParticipation>()
            };

            _mockActivityRepository.Setup(repo => repo.GetByIdAsync(1))
                                  .ReturnsAsync(existingActivity);
            _mockActivityRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Activity>()))
                                  .ReturnsAsync(existingActivity);

            // Act
            var result = await _controller.UpdateActivity(1, activityUpdateDto);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        [Fact]
        public async Task UpdateActivity_NonExistentActivity_ReturnsNotFound()
        {
            // Arrange
            var activityUpdateDto = new ActivityUpdateDto
            {
                ActivityName = "Updated Activity",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2)
            };

            _mockActivityRepository.Setup(repo => repo.GetByIdAsync(999))
                                  .ReturnsAsync((Activity?)null);

            // Act
            var result = await _controller.UpdateActivity(999, activityUpdateDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteActivity_ExistingActivity_ReturnsNoContent()
        {
            // Arrange
            var existingActivity = new Activity
            {
                ActivityID = 1,
                ActivityName = "Activity to Delete",
                StartDate = DateTime.UtcNow.AddDays(1), // Future activity
                EndDate = DateTime.UtcNow.AddDays(2),
                IsActive = true
            };

            _mockActivityRepository.Setup(repo => repo.GetByIdAsync(1))
                                  .ReturnsAsync(existingActivity);
            _mockActivityRepository.Setup(repo => repo.DeleteAsync(1))
                                  .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteActivity(1);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        [Fact]
        public async Task DeleteActivity_ActivityAlreadyStarted_ReturnsBadRequest()
        {
            // Arrange
            var existingActivity = new Activity
            {
                ActivityID = 1,
                ActivityName = "Started Activity",
                StartDate = DateTime.UtcNow.AddDays(-1), // Activity already started
                EndDate = DateTime.UtcNow.AddDays(1),
                IsActive = true
            };

            _mockActivityRepository.Setup(repo => repo.GetByIdAsync(1))
                                  .ReturnsAsync(existingActivity);

            // Act
            var result = await _controller.DeleteActivity(1);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task DeleteActivity_NonExistentActivity_ReturnsNotFound()
        {
            // Arrange
            _mockActivityRepository.Setup(repo => repo.GetByIdAsync(999))
                                  .ReturnsAsync((Activity?)null);

            // Act
            var result = await _controller.DeleteActivity(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }
    }
}