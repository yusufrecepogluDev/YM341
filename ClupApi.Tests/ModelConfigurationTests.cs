using ClupApi.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Xunit;

namespace ClupApi.Tests
{
    public class ModelConfigurationTests
    {
        [Fact]
        public void Student_ShouldHaveCorrectPrimaryKeyConfiguration()
        {
            // Arrange
            var studentType = typeof(Student);
            var primaryKeyProperty = studentType.GetProperty("StudentID");

            // Act & Assert
            Assert.NotNull(primaryKeyProperty);
            var keyAttribute = primaryKeyProperty.GetCustomAttribute<KeyAttribute>();
            Assert.NotNull(keyAttribute);
        }

        [Fact]
        public void Club_ShouldHaveCorrectPrimaryKeyConfiguration()
        {
            // Arrange
            var clubType = typeof(Club);
            var primaryKeyProperty = clubType.GetProperty("ClubID");

            // Act & Assert
            Assert.NotNull(primaryKeyProperty);
            var keyAttribute = primaryKeyProperty.GetCustomAttribute<KeyAttribute>();
            Assert.NotNull(keyAttribute);
        }

        [Fact]
        public void Activity_ShouldHaveCorrectPrimaryKeyConfiguration()
        {
            // Arrange
            var activityType = typeof(Activity);
            var primaryKeyProperty = activityType.GetProperty("ActivityID");

            // Act & Assert
            Assert.NotNull(primaryKeyProperty);
            var keyAttribute = primaryKeyProperty.GetCustomAttribute<KeyAttribute>();
            Assert.NotNull(keyAttribute);
        }

        [Fact]
        public void Announcement_ShouldHaveCorrectPrimaryKeyConfiguration()
        {
            // Arrange
            var announcementType = typeof(Announcement);
            var primaryKeyProperty = announcementType.GetProperty("AnnouncementID");

            // Act & Assert
            Assert.NotNull(primaryKeyProperty);
            var keyAttribute = primaryKeyProperty.GetCustomAttribute<KeyAttribute>();
            Assert.NotNull(keyAttribute);
        }

        [Fact]
        public void ClubMembership_ShouldHaveCorrectPrimaryKeyConfiguration()
        {
            // Arrange
            var membershipType = typeof(ClubMembership);
            var primaryKeyProperty = membershipType.GetProperty("MembershipID");

            // Act & Assert
            Assert.NotNull(primaryKeyProperty);
            var keyAttribute = primaryKeyProperty.GetCustomAttribute<KeyAttribute>();
            Assert.NotNull(keyAttribute);
        }

        [Fact]
        public void ActivityParticipation_ShouldHaveCorrectPrimaryKeyConfiguration()
        {
            // Arrange
            var participationType = typeof(ActivityParticipation);
            var primaryKeyProperty = participationType.GetProperty("ParticipationID");

            // Act & Assert
            Assert.NotNull(primaryKeyProperty);
            var keyAttribute = primaryKeyProperty.GetCustomAttribute<KeyAttribute>();
            Assert.NotNull(keyAttribute);
        }

        [Fact]
        public void Student_NavigationProperties_ShouldBeInitialized()
        {
            // Arrange & Act
            var student = new Student();

            // Assert
            Assert.NotNull(student.ActivityParticipations);
            Assert.NotNull(student.ClubMemberships);
            Assert.Empty(student.ActivityParticipations);
            Assert.Empty(student.ClubMemberships);
        }

        [Fact]
        public void Club_NavigationProperties_ShouldBeInitialized()
        {
            // Arrange & Act
            var club = new Club();

            // Assert
            Assert.NotNull(club.Activities);
            Assert.NotNull(club.Announcements);
            Assert.NotNull(club.ClubMemberships);
            Assert.Empty(club.Activities);
            Assert.Empty(club.Announcements);
            Assert.Empty(club.ClubMemberships);
        }

        [Fact]
        public void Activity_NavigationProperties_ShouldBeInitialized()
        {
            // Arrange & Act
            var activity = new Activity();

            // Assert
            Assert.NotNull(activity.ActivityParticipations);
            Assert.Empty(activity.ActivityParticipations);
        }

        [Fact]
        public void Activity_ShouldHaveCorrectForeignKeyConfiguration()
        {
            // Arrange
            var activityType = typeof(Activity);
            var foreignKeyProperty = activityType.GetProperty("OrganizingClubID");

            // Act & Assert
            Assert.NotNull(foreignKeyProperty);
            Assert.Equal(typeof(int), foreignKeyProperty.PropertyType);
            
            var navigationProperty = activityType.GetProperty("OrganizingClub");
            Assert.NotNull(navigationProperty);
            Assert.Equal(typeof(Club), navigationProperty.PropertyType);
        }

        [Fact]
        public void Announcement_ShouldHaveCorrectForeignKeyConfiguration()
        {
            // Arrange
            var announcementType = typeof(Announcement);
            var foreignKeyProperty = announcementType.GetProperty("ClubID");

            // Act & Assert
            Assert.NotNull(foreignKeyProperty);
            Assert.Equal(typeof(int), foreignKeyProperty.PropertyType);
            
            var navigationProperty = announcementType.GetProperty("Club");
            Assert.NotNull(navigationProperty);
            Assert.Equal(typeof(Club), navigationProperty.PropertyType);
        }

        [Fact]
        public void ClubMembership_ShouldHaveCorrectForeignKeyConfiguration()
        {
            // Arrange
            var membershipType = typeof(ClubMembership);
            var studentForeignKey = membershipType.GetProperty("StudentID");
            var clubForeignKey = membershipType.GetProperty("ClubID");

            // Act & Assert
            Assert.NotNull(studentForeignKey);
            Assert.Equal(typeof(int), studentForeignKey.PropertyType);
            Assert.NotNull(clubForeignKey);
            Assert.Equal(typeof(int), clubForeignKey.PropertyType);
            
            var studentNavigation = membershipType.GetProperty("Student");
            var clubNavigation = membershipType.GetProperty("Club");
            Assert.NotNull(studentNavigation);
            Assert.Equal(typeof(Student), studentNavigation.PropertyType);
            Assert.NotNull(clubNavigation);
            Assert.Equal(typeof(Club), clubNavigation.PropertyType);
        }

        [Fact]
        public void ActivityParticipation_ShouldHaveCorrectForeignKeyConfiguration()
        {
            // Arrange
            var participationType = typeof(ActivityParticipation);
            var activityForeignKey = participationType.GetProperty("ActivityID");
            var studentForeignKey = participationType.GetProperty("StudentID");

            // Act & Assert
            Assert.NotNull(activityForeignKey);
            Assert.Equal(typeof(int), activityForeignKey.PropertyType);
            Assert.NotNull(studentForeignKey);
            Assert.Equal(typeof(int), studentForeignKey.PropertyType);
            
            var activityNavigation = participationType.GetProperty("Activity");
            var studentNavigation = participationType.GetProperty("Student");
            Assert.NotNull(activityNavigation);
            Assert.Equal(typeof(Activity), activityNavigation.PropertyType);
            Assert.NotNull(studentNavigation);
            Assert.Equal(typeof(Student), studentNavigation.PropertyType);
        }

        [Fact]
        public void NavigationProperties_ShouldHaveCorrectNaming()
        {
            // Test collection navigation properties use plural names
            var studentType = typeof(Student);
            Assert.NotNull(studentType.GetProperty("ActivityParticipations")); // Plural
            Assert.NotNull(studentType.GetProperty("ClubMemberships")); // Plural

            var clubType = typeof(Club);
            Assert.NotNull(clubType.GetProperty("Activities")); // Plural
            Assert.NotNull(clubType.GetProperty("Announcements")); // Plural
            Assert.NotNull(clubType.GetProperty("ClubMemberships")); // Plural

            var activityType = typeof(Activity);
            Assert.NotNull(activityType.GetProperty("ActivityParticipations")); // Plural

            // Test single navigation properties use singular names
            Assert.NotNull(activityType.GetProperty("OrganizingClub")); // Singular
            
            var announcementType = typeof(Announcement);
            Assert.NotNull(announcementType.GetProperty("Club")); // Singular

            var membershipType = typeof(ClubMembership);
            Assert.NotNull(membershipType.GetProperty("Club")); // Singular
            Assert.NotNull(membershipType.GetProperty("Student")); // Singular

            var participationType = typeof(ActivityParticipation);
            Assert.NotNull(participationType.GetProperty("Activity")); // Singular
            Assert.NotNull(participationType.GetProperty("Student")); // Singular
        }

        [Fact]
        public void PrimaryKeys_ShouldNotHaveForeignKeyAttributes()
        {
            // Verify that primary key properties don't have [ForeignKey] attributes
            var studentIdProperty = typeof(Student).GetProperty("StudentID");
            var clubIdProperty = typeof(Club).GetProperty("ClubID");
            var activityIdProperty = typeof(Activity).GetProperty("ActivityID");
            var announcementIdProperty = typeof(Announcement).GetProperty("AnnouncementID");
            var membershipIdProperty = typeof(ClubMembership).GetProperty("MembershipID");
            var participationIdProperty = typeof(ActivityParticipation).GetProperty("ParticipationID");

            // Assert no [ForeignKey] attributes on primary keys
            Assert.Null(studentIdProperty.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute>());
            Assert.Null(clubIdProperty.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute>());
            Assert.Null(activityIdProperty.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute>());
            Assert.Null(announcementIdProperty.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute>());
            Assert.Null(membershipIdProperty.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute>());
            Assert.Null(participationIdProperty.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute>());
        }
    }
}