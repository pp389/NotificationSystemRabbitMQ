using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationSystem.Models;
using NotificationSystem.Contracts;

namespace NotificationSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationDbContext _db;
        private readonly IPublishEndpoint _bus;
        public NotificationController(NotificationDbContext db, IPublishEndpoint bus)
        {
            _db = db;
            _bus = bus;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Notification notification)
        {
            if (notification.ScheduledTime == default)
                notification.ScheduledTime = DateTime.UtcNow.AddSeconds(3);

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();

            await _bus.Publish(new SendNotification(notification.Id));
            return Ok(notification);
        }

        [HttpGet]
        public async Task<IActionResult> Get() =>
            Ok(await _db.Notifications.ToListAsync());

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var notif = await _db.Notifications.FindAsync(id);
            if (notif == null) return NotFound();
            notif.Canceled = true;
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("{id}/force")]
        public async Task<IActionResult> ForceSend(int id)
        {
            var notif = await _db.Notifications.FindAsync(id);
            if (notif == null) return NotFound();
            notif.ForceSend = true;
            await _db.SaveChangesAsync();
            await _bus.Publish(new SendNotification(id));
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var notif = await _db.Notifications.FindAsync(id);
            return notif != null ? Ok(notif) : NotFound();
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            var grouped = _db.Notifications
                .AsEnumerable()
                .GroupBy(n => n.Status)
                .ToDictionary(
                    g => g.Key.ToString(),
                    g => g.Select(n => new {
                        n.Id,
                        n.Content,
                        n.Channel,
                        n.Status,
                        n.Priority,
                        n.ScheduledTime,
                        n.TimeZone
                    }).ToList()
                );

            return Ok(grouped);
        }

    }
}