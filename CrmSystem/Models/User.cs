using System;
using System.Collections.Generic;

namespace CrmSystem.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public string? Position { get; set; }
        public string? Department { get; set; }
        public bool IsEmailVerified { get; set; }
        public string? AvatarPath { get; set; }

        public DateTime RegisteredAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public string EmailConfirmationCode { get; set; } = string.Empty;
        public DateTime EmailConfirmationExpiry { get; set; }

        public List<string> ProfileChangeLog { get; set; } = new();
        public List<TaskItem> Tasks { get; set; } = new();
        public List<string> Comments { get; set; } = new();
    }

    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }
    }
}
