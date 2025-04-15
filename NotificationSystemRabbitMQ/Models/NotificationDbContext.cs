using Microsoft.EntityFrameworkCore;

namespace NotificationSystem.Models
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions options) : base(options) {}
        public DbSet<Notification> Notifications => Set<Notification>();
    }
}