using ClupApi.DTOs;
using ClupApi.Models;
using ClupApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClupApi.Repositories
{
    public class CalendarRepository : BaseRepository<CalendarEventDto>, ICalendarRepository
    {
        public CalendarRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<CalendarEventDto>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var events = new List<CalendarEventDto>();

            // Activities (Kulüp Etkinlikleri)
            var activities = await _context.Activity
                .Include(a => a.OrganizingClub)
                .Where(a => !a.IsDeleted && a.IsActive &&
                           a.StartDate >= startDate && a.StartDate <= endDate)
                .Select(a => new CalendarEventDto
                {
                    Id = a.ActivityID,
                    Title = a.ActivityName,
                    Description = a.ActivityDescription ?? "",
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    Categories = "KulupEtkinligi",
                    CategoriesColor = "#4CAF50",
                    IsAllDay = a.StartDate.TimeOfDay == TimeSpan.Zero && a.EndDate.TimeOfDay == TimeSpan.Zero,
                    OrganizingClub = a.OrganizingClub.ClubName
                })
                .ToListAsync();

            events.AddRange(activities);

            // Announcements (Duyurular)
            var announcements = await _context.Announcements
                .Include(a => a.Club)
                .Where(a => !a.IsDeleted && a.IsActive &&
                           a.StartDate >= startDate && a.StartDate <= endDate)
                .Select(a => new CalendarEventDto
                {
                    Id = a.AnnouncementID,
                    Title = a.AnnouncementTitle,
                    Description = a.AnnouncementContent,
                    StartDate = a.StartDate,
                    EndDate = a.StartDate,
                    Categories = "Duyuru",
                    CategoriesColor = "#2196F3",
                    IsAllDay = true,
                    OrganizingClub = a.Club.ClubName
                })
                .ToListAsync();

            events.AddRange(announcements);

            // Academic Events (Akademik Olaylar)
            try
            {
                var academicEvents = await _context.AcademicEvents
                    .Where(a => a.StartDate >= startDate && a.StartDate <= endDate)
                    .Select(a => new CalendarEventDto
                    {
                        Id = a.ID,
                        Title = a.Title,
                        Description = a.Description,
                        StartDate = a.StartDate,
                        EndDate = a.EndDate,
                        Categories = "AkademikOlay",
                        CategoriesColor = "#FF9800",
                        IsAllDay = true, // Akademik olaylar genelde tam gün
                        OrganizingClub = a.Category
                    })
                    .ToListAsync();

                events.AddRange(academicEvents);
            }
            catch (Exception ex)
            {
                // AcademicEvents sorgusu başarısız oldu, devam et
                Console.WriteLine($"AcademicEvents query failed: {ex.Message}");
            }

            return events.OrderBy(e => e.StartDate).ToList();
        }

        public async Task<List<CalendarEventDto>> GetEventsByDateAsync(DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = date.Date.AddDays(1).AddTicks(-1);

            var events = new List<CalendarEventDto>();

            // Activities
            var activities = await _context.Activity
                .Include(a => a.OrganizingClub)
                .Where(a => !a.IsDeleted && a.IsActive &&
                           a.StartDate >= startOfDay && a.StartDate <= endOfDay)
                .Select(a => new CalendarEventDto
                {
                    Id = a.ActivityID,
                    Title = a.ActivityName,
                    Description = a.ActivityDescription ?? "",
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    Categories = "KulupEtkinligi",
                    CategoriesColor = "#4CAF50",
                    IsAllDay = a.StartDate.TimeOfDay == TimeSpan.Zero && a.EndDate.TimeOfDay == TimeSpan.Zero,
                    OrganizingClub = a.OrganizingClub.ClubName
                })
                .ToListAsync();

            events.AddRange(activities);

            // Announcements
            var announcements = await _context.Announcements
                .Include(a => a.Club)
                .Where(a => !a.IsDeleted && a.IsActive &&
                           a.StartDate >= startOfDay && a.StartDate <= endOfDay)
                .Select(a => new CalendarEventDto
                {
                    Id = a.AnnouncementID,
                    Title = a.AnnouncementTitle,
                    Description = a.AnnouncementContent,
                    StartDate = a.StartDate,
                    EndDate = a.StartDate,
                    Categories = "Duyuru",
                    CategoriesColor = "#2196F3",
                    IsAllDay = true,
                    OrganizingClub = a.Club.ClubName
                })
                .ToListAsync();

            events.AddRange(announcements);

            // Academic Events
            try
            {
                var academicEvents = await _context.AcademicEvents
                    .Where(a => a.StartDate >= startOfDay && a.StartDate <= endOfDay)
                    .Select(a => new CalendarEventDto
                    {
                        Id = a.ID,
                        Title = a.Title,
                        Description = a.Description,
                        StartDate = a.StartDate,
                        EndDate = a.EndDate,
                        Categories = "AkademikOlay",
                        CategoriesColor = "#FF9800",
                        IsAllDay = true, // Akademik olaylar genelde tam gün
                        OrganizingClub = a.Category
                    })
                    .ToListAsync();

                events.AddRange(academicEvents);
            }
            catch (Exception ex)
            {
                // AcademicEvents sorgusu başarısız oldu, devam et
                Console.WriteLine($"AcademicEvents query failed: {ex.Message}");
            }

            return events.OrderBy(e => e.StartDate).ToList();
        }

        public Task<List<CategoryDto>> GetCategoriesAsync()
        {
            var categories = new List<CategoryDto>
            {
                new CategoryDto
                {
                    Name = "KulupEtkinligi",
                    DisplayName = "Kulüp Etkinliği",
                    Color = "#4CAF50"
                },
                new CategoryDto
                {
                    Name = "Duyuru",
                    DisplayName = "Duyuru",
                    Color = "#2196F3"
                },
                new CategoryDto
                {
                    Name = "AkademikOlay",
                    DisplayName = "Akademik Olay",
                    Color = "#FF9800"
                }
            };
            
            return Task.FromResult(categories);
        }
    }
}
