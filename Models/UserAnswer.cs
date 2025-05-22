using System;

namespace DrivingLicenseQuiz.API.Models
{
    public class UserAnswer
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public int QuestionId { get; set; }
        public int AnswerId { get; set; }
        public DateTime AnsweredAt { get; set; }
        public bool IsCorrect { get; set; }

        // Navigation properties
        public virtual Quiz Quiz { get; set; }
        public virtual Question Question { get; set; }
        public virtual Answer Answer { get; set; }

        public UserAnswer()
        {
            AnsweredAt = DateTime.UtcNow;
        }
    }
} 