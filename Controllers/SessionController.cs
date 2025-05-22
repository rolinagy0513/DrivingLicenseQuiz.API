using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DrivingLicenseQuiz.API.Data;
using DrivingLicenseQuiz.API.Models;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

namespace DrivingLicenseQuiz.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly QuizDbContext _context;

        public SessionController(QuizDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email already registered");

            // Generate salt and hash password
            string salt = BCrypt.Net.BCrypt.GenerateSalt();
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, salt);

            var user = new User
            {
                Email = request.Email,
                Name = request.Name,
                PasswordHash = passwordHash,
                Salt = salt
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create session for the new user
            var session = await CreateSession(user.Id);
            return Ok(new { sessionId = session.SessionId });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                return Unauthorized("Invalid email or password");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password");

            // Update last login time
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Create new session
            var session = await CreateSession(user.Id);
            return Ok(new { sessionId = session.SessionId });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var sessionId = Request.Cookies["SessionId"];
            if (string.IsNullOrEmpty(sessionId))
                return Ok();

            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive);

            if (session != null)
            {
                session.IsActive = false;
                await _context.SaveChangesAsync();
            }

            Response.Cookies.Delete("SessionId");
            return Ok();
        }

        [HttpGet("validate")]
        public async Task<IActionResult> ValidateSession()
        {
            var sessionId = Request.Cookies["SessionId"];
            if (string.IsNullOrEmpty(sessionId))
                return Unauthorized();

            var session = await _context.Sessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive);

            if (session == null || session.ExpiresAt < DateTime.UtcNow)
            {
                if (session != null)
                {
                    session.IsActive = false;
                    await _context.SaveChangesAsync();
                }
                Response.Cookies.Delete("SessionId");
                return Unauthorized();
            }

            return Ok(new { 
                userId = session.UserId,
                email = session.User.Email,
                name = session.User.Name
            });
        }

        private async Task<Session> CreateSession(int userId)
        {
            var session = new Session
            {
                UserId = userId,
                SessionId = GenerateSessionId(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                IsActive = true
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            // Set session cookie
            Response.Cookies.Append("SessionId", session.SessionId, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = session.ExpiresAt
            });

            return session;
        }

        private string GenerateSessionId()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[32];
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes);
            }
        }
    }

    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
} 