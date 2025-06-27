using System;
using System.Windows;
using System.Windows.Controls;
using CrmSystem.ViewModels;

namespace CrmSystem.Views
{
    public partial class RegisterControl : UserControl
    {
        private readonly RegisterViewModel _viewModel;

        public Action SwitchToLogin { get; set; }

        public RegisterControl()
        {
            InitializeComponent();

            _viewModel = new RegisterViewModel();
            this.DataContext = _viewModel;

            _viewModel.OnError += message =>
            {
                MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            _viewModel.OnSuccess += () =>
            {
                MessageBox.Show("Регистрация успешна! Теперь вы можете войти. На вашу почту было отправлено письмо!",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                SwitchToLogin?.Invoke();
            };
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchToLogin?.Invoke();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Username = UsernameTextBox.Text.Trim();
            _viewModel.Email = EmailTextBox.Text.Trim();
            _viewModel.Password = PasswordBox.Password;
            _viewModel.ConfirmPassword = ConfirmPasswordBox.Password;

            _viewModel.Register();
        }
    }
}
