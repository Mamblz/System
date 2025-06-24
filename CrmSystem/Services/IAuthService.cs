using CrmSystem.Models;
using System;

namespace CrmSystem.Services
{
    public interface IAuthService
    {
        bool Register(string username, string email, string password, DateTime registeredAt, out string errorMessage);
        User? Login(string usernameOrEmail, string password);
        string GetHashedPassword(string password);
        void UpdateUserPassword(User user);
        bool SendConfirmationEmail(string userEmail, string confirmationToken);
        bool ConfirmEmail(string token);
    }
}
