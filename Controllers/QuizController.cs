using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DrivingLicenseQuiz.API.Data;
using DrivingLicenseQuiz.API.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace DrivingLicenseQuiz.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly QuizDbContext _context;
        private readonly Random _random = new Random();

        public QuizController(QuizDbContext context)
        {
            _context = context;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartQuiz()
        {
            try
            {
                var sessionId = Request.Cookies["SessionId"];
                Console.WriteLine($"StartQuiz called. Session cookie present: {!string.IsNullOrEmpty(sessionId)}");
                
                if (string.IsNullOrEmpty(sessionId))
                {
                    Console.WriteLine("Session cookie missing");
                    return BadRequest("Session cookie missing.");
                }

                var session = await _context.Sessions
                    .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive);

                Console.WriteLine($"Session found: {session != null}, IsActive: {session?.IsActive}, UserId: {session?.UserId}");

                if (session == null)
                {
                    Console.WriteLine("Session invalid or expired");
                    return BadRequest("Session invalid or expired.");
                }

                // Get total count of questions first
                var totalQuestions = await _context.Questions.CountAsync();
                Console.WriteLine($"Total questions in database: {totalQuestions}");

                // Get count of active questions
                var activeQuestions = await _context.Questions.Where(q => q.IsActive).CountAsync();
                Console.WriteLine($"Active questions in database: {activeQuestions}");

                // Log all questions for debugging
                var allQuestions = await _context.Questions
                    .Include(q => q.Answers)
                    .ToListAsync();
                
                Console.WriteLine("All questions in database:");
                foreach (var q in allQuestions)
                {
                    Console.WriteLine($"Question {q.Id}: {q.Text} (Active: {q.IsActive}, Answers: {q.Answers.Count})");
                }

                // Get all active questions and randomize in memory
                var questions = await _context.Questions
                    .Where(q => q.IsActive)
                    .Include(q => q.Answers)
                    .ToListAsync();

                // Randomize the questions in memory
                questions = questions.OrderBy(q => _random.Next()).ToList();

                // Take first 10 questions (or all if less than 10)
                questions = questions.Take(10).ToList();

                Console.WriteLine($"Questions retrieved for quiz: {questions.Count}");

                if (questions.Count < 10)
                {
                    Console.WriteLine($"Not enough active questions available (only {questions.Count} found)");
                    return BadRequest($"Not enough active questions available (only {questions.Count} found). Please contact the administrator.");
                }

                // Create new quiz
                var quiz = new Quiz
                {
                    UserId = session.UserId,
                    StartedAt = DateTime.UtcNow,
                    TimeLimitMinutes = 15
                };

                _context.Quizzes.Add(quiz);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Created new quiz with ID: {quiz.Id}");

                // Add questions to quiz in random order
                for (int i = 0; i < questions.Count; i++)
                {
                    var quizQuestion = new QuizQuestion
                    {
                        QuizId = quiz.Id,
                        QuestionId = questions[i].Id,
                        OrderIndex = i
                    };
                    _context.QuizQuestions.Add(quizQuestion);
                }

                await _context.SaveChangesAsync();
                Console.WriteLine("Added questions to quiz");

                // Return quiz questions with randomized answers
                var quizQuestions = questions.Select(q => new
                {
                    q.Id,
                    q.Text,
                    q.ImageUrl,
                    Answers = q.Answers
                        .OrderBy(a => _random.Next()) // Randomize answers in memory
                        .Select(a => new { a.Id, a.Text })
                });

                Console.WriteLine("Returning quiz data to client");
                return Ok(new
                {
                    quizId = quiz.Id,
                    timeLimitMinutes = quiz.TimeLimitMinutes,
                    questions = quizQuestions
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in StartQuiz: {ex}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest($"An error occurred while fetching the quiz data: {ex.Message}");
            }
        }

        [HttpPost("{quizId}/submit")]
        public async Task<IActionResult> SubmitQuiz(int quizId, [FromBody] List<QuizAnswer> answers)
        {
            var sessionId = Request.Cookies["SessionId"];
            if (string.IsNullOrEmpty(sessionId))
                return Unauthorized();

            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive);

            if (session == null)
                return Unauthorized();

            var quiz = await _context.Quizzes
                .Include(q => q.QuizQuestions)
                .FirstOrDefaultAsync(q => q.Id == quizId && q.UserId == session.UserId);

            if (quiz == null)
                return NotFound("Quiz not found");

            if (quiz.IsCompleted)
                return BadRequest("Quiz already completed");

            // Validate answers
            var correctAnswers = 0;
            foreach (var answer in answers)
            {
                var question = await _context.Questions
                    .Include(q => q.Answers)
                    .FirstOrDefaultAsync(q => q.Id == answer.QuestionId);

                if (question == null)
                    continue;

                var selectedAnswer = question.Answers.FirstOrDefault(a => a.Id == answer.AnswerId);
                if (selectedAnswer == null)
                    continue;

                var isCorrect = selectedAnswer.IsCorrect;
                if (isCorrect)
                    correctAnswers++;

                var userAnswer = new UserAnswer
                {
                    QuizId = quizId,
                    QuestionId = answer.QuestionId,
                    AnswerId = answer.AnswerId,
                    IsCorrect = isCorrect
                };

                _context.UserAnswers.Add(userAnswer);
            }

            // Update quiz status
            quiz.IsCompleted = true;
            quiz.CompletedAt = DateTime.UtcNow;
            quiz.Score = correctAnswers;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                score = correctAnswers,
                totalQuestions = answers.Count,
                completedAt = quiz.CompletedAt
            });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetQuizHistory()
        {
            var sessionId = Request.Cookies["SessionId"];
            if (string.IsNullOrEmpty(sessionId))
                return Unauthorized();

            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive);

            if (session == null)
                return Unauthorized();

            var quizzes = await _context.Quizzes
                .Where(q => q.UserId == session.UserId && q.IsCompleted)
                .OrderByDescending(q => q.CompletedAt)
                .Select(q => new
                {
                    q.Id,
                    q.StartedAt,
                    q.CompletedAt,
                    q.Score,
                    TotalQuestions = q.QuizQuestions.Count
                })
                .ToListAsync();

            return Ok(quizzes);
        }

        [HttpGet("{quizId}")]
        public async Task<IActionResult> GetQuizDetails(int quizId)
        {
            try
            {
                var sessionId = Request.Cookies["SessionId"];
                if (string.IsNullOrEmpty(sessionId))
                    return Unauthorized();

                var session = await _context.Sessions
                    .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive);

                if (session == null)
                    return Unauthorized();

                // Get the quiz with only wrong answers and their questions
                var quiz = await _context.Quizzes
                    .Include(q => q.QuizQuestions)
                        .ThenInclude(qq => qq.Question)
                            .ThenInclude(q => q.Answers)
                    .Include(q => q.UserAnswers)
                        .ThenInclude(ua => ua.Answer)
                    .FirstOrDefaultAsync(q => q.Id == quizId && q.UserId == session.UserId);

                if (quiz == null)
                    return NotFound("Quiz not found");

                // Get only questions with wrong answers
                var wrongAnswers = quiz.UserAnswers
                    .Where(ua => !ua.IsCorrect)
                    .ToList();

                if (!wrongAnswers.Any())
                {
                    // If there are no wrong answers, return a success message
                    return Ok(new
                    {
                        quiz.Id,
                        quiz.Score,
                        quiz.CompletedAt,
                        message = "Congratulations! You got all answers correct!",
                        questions = new List<object>() // Empty list since there are no wrong answers
                    });
                }

                // Get the questions and their correct answers for the wrong answers
                var wrongQuestions = quiz.QuizQuestions
                    .Where(qq => wrongAnswers.Any(ua => ua.QuestionId == qq.QuestionId))
                    .Select(qq => new
                    {
                        qq.Question.Id,
                        qq.Question.Text,
                        qq.Question.ImageUrl,
                        UserAnswer = wrongAnswers
                            .Where(ua => ua.QuestionId == qq.QuestionId)
                            .Select(ua => new
                            {
                                ua.Answer.Text,
                                ua.IsCorrect
                            })
                            .FirstOrDefault(),
                        CorrectAnswer = qq.Question.Answers
                            .Where(a => a.IsCorrect)
                            .Select(a => new { a.Text })
                            .FirstOrDefault()
                    })
                    .OrderBy(q => q.Id) // Order by question ID for consistency
                    .ToList();

                var details = new
                {
                    quiz.Id,
                    quiz.Score,
                    quiz.CompletedAt,
                    TotalQuestions = quiz.QuizQuestions.Count,
                    WrongAnswersCount = wrongAnswers.Count,
                    message = $"You got {wrongAnswers.Count} questions wrong. Here are the correct answers:",
                    questions = wrongQuestions
                };

                return Ok(details);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetQuizDetails: {ex}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest($"An error occurred while fetching quiz details: {ex.Message}");
            }
        }
    }

    public class QuizAnswer
    {
        public int QuestionId { get; set; }
        public int AnswerId { get; set; }
    }
} 