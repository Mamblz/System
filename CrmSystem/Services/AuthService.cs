using CrmSystem.Data;
using CrmSystem.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CrmSystem.EmailSend;

namespace CrmSystem.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public AuthService()
        {
            _context = new ApplicationDbContext();
            _context.Database.EnsureCreated();
            _emailService = new EmailService();
        }

        public bool Register(string username, string email, string password, DateTime registeredAt, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "«аполните все об€зательные пол€.";
                return false;
            }

            if (_context.Users.Any(u => u.Username == username))
            {
                errorMessage = "ѕользователь с таким именем уже существует.";
                return false;
            }

            if (_context.Users.Any(u => u.Email == email))
            {
                errorMessage = "ѕользователь с таким email уже существует.";
                return false;
            }

            var confirmationCode = GenerateConfirmationCode();

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = HashPassword(password),
                RegisteredAt = registeredAt,
                IsEmailVerified = false,
                EmailConfirmationCode = confirmationCode,
                EmailConfirmationExpiry = DateTime.UtcNow.AddHours(24)
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            bool emailSent = SendConfirmationEmail(email, confirmationCode);
            if (!emailSent)
            {
                errorMessage = "ѕользователь зарегистрирован, но не удалось отправить письмо с подтверждением.";
                return false;
            }

            return true;
        }

        public bool Register(string username, string email, string password, DateTime registeredAt)
        {
            return Register(username, email, password, registeredAt, out _);
        }

        public User? Login(string usernameOrEmail, string password)
        {
            if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = _context.Users.FirstOrDefault(u =>
                u.Username == usernameOrEmail || u.Email == usernameOrEmail);

            if (user == null) return null;

            if (VerifyPassword(password, user.PasswordHash))
            {
                user.LastLoginAt = DateTime.Now;
                _context.SaveChanges();
                return user;
            }

            return null;
        }

        public string GetHashedPassword(string password) => HashPassword(password);

        public void UpdateUserPassword(User user)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Id == user.Id);
            if (existingUser != null)
            {
                existingUser.PasswordHash = user.PasswordHash;
                _context.SaveChanges();
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == storedHash;
        }

        private string GenerateConfirmationCode()
        {
            var rng = new Random();
            return rng.Next(100000, 999999).ToString();
        }

        public bool SendConfirmationEmail(string userEmail, string confirmationCode)
        {
            string subject = " од подтверждени€ электронной почты";
            string body = $"<p>¬аш код подтверждени€ дл€ подтверждени€ email:</p>" +
                          $"<h2>{confirmationCode}</h2>" +
                          $"<p>¬ведите этот код в приложении дл€ подтверждени€ вашей почты.</p>";

            return _emailService.SendEmail(userEmail, subject, body);
        }

        public bool ConfirmEmail(string code)
        {
            var user = _context.Users.FirstOrDefault(u => u.EmailConfirmationCode == code);

            if (user == null)
                return false;

            if (user.EmailConfirmationExpiry < DateTime.UtcNow)
                return false;

            user.IsEmailVerified = true;
            user.EmailConfirmationCode = string.Empty;
            _context.SaveChanges();

            return true;
        }
    }
}
