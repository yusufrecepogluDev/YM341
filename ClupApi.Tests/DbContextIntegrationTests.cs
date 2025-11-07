using ClupApi;
using ClupApi.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ClupApi.Tests
{
    public class DbContextIntegrationTests : IDisposable
    {
        private readonly AppDbContext _context;

        public DbContextIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
        }

        [Fact]
        public void CanCreateAndRetrieveStudent()
        {
            // Arrange
            var student = new Student
            {
                StudentName = "John",
                StudentSurname = "Doe",
                StudentNumber = 12345,
                StudentMail = "john.doe@example.com",
                StudentPassword = "password123",
                IsActive = true
            };

            // Act
            _context.Students.Add(student);
            _context.SaveChanges();

            var retrievedStudent = _context.Students.First(s => s.StudentNumber == 12345);

            // Assert
            Assert.NotNull(retrievedStudent);
            Assert.Equal("John", retrievedStudent.StudentName);
            Assert.Equal("Doe", retrievedStudent.StudentSurname);
        }

        [Fact]
        public void CanCreateAndRetrieveClub()
        {
            // Arrange
            var club = new Club
            {
                ClubName = "Test Club",
                ClubNumber = 54321,
                ClubPassword = "clubpass",
                IsActive = true
            };

            // Act
            _context.Clubs.Add(club);
            _context.SaveChanges();

            var retrievedClub = _context.Clubs.First(c => c.ClubNumber == 54321);

            // Assert
            Assert.NotNull(retrievedClub);
            Assert.Equal("Test Club", retrievedClub.ClubName);
        }

        [Fact]
        public void CanLoadNavigationProperties_ClubWithActivities()
        {
            // Arrange
            var club = new Club
            {
                ClubName = "Activity Club",
                ClubNumber = 11111,
                ClubPassword = "pass",
                IsActive = true
            };

            var activity = new Activity
            {
                ActivityName = "Test Activity",
                ActivityDescription = "Test Description",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1),
                IsActive = true,
                OrganizingClub = club
            };

            // Act
            _context.Clubs.Add(club);
            _context.Activities.Add(activity);
            _context.SaveChanges();

            var clubWithActivities = _context.Clubs
                .Include(c => c.Activities)
                .First(c => c.ClubNumber == 11111);

            // Assert
            Assert.NotNull(clubWithActivities);
            Assert.Single(clubWithActivities.Activities);
            Assert.Equal("Test Activity", clubWithActivities.Activities.First().ActivityName);
        }

        [Fact]
        public void CanLoadNavigationProperties_StudentWithMemberships()
        {
            // Arrange
            var student = new Student
            {
                StudentName = "Jane",
                StudentSurname = "Smith",
                StudentNumber = 22222,
                StudentMail = "jane.smith@example.com",
                StudentPassword = "password456",
                IsActive = true
            };

            var club = new Club
            {
                ClubName = "Membership Club",
                ClubNumber = 33333,
                ClubPassword = "clubpass",
                IsActive = true
            };

            var membership = new ClubMembership
            {
                Student = student,
                Club = club,
                JoinDate = DateTime.Now,
                IsApproved = true
            };

            // Act
            _context.Students.Add(student);
            _context.Clubs.Add(club);
            _context.ClubMemberships.Add(membership);
            _context.SaveChanges();

            var studentWithMemberships = _context.Students
                .Include(s => s.ClubMemberships)
                .ThenInclude(cm => cm.Club)
                .First(s => s.StudentNumber == 22222);

            // Assert
            Assert.NotNull(studentWithMemberships);
            Assert.Single(studentWithMemberships.ClubMemberships);
            Assert.Equal("Membership Club", studentWithMemberships.ClubMemberships.First().Club.ClubName);
        }

        [Fact]
        public void CascadeDelete_RemovingClub_RemovesRelatedActivities()
        {
            // Arrange
            var club = new Club
            {
                ClubName = "Delete Test Club",
                ClubNumber = 44444,
                ClubPassword = "pass",
                IsActive = true
            };

            var activity = new Activity
            {
                ActivityName = "Delete Test Activity",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1),
                IsActive = true,
                OrganizingClub = club
            };

            _context.Clubs.Add(club);
            _context.Activities.Add(activity);
            _context.SaveChanges();

            var clubId = club.ClubID;
            var activityId = activity.ActivityID;

            // Act
            _context.Clubs.Remove(club);
            _context.SaveChanges();

            // Assert
            var deletedClub = _context.Clubs.FirstOrDefault(c => c.ClubID == clubId);
            var deletedActivity = _context.Activities.FirstOrDefault(a => a.ActivityID == activityId);

            Assert.Null(deletedClub);
            Assert.Null(deletedActivity); // Should be deleted due to cascade
        }

        [Fact]
        public void CanLoadNavigationProperties_ActivityWithParticipations()
        {
            // Arrange
            var student = new Student
            {
                StudentName = "Test",
                StudentSurname = "Participant",
                StudentNumber = 55555,
                StudentMail = "test.participant@example.com",
                StudentPassword = "password789",
                IsActive = true
            };

            var club = new Club
            {
                ClubName = "Activity Host Club",
                ClubNumber = 66666,
                ClubPassword = "clubpass",
                IsActive = true
            };

            var activity = new Activity
            {
                ActivityName = "Participation Test Activity",
                ActivityDescription = "Test Description",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1),
                IsActive = true,
                OrganizingClub = club
            };

            var participation = new ActivityParticipation
            {
                Student = student,
                Activity = activity,
                JoinDate = DateTime.Now
            };

            // Act
            _context.Students.Add(student);
            _context.Clubs.Add(club);
            _context.Activities.Add(activity);
            _context.ActivityParticipations.Add(participation);
            _context.SaveChanges();

            var activityWithParticipations = _context.Activities
                .Include(a => a.ActivityParticipations)
                .ThenInclude(ap => ap.Student)
                .First(a => a.ActivityName == "Participation Test Activity");

            // Assert
            Assert.NotNull(activityWithParticipations);
            Assert.Single(activityWithParticipations.ActivityParticipations);
            Assert.Equal("Test", activityWithParticipations.ActivityParticipations.First().Student.StudentName);
        }

        [Fact]
        public void CanLoadNavigationProperties_ClubWithAnnouncements()
        {
            // Arrange
            var club = new Club
            {
                ClubName = "Announcement Club",
                ClubNumber = 77777,
                ClubPassword = "pass",
                IsActive = true
            };

            var announcement = new Announcement
            {
                AnnouncementTitle = "Test Announcement",
                AnnouncementContent = "This is a test announcement",
                CreationDate = DateTime.Now,
                StartDate = DateTime.Now,
                IsActive = true,
                Club = club
            };

            // Act
            _context.Clubs.Add(club);
            _context.Announcements.Add(announcement);
            _context.SaveChanges();

            var clubWithAnnouncements = _context.Clubs
                .Include(c => c.Announcements)
                .First(c => c.ClubNumber == 77777);

            // Assert
            Assert.NotNull(clubWithAnnouncements);
            Assert.Single(clubWithAnnouncements.Announcements);
            Assert.Equal("Test Announcement", clubWithAnnouncements.Announcements.First().AnnouncementTitle);
        }

        [Fact]
        public void CascadeDelete_RemovingStudent_RemovesRelatedMembershipsAndParticipations()
        {
            // Arrange
            var student = new Student
            {
                StudentName = "Delete",
                StudentSurname = "Test",
                StudentNumber = 88888,
                StudentMail = "delete.test@example.com",
                StudentPassword = "password",
                IsActive = true
            };

            var club = new Club
            {
                ClubName = "Test Club for Deletion",
                ClubNumber = 99999,
                ClubPassword = "pass",
                IsActive = true
            };

            var activity = new Activity
            {
                ActivityName = "Test Activity for Deletion",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1),
                IsActive = true,
                OrganizingClub = club
            };

            var membership = new ClubMembership
            {
                Student = student,
                Club = club,
                JoinDate = DateTime.Now,
                IsApproved = true
            };

            var participation = new ActivityParticipation
            {
                Student = student,
                Activity = activity,
                JoinDate = DateTime.Now
            };

            _context.Students.Add(student);
            _context.Clubs.Add(club);
            _context.Activities.Add(activity);
            _context.ClubMemberships.Add(membership);
            _context.ActivityParticipations.Add(participation);
            _context.SaveChanges();

            var studentId = student.StudentID;
            var membershipId = membership.MembershipID;
            var participationId = participation.ParticipationID;

            // Act
            _context.Students.Remove(student);
            _context.SaveChanges();

            // Assert
            var deletedStudent = _context.Students.FirstOrDefault(s => s.StudentID == studentId);
            var deletedMembership = _context.ClubMemberships.FirstOrDefault(cm => cm.MembershipID == membershipId);
            var deletedParticipation = _context.ActivityParticipations.FirstOrDefault(ap => ap.ParticipationID == participationId);

            Assert.Null(deletedStudent);
            Assert.Null(deletedMembership); // Should be deleted due to cascade
            Assert.Null(deletedParticipation); // Should be deleted due to cascade
        }

        [Fact]
        public void CanCreateComplexEntityRelationships()
        {
            // Arrange - Create a complete scenario with all entities
            var student = new Student
            {
                StudentName = "Complex",
                StudentSurname = "Test",
                StudentNumber = 10101,
                StudentMail = "complex.test@example.com",
                StudentPassword = "password",
                IsActive = true
            };

            var club = new Club
            {
                ClubName = "Complex Test Club",
                ClubNumber = 20202,
                ClubPassword = "pass",
                IsActive = true
            };

            var activity = new Activity
            {
                ActivityName = "Complex Test Activity",
                ActivityDescription = "Complex test scenario",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1),
                IsActive = true,
                OrganizingClub = club
            };

            var announcement = new Announcement
            {
                AnnouncementTitle = "Complex Test Announcement",
                AnnouncementContent = "Complex test content",
                CreationDate = DateTime.Now,
                StartDate = DateTime.Now,
                IsActive = true,
                Club = club
            };

            var membership = new ClubMembership
            {
                Student = student,
                Club = club,
                JoinDate = DateTime.Now,
                IsApproved = true
            };

            var participation = new ActivityParticipation
            {
                Student = student,
                Activity = activity,
                JoinDate = DateTime.Now
            };

            // Act
            _context.Students.Add(student);
            _context.Clubs.Add(club);
            _context.Activities.Add(activity);
            _context.Announcements.Add(announcement);
            _context.ClubMemberships.Add(membership);
            _context.ActivityParticipations.Add(participation);
            _context.SaveChanges();

            // Load with all navigation properties
            var complexClub = _context.Clubs
                .Include(c => c.Activities)
                    .ThenInclude(a => a.ActivityParticipations)
                        .ThenInclude(ap => ap.Student)
                .Include(c => c.Announcements)
                .Include(c => c.ClubMemberships)
                    .ThenInclude(cm => cm.Student)
                .First(c => c.ClubNumber == 20202);

            // Assert
            Assert.NotNull(complexClub);
            Assert.Single(complexClub.Activities);
            Assert.Single(complexClub.Announcements);
            Assert.Single(complexClub.ClubMemberships);
            Assert.Single(complexClub.Activities.First().ActivityParticipations);
            Assert.Equal("Complex", complexClub.ClubMemberships.First().Student.StudentName);
            Assert.Equal("Complex", complexClub.Activities.First().ActivityParticipations.First().Student.StudentName);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}