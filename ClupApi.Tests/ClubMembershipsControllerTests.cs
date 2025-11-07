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
    public class ClubMembershipsControllerTests
    {
        private readonly Mock<IClubMembershipRepository> _mockMembershipRepository;
        private readonly Mock<IClubRepository> _mockClubRepository;
        private readonly Mock<IStudentRepository> _mockStudentRepository;
        private readonly IMapper _mapper;
        private readonly ClubMembershipsController _controller;

        public ClubMembershipsControllerTests()
        {
            _mockMembershipRepository = new Mock<IClubMembershipRepository>();
            _mockClubRepository = new Mock<IClubRepository>();
            _mockStudentRepository = new Mock<IStudentRepository>();
            
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();
            
            _controller = new ClubMembershipsController(
                _mockMembershipRepository.Object,
                _mockClubRepository.Object,
                _mockStudentRepository.Object,
                _mapper);
        }

        [Fact]
        public async Task GetMembershipsByStudent_ReturnsOkResult_WithMembershipList()
        {
            // Arrange
            var studentId = 1;
            var memberships = new List<ClubMembership>
            {
                new ClubMembership 
                { 
                    MembershipID = 1, 
                    StudentID = studentId, 
                    ClubID = 1, 
                    IsApproved = true,
                    JoinDate = DateTime.UtcNow,
                    Student = new Student { StudentID = studentId, StudentName = "John", StudentSurname = "Doe", StudentNumber = 12345 },
                    Club = new Club { ClubID = 1, ClubName = "Test Club" }
                }
            };

            _mockStudentRepository.Setup(repo => repo.ExistsAsync(studentId))
                                 .ReturnsAsync(true);
            _mockMembershipRepository.Setup(repo => repo.GetByStudentIdAsync(studentId))
                                    .ReturnsAsync(memberships);

            // Act
            var result = await _controller.GetMembershipsByStudent(studentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetMembershipsByStudent_ReturnsNotFound_WhenStudentDoesNotExist()
        {
            // Arrange
            var studentId = 999;
            _mockStudentRepository.Setup(repo => repo.ExistsAsync(studentId))
                                 .ReturnsAsync(false);

            // Act
            var result = await _controller.GetMembershipsByStudent(studentId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetMembershipsByClub_ReturnsOkResult_WithMembershipList()
        {
            // Arrange
            var clubId = 1;
            var memberships = new List<ClubMembership>
            {
                new ClubMembership 
                { 
                    MembershipID = 1, 
                    StudentID = 1, 
                    ClubID = clubId, 
                    IsApproved = true,
                    JoinDate = DateTime.UtcNow,
                    Student = new Student { StudentID = 1, StudentName = "John", StudentSurname = "Doe", StudentNumber = 12345 },
                    Club = new Club { ClubID = clubId, ClubName = "Test Club" }
                }
            };

            _mockClubRepository.Setup(repo => repo.ExistsAsync(clubId))
                              .ReturnsAsync(true);
            _mockMembershipRepository.Setup(repo => repo.GetByClubIdAsync(clubId))
                                    .ReturnsAsync(memberships);

            // Act
            var result = await _controller.GetMembershipsByClub(clubId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task CreateMembership_ReturnsCreatedResult_WithValidData()
        {
            // Arrange
            var createDto = new ClubMembershipCreateDto
            {
                StudentID = 1,
                ClubID = 1
            };

            var createdMembership = new ClubMembership
            {
                MembershipID = 1,
                StudentID = 1,
                ClubID = 1,
                JoinDate = DateTime.UtcNow,
                IsApproved = null,
                Student = new Student { StudentID = 1, StudentName = "John", StudentSurname = "Doe", StudentNumber = 12345 },
                Club = new Club { ClubID = 1, ClubName = "Test Club" }
            };

            _mockStudentRepository.Setup(repo => repo.ExistsAsync(createDto.StudentID))
                                 .ReturnsAsync(true);
            _mockClubRepository.Setup(repo => repo.ExistsAsync(createDto.ClubID))
                              .ReturnsAsync(true);
            _mockMembershipRepository.Setup(repo => repo.GetMembershipAsync(createDto.StudentID, createDto.ClubID))
                                    .ReturnsAsync((ClubMembership?)null);
            _mockMembershipRepository.Setup(repo => repo.CreateAsync(It.IsAny<ClubMembership>()))
                                    .ReturnsAsync(createdMembership);

            // Act
            var result = await _controller.CreateMembership(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task CreateMembership_ReturnsBadRequest_WhenStudentDoesNotExist()
        {
            // Arrange
            var createDto = new ClubMembershipCreateDto
            {
                StudentID = 999,
                ClubID = 1
            };

            _mockStudentRepository.Setup(repo => repo.ExistsAsync(createDto.StudentID))
                                 .ReturnsAsync(false);

            // Act
            var result = await _controller.CreateMembership(createDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task ApproveMembership_ReturnsOkResult_WithUpdatedMembership()
        {
            // Arrange
            var membershipId = 1;
            var updateDto = new ClubMembershipUpdateDto { IsApproved = true };
            var membership = new ClubMembership
            {
                MembershipID = membershipId,
                StudentID = 1,
                ClubID = 1,
                IsApproved = null,
                Student = new Student { StudentID = 1, StudentName = "John", StudentSurname = "Doe", StudentNumber = 12345 },
                Club = new Club { ClubID = 1, ClubName = "Test Club" }
            };

            _mockMembershipRepository.Setup(repo => repo.GetByIdAsync(membershipId))
                                    .ReturnsAsync(membership);
            _mockMembershipRepository.Setup(repo => repo.UpdateAsync(It.IsAny<ClubMembership>()))
                                    .ReturnsAsync(membership);

            // Act
            var result = await _controller.ApproveMembership(membershipId, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task DeleteMembership_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var membershipId = 1;
            _mockMembershipRepository.Setup(repo => repo.DeleteAsync(membershipId))
                                    .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteMembership(membershipId);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        [Fact]
        public async Task DeleteMembership_ReturnsNotFound_WhenMembershipDoesNotExist()
        {
            // Arrange
            var membershipId = 999;
            _mockMembershipRepository.Setup(repo => repo.DeleteAsync(membershipId))
                                    .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteMembership(membershipId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }
    }
}