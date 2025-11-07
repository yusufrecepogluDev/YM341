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
    public class ClubsControllerTests
    {
        private readonly Mock<IClubRepository> _mockRepository;
        private readonly IMapper _mapper;
        private readonly ClubsController _controller;

        public ClubsControllerTests()
        {
            _mockRepository = new Mock<IClubRepository>();
            var mockValidationService = new Mock<ClupApi.Services.IValidationService>();
            
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();
            
            _controller = new ClubsController(_mockRepository.Object, _mapper, mockValidationService.Object);
        }

        [Fact]
        public async Task GetAllClubs_ReturnsOkResult_WithClubList()
        {
            // Arrange
            var clubs = new List<Club>
            {
                new Club { ClubID = 1, ClubName = "Test Club 1", ClubNumber = 12345, IsActive = true, ClubMemberships = new List<ClubMembership>(), Activities = new List<Activity>() },
                new Club { ClubID = 2, ClubName = "Test Club 2", ClubNumber = 67890, IsActive = true, ClubMemberships = new List<ClubMembership>(), Activities = new List<Activity>() }
            };

            _mockRepository.Setup(repo => repo.GetActiveClubsAsync())
                          .ReturnsAsync(clubs);

            // Act
            var result = await _controller.GetAllClubs();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetClubById_ExistingActiveClub_ReturnsOkResult()
        {
            // Arrange
            var club = new Club 
            { 
                ClubID = 1, 
                ClubName = "Test Club", 
                ClubNumber = 12345, 
                IsActive = true,
                ClubMemberships = new List<ClubMembership>(),
                Activities = new List<Activity>()
            };

            _mockRepository.Setup(repo => repo.GetByIdAsync(1))
                          .ReturnsAsync(club);

            // Act
            var result = await _controller.GetClubById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetClubById_NonExistentClub_ReturnsNotFound()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.GetByIdAsync(999))
                          .ReturnsAsync((Club?)null);

            // Act
            var result = await _controller.GetClubById(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetClubById_InactiveClub_ReturnsNotFound()
        {
            // Arrange
            var inactiveClub = new Club 
            { 
                ClubID = 1, 
                ClubName = "Inactive Club", 
                ClubNumber = 12345, 
                IsActive = false 
            };

            _mockRepository.Setup(repo => repo.GetByIdAsync(1))
                          .ReturnsAsync(inactiveClub);

            // Act
            var result = await _controller.GetClubById(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task CreateClub_ValidData_ReturnsCreatedResult()
        {
            // Arrange
            var clubCreateDto = new ClubCreateDto
            {
                ClubName = "New Club",
                ClubNumber = 12345,
                ClubPassword = "password123"
            };

            var createdClub = new Club
            {
                ClubID = 1,
                ClubName = "New Club",
                ClubNumber = 12345,
                ClubPassword = "password123",
                IsActive = true,
                ClubMemberships = new List<ClubMembership>(),
                Activities = new List<Activity>()
            };

            _mockRepository.Setup(repo => repo.GetByClubNumberAsync(12345))
                          .ReturnsAsync((Club?)null);
            _mockRepository.Setup(repo => repo.CreateAsync(It.IsAny<Club>()))
                          .ReturnsAsync(createdClub);

            // Act
            var result = await _controller.CreateClub(clubCreateDto);

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task CreateClub_DuplicateClubNumber_ReturnsBadRequest()
        {
            // Arrange
            var clubCreateDto = new ClubCreateDto
            {
                ClubName = "New Club",
                ClubNumber = 12345,
                ClubPassword = "password123"
            };

            var existingClub = new Club
            {
                ClubID = 1,
                ClubName = "Existing Club",
                ClubNumber = 12345,
                IsActive = true
            };

            _mockRepository.Setup(repo => repo.GetByClubNumberAsync(12345))
                          .ReturnsAsync(existingClub);

            // Act
            var result = await _controller.CreateClub(clubCreateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task UpdateClub_ExistingClub_ReturnsNoContent()
        {
            // Arrange
            var clubUpdateDto = new ClubUpdateDto
            {
                ClubName = "Updated Club",
                ClubPassword = "newpassword"
            };

            var existingClub = new Club
            {
                ClubID = 1,
                ClubName = "Original Club",
                ClubNumber = 12345,
                ClubPassword = "oldpassword",
                IsActive = true,
                ClubMemberships = new List<ClubMembership>(),
                Activities = new List<Activity>()
            };

            _mockRepository.Setup(repo => repo.GetByIdAsync(1))
                          .ReturnsAsync(existingClub);
            _mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Club>()))
                          .ReturnsAsync(existingClub);

            // Act
            var result = await _controller.UpdateClub(1, clubUpdateDto);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        [Fact]
        public async Task UpdateClub_NonExistentClub_ReturnsNotFound()
        {
            // Arrange
            var clubUpdateDto = new ClubUpdateDto
            {
                ClubName = "Updated Club",
                ClubPassword = "newpassword"
            };

            _mockRepository.Setup(repo => repo.GetByIdAsync(999))
                          .ReturnsAsync((Club?)null);

            // Act
            var result = await _controller.UpdateClub(999, clubUpdateDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteClub_ExistingClub_ReturnsNoContent()
        {
            // Arrange
            var existingClub = new Club
            {
                ClubID = 1,
                ClubName = "Club to Delete",
                ClubNumber = 12345,
                IsActive = true
            };

            _mockRepository.Setup(repo => repo.GetByIdAsync(1))
                          .ReturnsAsync(existingClub);
            _mockRepository.Setup(repo => repo.DeleteAsync(1))
                          .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteClub(1);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        [Fact]
        public async Task DeleteClub_NonExistentClub_ReturnsNotFound()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.GetByIdAsync(999))
                          .ReturnsAsync((Club?)null);

            // Act
            var result = await _controller.DeleteClub(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }
    }
}