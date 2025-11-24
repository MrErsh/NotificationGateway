using Hangfire;
using Microsoft.Extensions.Logging;
using NotificationGateway.Infrastructure.Interfaces;

namespace NotificationGateway.Infrastructure.Services
{
    public class RecurringJobsService : IRecurringJobsService
    {
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly ILogger<RecurringJobsService> _logger;

        public RecurringJobsService(
            IRecurringJobManager recurringJobManager,
            ILogger<RecurringJobsService> logger)
        {
            _recurringJobManager = recurringJobManager;
            _logger = logger;
        }

        public void SetupRecurringJobs()
        {
            // Комплексная очистка каждый день в 2:00
            _recurringJobManager.AddOrUpdate<INotificationCleanupService>(
                "comprehensive-cleanup",
                service => service.CleanupOldNotificationsAsync(CancellationToken.None),
                "0 2 * * *"); // Каждый день в 2:00

            // Быстрая очистка проваленных уведомлений каждый день в 3:00
            _recurringJobManager.AddOrUpdate<INotificationCleanupService>(
                "failed-cleanup",
                service => service.CleanupFailedNotificationsAsync(TimeSpan.FromDays(7), CancellationToken.None),
                "0 3 * * *");

            // Очистка зависших pending уведомлений каждый час
            _recurringJobManager.AddOrUpdate<INotificationCleanupService>(
                "pending-cleanup",
                service => service.CleanupPendingNotificationsAsync(TimeSpan.FromHours(24), CancellationToken.None),
                "0 * * * *"); // Каждый час

            _recurringJobManager.AddOrUpdate<IStuckNotificationsService>(
                "retry-stuck-notifications",
                service => service.RetryStuckNotificationsAsync(),
                "*/10 * * * *");

            _logger.LogInformation("All recurring jobs setup completed");
        }
    }
}
