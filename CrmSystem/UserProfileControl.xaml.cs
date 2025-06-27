using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using CrmSystem.Models;
using CrmSystem.Data;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Microsoft.Win32;
using CrmSystem.Views;

namespace CrmSystem.Views
{
    public partial class UserProfileControl : UserControl
    {
        private User _currentUser;
        private readonly ApplicationDbContext _dbContext;

        public event Action BackToMainRequested = delegate { };

        public UserProfileControl(User currentUser, ApplicationDbContext dbContext)
        {
            InitializeComponent();

            _currentUser = currentUser;
            _dbContext = dbContext;

            LoadUserData();
        }

        private void LoadUserData()
        {
            UserNameTextBox.Text = _currentUser.Username;
            EmailTextBox.Text = _currentUser.Email;
            PhoneTextBox.Text = _currentUser.PhoneNumber ?? "";
            PositionTextBox.Text = _currentUser.Position ?? "";
            DepartmentTextBox.Text = _currentUser.Department ?? "";

            UpdateEmailStatus();

            if (!string.IsNullOrEmpty(_currentUser.AvatarPath))
            {
                try
                {
                    AvatarImage.Source = new BitmapImage(new Uri(_currentUser.AvatarPath));
                }
                catch {}
            }

            RegisteredAtTextBlock.Text = _currentUser.RegisteredAt == DateTime.MinValue
                ? "Неизвестна"
                : $"Дата регистрации: {_currentUser.RegisteredAt:dd.MM.yyyy HH:mm}";

            LastLoginTextBlock.Text = (_currentUser.LastLoginAt == null || _currentUser.LastLoginAt == DateTime.MinValue)
                ? "Последний вход: никогда"
                : $"Последний вход: {_currentUser.LastLoginAt:dd.MM.yyyy HH:mm}";
        }

        private void UpdateEmailStatus()
        {
            EmailStatusTextBlock.Text = _currentUser.IsEmailVerified ? "✅ Подтвержден" : "🔴 Не подтвержден";
            EmailStatusTextBlock.Foreground = _currentUser.IsEmailVerified ? Brushes.Green : Brushes.Red;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            BackToMainRequested?.Invoke();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string newUsername = UserNameTextBox.Text.Trim();
            string newEmail = EmailTextBox.Text.Trim();
            string phone = PhoneTextBox.Text.Trim();
            string position = PositionTextBox.Text.Trim();
            string department = DepartmentTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(newUsername))
            {
                MessageBox.Show("Имя пользователя не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IsValidEmail(newEmail))
            {
                MessageBox.Show("Введите корректный email.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Если email изменён — сбрасываем подтверждение
            if (!string.Equals(_currentUser.Email, newEmail, StringComparison.OrdinalIgnoreCase))
            {
                _currentUser.IsEmailVerified = false;
                _currentUser.EmailConfirmationCode = string.Empty;
            }

            _currentUser.Username = newUsername;
            _currentUser.Email = newEmail;
            _currentUser.PhoneNumber = phone;
            _currentUser.Position = position;
            _currentUser.Department = department;

            try
            {
                _dbContext.Users.Update(_currentUser);
                _dbContext.SaveChanges();

                UpdateEmailStatus();

                StatusTextBlock.Visibility = Visibility.Visible;

                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(3)
                };
                timer.Tick += (s, args) =>
                {
                    StatusTextBlock.Visibility = Visibility.Collapsed;
                    timer.Stop();
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var passwordWindow = new ChangePasswordWindow(_currentUser);
            passwordWindow.ShowDialog();
        }

        private void ChangeEmail_Click(object sender, RoutedEventArgs e)
        {
            var changeEmailWindow = new ChangeEmailWindow(EmailTextBox.Text);
            if (changeEmailWindow.ShowDialog() == true)
            {
                EmailTextBox.Text = changeEmailWindow.NewEmail;
                _currentUser.IsEmailVerified = false;
                UpdateEmailStatus();
            }
        }

        private void ResendConfirmationEmail_Click(object sender, RoutedEventArgs e)
        {
            var authService = new CrmSystem.Services.AuthService();

            // Генерируем новый код подтверждения
            var newCode = GenerateNewConfirmationCode();

            _currentUser.EmailConfirmationCode = newCode;
            _currentUser.EmailConfirmationExpiry = DateTime.UtcNow.AddHours(24);
            _dbContext.Users.Update(_currentUser);
            _dbContext.SaveChanges();

            bool emailSent = authService.SendConfirmationEmail(_currentUser.Email, newCode);
            if (emailSent)
                MessageBox.Show("Письмо с подтверждением отправлено повторно.", "Подтверждение email", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show("Не удалось отправить письмо с подтверждением.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private string GenerateNewConfirmationCode()
        {
            var rng = new Random();
            return rng.Next(100000, 999999).ToString();
        }

        private void UploadAvatar_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var selectedFile = dlg.FileName;

                    _currentUser.AvatarPath = selectedFile;

                    AvatarImage.Source = new BitmapImage(new Uri(selectedFile));

                    _dbContext.Users.Update(_currentUser);
                    _dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке фото: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }

        private void ConfirmEmailButton_Click(object sender, RoutedEventArgs e)
        {
            var confirmEmailWindow = new ConfirmEmailWindow(_currentUser);
            bool? dialogResult = confirmEmailWindow.ShowDialog();

            if (dialogResult == true)
            {
                // После подтверждения обновляем статус
                _dbContext.Users.Update(_currentUser);
                _dbContext.SaveChanges();

                UpdateEmailStatus();
            }
        }
    }
}
