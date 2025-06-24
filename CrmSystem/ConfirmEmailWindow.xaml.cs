using CrmSystem.Models; // Добавь, если используешь User
using CrmSystem.Services;
using System.Windows;

namespace CrmSystem.Views
{
    public partial class ConfirmEmailWindow : Window
    {
        private readonly AuthService _authService;
        private readonly User _user;

        public ConfirmEmailWindow(User user)  // <-- конструктор с параметром
        {
            InitializeComponent();
            _authService = new AuthService();
            _user = user;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            string token = TokenTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(token))
            {
                ResultTextBlock.Text = "Пожалуйста, введите код подтверждения.";
                return;
            }

            bool result = _authService.ConfirmEmail(token);

            if (result)
            {
                _user.IsEmailVerified = true; // обновляем флаг
                MessageBox.Show("Email успешно подтвержден!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            else
            {
                ResultTextBlock.Text = "Неверный или просроченный код подтверждения.";
            }
        }
    }
}
