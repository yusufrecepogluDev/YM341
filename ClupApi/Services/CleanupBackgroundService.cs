using ClupApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ClupApi.Services
{
    public class CleanupBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CleanupBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Her saat kontrol et

        public CleanupBackgroundService(IServiceProvider serviceProvider, ILogger<CleanupBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cleanup Background Service başlatıldı");

            // İlk çalıştırmada hemen kontrol et
            await DoCleanupAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_checkInterval, stoppingToken);
                    await DoCleanupAsync();
                }
                catch (OperationCanceledException)
                {
                    // Servis durduruluyor
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cleanup işlemi sırasında hata oluştu");
                }
            }

            _logger.LogInformation("Cleanup Background Service durduruldu");
        }

        private async Task DoCleanupAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var now = DateTime.UtcNow;
            var deactivatedActivities = 0;
            var deactivatedAnnouncements = 0;

            // Oylaması biten etkinlikleri deaktif et (EvaluationEndDate geçmiş olanlar)
            // EvaluationEndDate null ise EndDate + 1 gün kullanılır
            var expiredActivities = await dbContext.Activity
                .Where(a => a.IsActive && !a.IsDeleted && 
                    ((a.EvaluationEndDate.HasValue && a.EvaluationEndDate.Value < now) ||
                     (!a.EvaluationEndDate.HasValue && a.EndDate.AddDays(1) < now)))
                .ToListAsync();

            foreach (var activity in expiredActivities)
            {
                activity.IsActive = false;
                deactivatedActivities++;
            }

            // Tarihi geçmiş duyuruları deaktif et (StartDate geçmiş olanlar - 7 gün sonra deaktif)
            var expiredAnnouncements = await dbContext.Announcements
                .Where(a => a.IsActive && !a.IsDeleted && a.StartDate.AddDays(7) < now)
                .ToListAsync();

            foreach (var announcement in expiredAnnouncements)
            {
                announcement.IsActive = false;
                deactivatedAnnouncements++;
            }

            if (deactivatedActivities > 0 || deactivatedAnnouncements > 0)
            {
                await dbContext.SaveChangesAsync();
                _logger.LogInformation(
                    "Cleanup tamamlandı: {ActivityCount} etkinlik, {AnnouncementCount} duyuru deaktif edildi",
                    deactivatedActivities, deactivatedAnnouncements);
            }
            else
            {
                _logger.LogDebug("Cleanup: Deaktif edilecek kayıt bulunamadı");
            }
        }
    }
}
