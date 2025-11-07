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
    public class StudentsControllerTests
    {
        private readonly Mock<IStudentRepository> _mockRepository;
        private readonly IMapper _mapper;
        private readonly StudentsController _controller;

        public StudentsControllerTests()
        {
            _mockRepository = new Mock<IStudentRepository>();
            
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();
            
            _controller = new StudentsController(_mockRepository.Object, _mapper);
        }

        [Fact]
        public async Task GetAllStudents_ReturnsOkResult_WithStudentList()
        {
            // Arrange
            var students = new List<Student>
            {
                new Student 
                { 
                    StudentID = 1, 
                    StudentName = "John", 
                    StudentSurname = "Doe", 
                    StudentNumber = 12345, 
                    StudentMail = "john.doe@example.com",
                    IsActive = true, 
                    ClubMemberships = new List<ClubMembership>(), 
                    ActivityParticipations = new List<ActivityParticipation>() 
                },
                new Student 
                { 
                    StudentID = 2, 
                    StudentName = "Jane", 
                    StudentSurname = "Smith", 
                    StudentNumber = 67890, 
                    StudentMail = "jane.smith@example.com",
                    IsActive = true, 
                    ClubMemberships = new List<ClubMembership>(), 
                    ActivityParticipations = new List<ActivityParticipation>() 
                }
            };

            _mockRepository.Setup(repo => repo.GetActiveStudentsAsync())
                          .ReturnsAsync(students);

            // Act
            var result = await _controller.GetAllStudents();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetStudentById_ExistingActiveStudent_ReturnsOkResult()
        {
            // Arrange
            var student = new Student 
            { 
                StudentID = 1, 
                StudentName = "John", 
                StudentSurname = "Doe", 
                StudentNumber = 12345, 
                StudentMail = "john.doe@example.com",
                IsActive = true,
                ClubMemberships = new List<ClubMembership>(),
                ActivityParticipations = new List<ActivityParticipation>()
            };

            _mockRepository.Setup(repo => repo.GetByIdAsync(1))
                          .ReturnsAsync(student);

            // Act
            var result = await _controller.GetStudentById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetStudentById_NonExistentStudent_ReturnsNotFound()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.GetByIdAsync(999))
                          .ReturnsAsync((Student?)null);

            // Act
            var result = await _controller.GetStudentById(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetStudentById_InactiveStudent_ReturnsNotFound()
        {
            // Arrange
            var inactiveStudent = new Student 
            { 
                StudentID = 1, 
                StudentName = "John", 
                StudentSurname = "Doe", 
                StudentNumber = 12345, 
                StudentMail = "john.doe@example.com",
                IsActive = false 
            };

            _mockRepository.Setup(repo => repo.GetByIdAsync(1))
                          .ReturnsAsync(inactiveStudent);

            // Act
            var result = await _controller.GetStudentById(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task CreateStudent_ValidData_ReturnsCreatedResult()
        {
            // Arrange
            var studentCreateDto = new StudentCreateDto
            {
                StudentName = "John",
                StudentSurname = "Doe",
                StudentNumber = 12345,
                StudentMail = "john.doe@example.com",
                StudentPassword = "password123"
            };

            var createdStudent = new Student
            {
                StudentID = 1,
                StudentName = "John",
                StudentSurname = "Doe",
                StudentNumber = 12345,
                StudentMail = "john.doe@example.com",
                StudentPassword = "password123",
                IsActive = true,
                ClubMemberships = new List<ClubMembership>(),
                ActivityParticipations = new List<ActivityParticipation>()
            };

            _mockRepository.Setup(repo => repo.GetByStudentNumberAsync(12345))
                          .ReturnsAsync((Student?)null);
            _mockRepository.Setup(repo => repo.GetByEmailAsync("john.doe@example.com"))
                          .ReturnsAsync((Student?)null);
            _mockRepository.Setup(repo => repo.CreateAsync(It.IsAny<Student>()))
                          .ReturnsAsync(createdStudent);

            // Act
            var result = await _controller.CreateStudent(studentCreateDto);

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task CreateStudent_DuplicateStudentNumber_ReturnsBadRequest()
        {
            // Arrange
            var studentCreateDto = new StudentCreateDto
            {
                StudentName = "John",
                StudentSurname = "Doe",
                StudentNumber = 12345,
                StudentMail = "john.doe@example.com",
                StudentPassword = "password123"
            };

            var existingStudent = new Student
            {
                StudentID = 1,
                StudentName = "Existing",
                StudentSurname = "Student",
                StudentNumber = 12345,
                StudentMail = "existing@example.com",
                IsActive = true
            };

            _mockRepository.Setup(repo => repo.GetByStudentNumberAsync(12345))
                          .ReturnsAsync(existingStudent);

            // Act
            var result = await _controller.CreateStudent(studentCreateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task CreateStudent_DuplicateEmail_ReturnsBadRequest()
        {
            // Arrange
            var studentCreateDto = new StudentCreateDto
            {
                StudentName = "John",
                StudentSurname = "Doe",
                StudentNumber = 12345,
                StudentMail = "john.doe@example.com",
                StudentPassword = "password123"
            };

            var existingStudent = new Student
            {
                StudentID = 1,
                StudentName = "Existing",
                StudentSurname = "Student",
                StudentNumber = 67890,
                StudentMail = "john.doe@example.com",
                IsActive = true
            };

            _mockRepository.Setup(repo => repo.GetByStudentNumberAsync(12345))
                          .ReturnsAsync((Student?)null);
            _mockRepository.Setup(repo => repo.GetByEmailAsync("john.doe@example.com"))
                          .ReturnsAsync(existingStudent);

            // Act
            var result = await _controller.CreateStudent(studentCreateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task UpdateStudent_ExistingStudent_ReturnsNoContent()
        {
            // Arrange
            var studentUpdateDto = new StudentUpdateDto
            {
                StudentName = "Updated John",
                StudentSurname = "Updated Doe",
                StudentMail = "updated.john@example.com",
                StudentPassword = "newpassword"
            };

            var existingStudent = new Student
            {
                StudentID = 1,
                StudentName = "John",
                StudentSurname = "Doe",
                StudentNumber = 12345,
                StudentMail = "john.doe@example.com",
                StudentPassword = "oldpassword",
                IsActive = true,
                ClubMemberships = new List<ClubMembership>(),
                ActivityParticipations = new List<ActivityParticipation>()
            };

            _mockRepository.Setup(repo => repo.GetByIdAsync(1))
                          .ReturnsAsync(existingStudent);
            _mockRepository.Setup(repo => repo.GetByEmailAsync("updated.john@example.com"))
                          .ReturnsAsync((Student?)null);
            _mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Student>()))
                          .ReturnsAsync(existingStudent);

            // Act
            var result = await _controller.UpdateStudent(1, studentUpdateDto);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        [Fact]
        public async Task UpdateStudent_NonExistentStudent_ReturnsNotFound()
        {
            // Arrange
            var studentUpdateDto = new StudentUpdateDto
            {
                StudentName = "Updated John",
                StudentSurname = "Updated Doe",
                StudentMail = "updated.john@example.com",
                StudentPassword = "newpassword"
            };

            _mockRepository.Setup(repo => repo.GetByIdAsync(999))
                          .ReturnsAsync((Student?)null);

            // Act
            var result = await _controller.UpdateStudent(999, studentUpdateDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task UpdateStudent_DuplicateEmail_ReturnsBadRequest()
        {
            // Arrange
            var studentUpdateDto = new StudentUpdateDto
            {
                StudentName = "Updated John",
                StudentSurname = "Updated Doe",
                StudentMail = "existing@example.com",
                StudentPassword = "newpassword"
            };

            var existingStudent = new Student
            {
                StudentID = 1,
                StudentName = "John",
                StudentSurname = "Doe",
                StudentNumber = 12345,
                StudentMail = "john.doe@example.com",
                IsActive = true
            };

            var studentWithSameEmail = new Student
            {
                StudentID = 2,
                StudentName = "Other",
                StudentSurname = "Student",
                StudentMail = "existing@example.com",
                IsActive = true
            };

            _mockRepository.Setup(repo => repo.GetByIdAsync(1))
                          .ReturnsAsync(existingStudent);
            _mockRepository.Setup(repo => repo.GetByEmailAsync("existing@example.com"))
                          .ReturnsAsync(studentWithSameEmail);

            // Act
            var result = await _controller.UpdateStudent(1, studentUpdateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task DeleteStudent_ExistingStudent_ReturnsNoContent()
        {
            // Arrange
            var existingStudent = new Student
            {
                StudentID = 1,
                StudentName = "John",
                StudentSurname = "Doe",
                StudentNumber = 12345,
                StudentMail = "john.doe@example.com",
                IsActive = true
            };

            _mockRepository.Setup(repo => repo.GetByIdAsync(1))
                          .ReturnsAsync(existingStudent);
            _mockRepository.Setup(repo => repo.DeleteAsync(1))
                          .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteStudent(1);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        [Fact]
        public async Task DeleteStudent_NonExistentStudent_ReturnsNotFound()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.GetByIdAsync(999))
                          .ReturnsAsync((Student?)null);

            // Act
            var result = await _controller.DeleteStudent(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }
    }
}