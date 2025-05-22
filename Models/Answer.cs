using System.Collections.Generic;

namespace DrivingLicenseQuiz.API.Models
{
    public class Answer
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string Text { get; set; }
        public bool IsCorrect { get; set; }

        // Navigation properties
        public virtual Question Question { get; set; }
        public virtual ICollection<UserAnswer> UserAnswers { get; set; }

        public Answer()
        {
            UserAnswers = new HashSet<UserAnswer>();
        }
    }
} 