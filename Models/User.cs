using System;
using System.Collections.Generic;

namespace DrivingLicenseQuiz.API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Navigation properties
        public virtual ICollection<Session> Sessions { get; set; }
        public virtual ICollection<Quiz> Quizzes { get; set; }

        public User()
        {
            CreatedAt = DateTime.UtcNow;
            Sessions = new HashSet<Session>();
            Quizzes = new HashSet<Quiz>();
        }
    }
} 