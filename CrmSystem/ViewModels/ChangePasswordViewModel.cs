using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CrmSystem.Models;
using CrmSystem.Services;

namespace CrmSystem.ViewModels
{
    public interface IMessageBoxService
    {
        void Show(string message, string caption, MessageBoxButton buttons, MessageBoxImage icon);
    }

    public class MessageBoxService : IMessageBoxService
    {
        public void Show(string message, string caption, MessageBoxButton buttons, MessageBoxImage icon)
        {
            System.Windows.MessageBox.Show(message, caption, buttons, icon);
        }
    }

    public class ChangePasswordViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService;
        private readonly IMessageBoxService _messageBoxService;
        private readonly User _user;

        private string _oldPassword;
        private string _newPassword;
        private string _confirmPassword;

        public string OldPassword
        {
            get => _oldPassword;
            set { _oldPassword = value; OnPropertyChanged(); }
        }

        public string NewPassword
        {
            get => _newPassword;
            set { _newPassword = value; OnPropertyChanged(); }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action RequestClose;

        public ChangePasswordViewModel(
            User user,
            AuthService authService = null,
            IMessageBoxService messageBoxService = null)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _authService = authService ?? new AuthService();
            _messageBoxService = messageBoxService ?? new MessageBoxService();

            SaveCommand = new RelayCommand(_ => ExecuteSave());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke());
        }

        private void ExecuteSave()
        {
            if (string.IsNullOrWhiteSpace(OldPassword) ||
                string.IsNullOrWhiteSpace(NewPassword) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                _messageBoxService.Show("Все поля обязательны.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                _messageBoxService.Show("Новые пароли не совпадают.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var loggedUser = _authService.Login(_user.Username, OldPassword);
            if (loggedUser == null)
            {
                _messageBoxService.Show("Старый пароль неверный.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _user.PasswordHash = _authService.GetHashedPassword(NewPassword);
            _authService.UpdateUserPassword(_user);

            _messageBoxService.Show("Пароль успешно изменен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            RequestClose?.Invoke();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
