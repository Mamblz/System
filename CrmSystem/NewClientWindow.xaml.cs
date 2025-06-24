using System;
using System.Text.RegularExpressions;
using System.Windows;
using CrmSystem.Data;
using CrmSystem.Models;

namespace CrmSystem.Views
{
    public partial class NewClientWindow : Window
    {
        private readonly ApplicationDbContext _dbContext;

        public Client NewClient { get; private set; }

        public NewClientWindow(ApplicationDbContext dbContext)
        {
            InitializeComponent();
            _dbContext = dbContext;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите имя клиента.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(EmailBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите email клиента.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(EmailBox.Text.Trim(), emailPattern))
            {
                MessageBox.Show("Некорректный формат email.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var phonePattern = @"^\+?\d[\d\s\-\(\)]{9,14}\d$";
            if (!string.IsNullOrWhiteSpace(PhoneBox.Text) && !Regex.IsMatch(PhoneBox.Text.Trim(), phonePattern))
            {
                MessageBox.Show("Некорректный формат номера телефона.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                NewClient = new Client
                {
                    Name = NameBox.Text.Trim(),
                    Email = EmailBox.Text.Trim(),
                    Phone = PhoneBox.Text.Trim(),
                    Address = AddressBox.Text.Trim(),
                    Company = CompanyBox.Text.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                MessageBox.Show("Клиент успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении клиента:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
