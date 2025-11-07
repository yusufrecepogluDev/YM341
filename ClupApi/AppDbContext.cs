using Microsoft.EntityFrameworkCore;
using ClupApi.Models;

namespace ClupApi
{
    public class AppDbContext : DbContext
     {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Club> Clubs { get; set; }

        public DbSet<Student> Students { get; set; }

        public DbSet<Activity> Activity { get; set; }

        public DbSet<Announcement> Announcements { get; set; }

        public DbSet<ClubMembership> ClubMemberships { get; set; }

        public DbSet<ActivityParticipation> ActivityParticipations { get; set; }

    
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure unique indexes
            modelBuilder.Entity<Club>()
                .HasIndex(e => e.ClubNumber)
                .IsUnique();

            modelBuilder.Entity<Student>()
                .HasIndex(e => e.StudentNumber)
                .IsUnique();

            // Configure relationships with corrected navigation property names
            modelBuilder.Entity<Activity>()
                .HasOne(a => a.OrganizingClub)
                .WithMany(c => c.Activities)
                .HasForeignKey(a => a.OrganizingClubID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ActivityParticipation>()
                .HasOne(ap => ap.Activity)
                .WithMany(a => a.ActivityParticipations)
                .HasForeignKey(ap => ap.ActivityID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ActivityParticipation>()
                .HasOne(ap => ap.Student)
                .WithMany(s => s.ActivityParticipations)
                .HasForeignKey(ap => ap.StudentID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Announcement>()
                .HasOne(a => a.Club)
                .WithMany(c => c.Announcements)
                .HasForeignKey(a => a.ClubID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClubMembership>()
                .HasOne(cm => cm.Club)
                .WithMany(c => c.ClubMemberships)
                .HasForeignKey(cm => cm.ClubID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClubMembership>()
                .HasOne(cm => cm.Student)
                .WithMany(s => s.ClubMemberships)
                .HasForeignKey(cm => cm.StudentID)
                .OnDelete(DeleteBehavior.Cascade);
        }
     }
}
