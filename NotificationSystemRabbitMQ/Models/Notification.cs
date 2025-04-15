using System;

namespace NotificationSystem.Models
{
    public enum ChannelType { Email, Push }
    public enum NotificationStatus { Pending, Sent, Failed }
    public enum NotificationPriority { Low, High }

    public class Notification
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public ChannelType Channel { get; set; }
        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
        public string Recipient { get; set; }
        public string TimeZone { get; set; }
        public DateTime ScheduledTime { get; set; }
        public NotificationPriority Priority { get; set; } = NotificationPriority.Low;
        public bool ForceSend { get; set; } = false;
        public bool Canceled { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}