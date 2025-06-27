using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using CrmSystem.Services;

namespace CrmSystem.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        public RegisterViewModel(IAuthService authService)
        {
            _authService = authService;
        }
        public RegisterViewModel() : this(new AuthService()) { }

        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public event Action<string> OnError;
        public event Action OnSuccess;

        public void Register()
        {
            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                OnError?.Invoke("Заполните все обязательные поля.");
                return;
            }

            if (!IsValidEmail(Email))
            {
                OnError?.Invoke("Введите корректный email.");
                return;
            }

            if (Password != ConfirmPassword)
            {
                OnError?.Invoke("Пароли не совпадают.");
                return;
            }

            if (!IsPasswordStrong(Password))
            {
                OnError?.Invoke("Пароль должен быть не менее 8 символов, содержать буквы и цифры.");
                return;
            }

            var registeredAt = DateTime.UtcNow;

            bool result = _authService.Register(Username, Email, Password, registeredAt, out string errorMessage);

            if (!result)
            {
                OnError?.Invoke(errorMessage);
                return;
            }

            OnSuccess?.Invoke();
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        private bool IsPasswordStrong(string password)
        {
            return password.Length >= 8 &&
                   Regex.IsMatch(password, "[0-9]") &&
                   Regex.IsMatch(password, "[a-zA-Z]");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string prop = null)
        {
            if (!Equals(field, value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}
