using System;

namespace DrivingLicenseQuiz.API.Models
{
    public class Session
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string SessionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; }

        // Navigation property
        public virtual User User { get; set; }

        public Session()
        {
            CreatedAt = DateTime.UtcNow;
            ExpiresAt = CreatedAt.AddDays(1); // Sessions expire after 24 hours
            IsActive = true;
        }
    }
} 