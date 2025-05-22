using Microsoft.EntityFrameworkCore;
using DrivingLicenseQuiz.API.Models;

namespace DrivingLicenseQuiz.API.Data
{
    public static class DbSeeder
    {
        public static async Task SeedDatabase(QuizDbContext context)
        {
            // Only seed if there are no questions
            if (await context.Questions.AnyAsync())
                return;

            var questions = new List<Question>
            {
                new Question
                {
                    Text = "What does a red traffic light mean?",
                    ImageUrl = "",
                    IsActive = true,
                    Answers = new List<Answer>
                    {
                        new Answer { Text = "Stop", IsCorrect = true },
                        new Answer { Text = "Proceed with caution", IsCorrect = false },
                        new Answer { Text = "Speed up", IsCorrect = false },
                        new Answer { Text = "Turn right", IsCorrect = false }
                    }
                },
                new Question
                {
                    Text = "What is the speed limit in a residential area?",
                    ImageUrl = "",
                    IsActive = true,
                    Answers = new List<Answer>
                    {
                        new Answer { Text = "30 km/h", IsCorrect = true },
                        new Answer { Text = "50 km/h", IsCorrect = false },
                        new Answer { Text = "70 km/h", IsCorrect = false },
                        new Answer { Text = "90 km/h", IsCorrect = false }
                    }
                },
                new Question
                {
                    Text = "When should you use your horn?",
                    ImageUrl = "",
                    IsActive = true,
                    Answers = new List<Answer>
                    {
                        new Answer { Text = "To warn other drivers of danger", IsCorrect = true },
                        new Answer { Text = "To greet other drivers", IsCorrect = false },
                        new Answer { Text = "To express frustration", IsCorrect = false },
                        new Answer { Text = "To get through traffic faster", IsCorrect = false }
                    }
                },
                new Question
                {
                    Text = "What does a yellow traffic light mean?",
                    ImageUrl = "",
                    IsActive = true,
                    Answers = new List<Answer>
                    {
                        new Answer { Text = "Prepare to stop", IsCorrect = true },
                        new Answer { Text = "Speed up to cross", IsCorrect = false },
                        new Answer { Text = "Continue at normal speed", IsCorrect = false },
                        new Answer { Text = "Turn left", IsCorrect = false }
                    }
                },
                new Question
                {
                    Text = "What should you do when approaching a school bus with flashing lights?",
                    ImageUrl = "",
                    IsActive = true,
                    Answers = new List<Answer>
                    {
                        new Answer { Text = "Stop and wait until lights stop flashing", IsCorrect = true },
                        new Answer { Text = "Speed up to pass quickly", IsCorrect = false },
                        new Answer { Text = "Honk your horn", IsCorrect = false },
                        new Answer { Text = "Drive around the bus", IsCorrect = false }
                    }
                },
                new Question
                {
                    Text = "What is the meaning of a solid white line on the road?",
                    ImageUrl = "",
                    IsActive = true,
                    Answers = new List<Answer>
                    {
                        new Answer { Text = "Do not cross the line", IsCorrect = true },
                        new Answer { Text = "You may cross with caution", IsCorrect = false },
                        new Answer { Text = "Passing is allowed", IsCorrect = false },
                        new Answer { Text = "Merge ahead", IsCorrect = false }
                    }
                },
                new Question
                {
                    Text = "When should you use your high beam headlights?",
                    ImageUrl = "",
                    IsActive = true,
                    Answers = new List<Answer>
                    {
                        new Answer { Text = "On dark, unlit roads with no oncoming traffic", IsCorrect = true },
                        new Answer { Text = "In heavy rain", IsCorrect = false },
                        new Answer { Text = "In foggy conditions", IsCorrect = false },
                        new Answer { Text = "When following another vehicle", IsCorrect = false }
                    }
                },
                new Question
                {
                    Text = "What should you do when your vehicle starts to skid?",
                    ImageUrl = "",
                    IsActive = true,
                    Answers = new List<Answer>
                    {
                        new Answer { Text = "Steer in the direction of the skid", IsCorrect = true },
                        new Answer { Text = "Brake hard", IsCorrect = false },
                        new Answer { Text = "Accelerate to regain control", IsCorrect = false },
                        new Answer { Text = "Turn the wheel sharply in the opposite direction", IsCorrect = false }
                    }
                },
                new Question
                {
                    Text = "What does a blue traffic sign indicate?",
                    ImageUrl = "",
                    IsActive = true,
                    Answers = new List<Answer>
                    {
                        new Answer { Text = "Information or guidance", IsCorrect = true },
                        new Answer { Text = "Warning", IsCorrect = false },
                        new Answer { Text = "Prohibition", IsCorrect = false },
                        new Answer { Text = "Mandatory action", IsCorrect = false }
                    }
                },
                new Question
                {
                    Text = "What is the minimum safe following distance in good weather?",
                    ImageUrl = "",
                    IsActive = true,
                    Answers = new List<Answer>
                    {
                        new Answer { Text = "2 seconds", IsCorrect = true },
                        new Answer { Text = "1 second", IsCorrect = false },
                        new Answer { Text = "3 car lengths", IsCorrect = false },
                        new Answer { Text = "5 meters", IsCorrect = false }
                    }
                }
            };

            await context.Questions.AddRangeAsync(questions);
            await context.SaveChangesAsync();
        }
    }
} 