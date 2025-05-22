using System.Collections.Generic;

namespace DrivingLicenseQuiz.API.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }

        // Navigation property
        public virtual ICollection<Answer> Answers { get; set; }
        public virtual ICollection<QuizQuestion> QuizQuestions { get; set; }

        public Question()
        {
            Answers = new HashSet<Answer>();
            QuizQuestions = new HashSet<QuizQuestion>();
            IsActive = true;
        }
    }
} 