using System;
using System.Collections.Generic;

namespace DrivingLicenseQuiz.API.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int TimeLimitMinutes { get; set; }
        public int Score { get; set; }
        public bool IsCompleted { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual ICollection<QuizQuestion> QuizQuestions { get; set; }
        public virtual ICollection<UserAnswer> UserAnswers { get; set; }

        public Quiz()
        {
            StartedAt = DateTime.UtcNow;
            QuizQuestions = new HashSet<QuizQuestion>();
            UserAnswers = new HashSet<UserAnswer>();
            TimeLimitMinutes = 15; // Default time limit
            IsCompleted = false;
        }
    }
} 