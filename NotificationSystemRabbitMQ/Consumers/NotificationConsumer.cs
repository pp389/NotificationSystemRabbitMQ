using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationSystem.Contracts;
using NotificationSystem.Models;

namespace NotificationSystem.Consumers
{
    public class NotificationConsumer : IConsumer<SendNotification>
    {
        private readonly NotificationDbContext _db;
        public NotificationConsumer(NotificationDbContext db) => _db = db;

        public async Task Consume(ConsumeContext<SendNotification> context)
        {
            var notif = await _db.Notifications.FindAsync(context.Message.NotificationId);
            if (notif == null || notif.Canceled)
                return;

            var now = DateTime.UtcNow;

            if (!notif.ForceSend && notif.ScheduledTime > now)
            {
                Console.WriteLine($"[REQUEUE] {notif.Id} – too early, requeueing in 10s");

                await Task.Delay(5000);
                await context.Publish(new SendNotification(notif.Id));

                return;
            }

            try
            {
                var localZone = TimeZoneInfo.FindSystemTimeZoneById(notif.TimeZone);
                var localTime = TimeZoneInfo.ConvertTimeFromUtc(now, localZone);
                if (localTime.Hour < 6 || localTime.Hour >= 22)
                {
                    Console.WriteLine($"[SKIP] {notif.Id} – outside local time: {localTime}");
                    return;
                }
            }
            catch
            {
                Console.WriteLine($"[ERROR] Invalid timezone for {notif.Id}: {notif.TimeZone}");
                return;
            }

            notif.Status = new Random().NextDouble() > 0.5 ? NotificationStatus.Sent : NotificationStatus.Failed;
            notif.ForceSend = false;

            await _db.SaveChangesAsync();
            Console.WriteLine($"[INFO] Notification {notif.Id} processed: {notif.Status}");
        }
    }
}
