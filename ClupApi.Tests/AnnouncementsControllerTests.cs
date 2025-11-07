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
    public class AnnouncementsControllerTests
    {
        private readonly Mock<IAnnouncementRepository> _mockAnnouncementRepository;
        private readonly Mock<IClubRepository> _mockClubRepository;
        private readonly IMapper _mapper;
        private readonly AnnouncementsController _controller;

        public AnnouncementsControllerTests()
        {
            _mockAnnouncementRepository = new Mock<IAnnouncementRepository>();
            _mockClubRepository = new Mock<IClubRepository>();
            
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();
            
            _controller = new AnnouncementsController(
                _mockAnnouncementRepository.Object, 
                _mockClubRepository.Object, 
                _mapper);
        }

        [Fact]
        public async Task GetAllAnnouncements_ReturnsOkResult_WithAnnouncementList()
        {
            // Arrange
            var announcements = new List<Announcement>
            {
                new Announcement 
                { 
                    AnnouncementID = 1, 
                    AnnouncementTitle = "Test Announcement 1", 
                    AnnouncementContent = "Content 1",
                    ClubID = 1,
                    IsActive = true,
                    Club = new Club { ClubID = 1, ClubName = "Test Club 1" }
                },
                new Announcement 
                { 
                    AnnouncementID = 2, 
                    AnnouncementTitle = "Test Announcement 2", 
                    AnnouncementContent = "Content 2",
                    ClubID = 1,
                    IsActive = true,
                    Club = new Club { ClubID = 1, ClubName = "Test Club 1" }
                }
            };

            _mockAnnouncementRepository.Setup(repo => repo.GetActiveAnnouncementsAsync())
                                     .ReturnsAsync(announcements);

            // Act
            var result = await _controller.GetAllAnnouncements();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetAnnouncementById_ExistingActiveAnnouncement_ReturnsOkResult()
        {
            // Arrange
            var announcement = new Announcement 
            { 
                AnnouncementID = 1, 
                AnnouncementTitle = "Test Announcement", 
                AnnouncementContent = "Test Content",
                ClubID = 1,
                IsActive = true,
                Club = new Club { ClubID = 1, ClubName = "Test Club" }
            };

            _mockAnnouncementRepository.Setup(repo => repo.GetByIdAsync(1))
                                     .ReturnsAsync(announcement);

            // Act
            var result = await _controller.GetAnnouncementById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetAnnouncementById_NonExistentAnnouncement_ReturnsNotFound()
        {
            // Arrange
            _mockAnnouncementRepository.Setup(repo => repo.GetByIdAsync(999))
                                     .ReturnsAsync((Announcement?)null);

            // Act
            var result = await _controller.GetAnnouncementById(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetAnnouncementById_InactiveAnnouncement_ReturnsNotFound()
        {
            // Arrange
            var inactiveAnnouncement = new Announcement 
            { 
                AnnouncementID = 1, 
                AnnouncementTitle = "Inactive Announcement", 
                AnnouncementContent = "Content",
                ClubID = 1,
                IsActive = false 
            };

            _mockAnnouncementRepository.Setup(repo => repo.GetByIdAsync(1))
                                     .ReturnsAsync(inactiveAnnouncement);

            // Act
            var result = await _controller.GetAnnouncementById(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetAnnouncementsByClub_ExistingClub_ReturnsOkResult()
        {
            // Arrange
            var clubId = 1;
            var announcements = new List<Announcement>
            {
                new Announcement 
                { 
                    AnnouncementID = 1, 
                    AnnouncementTitle = "Club Announcement", 
                    AnnouncementContent = "Content",
                    ClubID = clubId,
                    IsActive = true,
                    Club = new Club { ClubID = clubId, ClubName = "Test Club" }
                }
            };

            _mockClubRepository.Setup(repo => repo.ExistsAsync(clubId))
                              .ReturnsAsync(true);
            _mockAnnouncementRepository.Setup(repo => repo.GetActiveByClubIdAsync(clubId))
                                     .ReturnsAsync(announcements);

            // Act
            var result = await _controller.GetAnnouncementsByClub(clubId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetAnnouncementsByClub_NonExistentClub_ReturnsNotFound()
        {
            // Arrange
            var clubId = 999;
            _mockClubRepository.Setup(repo => repo.ExistsAsync(clubId))
                              .ReturnsAsync(false);

            // Act
            var result = await _controller.GetAnnouncementsByClub(clubId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task CreateAnnouncement_ValidData_ReturnsCreatedResult()
        {
            // Arrange
            var announcementCreateDto = new AnnouncementCreateDto
            {
                AnnouncementTitle = "New Announcement",
                AnnouncementContent = "New Content",
                ClubID = 1,
                StartDate = DateTime.UtcNow.AddDays(1)
            };

            var createdAnnouncement = new Announcement
            {
                AnnouncementID = 1,
                AnnouncementTitle = "New Announcement",
                AnnouncementContent = "New Content",
                ClubID = 1,
                StartDate = DateTime.UtcNow.AddDays(1),
                IsActive = true,
                CreationDate = DateTime.UtcNow
            };

            var announcementWithClub = new Announcement
            {
                AnnouncementID = 1,
                AnnouncementTitle = "New Announcement",
                AnnouncementContent = "New Content",
                ClubID = 1,
                StartDate = DateTime.UtcNow.AddDays(1),
                IsActive = true,
                CreationDate = DateTime.UtcNow,
                Club = new Club { ClubID = 1, ClubName = "Test Club" }
            };

            _mockClubRepository.Setup(repo => repo.ExistsAsync(1))
                              .ReturnsAsync(true);
            _mockAnnouncementRepository.Setup(repo => repo.CreateAsync(It.IsAny<Announcement>()))
                                     .ReturnsAsync(createdAnnouncement);
            _mockAnnouncementRepository.Setup(repo => repo.GetByIdAsync(1))
                                     .ReturnsAsync(announcementWithClub);

            // Act
            var result = await _controller.CreateAnnouncement(announcementCreateDto);

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task CreateAnnouncement_NonExistentClub_ReturnsBadRequest()
        {
            // Arrange
            var announcementCreateDto = new AnnouncementCreateDto
            {
                AnnouncementTitle = "New Announcement",
                AnnouncementContent = "New Content",
                ClubID = 999,
                StartDate = DateTime.UtcNow.AddDays(1)
            };

            _mockClubRepository.Setup(repo => repo.ExistsAsync(999))
                              .ReturnsAsync(false);

            // Act
            var result = await _controller.CreateAnnouncement(announcementCreateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task UpdateAnnouncement_ExistingAnnouncement_ReturnsNoContent()
        {
            // Arrange
            var announcementUpdateDto = new AnnouncementUpdateDto
            {
                AnnouncementTitle = "Updated Announcement",
                AnnouncementContent = "Updated Content",
                StartDate = DateTime.UtcNow.AddDays(2)
            };

            var existingAnnouncement = new Announcement
            {
                AnnouncementID = 1,
                AnnouncementTitle = "Original Announcement",
                AnnouncementContent = "Original Content",
                ClubID = 1,
                IsActive = true,
                Club = new Club { ClubID = 1, ClubName = "Test Club" }
            };

            _mockAnnouncementRepository.Setup(repo => repo.GetByIdAsync(1))
                                     .ReturnsAsync(existingAnnouncement);
            _mockAnnouncementRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Announcement>()))
                                     .ReturnsAsync(existingAnnouncement);

            // Act
            var result = await _controller.UpdateAnnouncement(1, announcementUpdateDto);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        [Fact]
        public async Task UpdateAnnouncement_NonExistentAnnouncement_ReturnsNotFound()
        {
            // Arrange
            var announcementUpdateDto = new AnnouncementUpdateDto
            {
                AnnouncementTitle = "Updated Announcement",
                AnnouncementContent = "Updated Content",
                StartDate = DateTime.UtcNow.AddDays(2)
            };

            _mockAnnouncementRepository.Setup(repo => repo.GetByIdAsync(999))
                                     .ReturnsAsync((Announcement?)null);

            // Act
            var result = await _controller.UpdateAnnouncement(999, announcementUpdateDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteAnnouncement_ExistingAnnouncement_ReturnsNoContent()
        {
            // Arrange
            _mockAnnouncementRepository.Setup(repo => repo.DeleteAsync(1))
                                     .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteAnnouncement(1);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        [Fact]
        public async Task DeleteAnnouncement_NonExistentAnnouncement_ReturnsNotFound()
        {
            // Arrange
            _mockAnnouncementRepository.Setup(repo => repo.DeleteAsync(999))
                                     .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteAnnouncement(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }
    }
}