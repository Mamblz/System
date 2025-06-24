using System;
using System.Linq;
using System.Windows;
using CrmSystem.Data;
using CrmSystem.Models;
using CrmSystem.Views;
using Microsoft.EntityFrameworkCore;

namespace CrmSystem
{
    public partial class MainWindow : Window
    {
        private User _currentUser;
        private readonly ApplicationDbContext _dbContext;

        public MainWindow()
        {
            InitializeComponent();

            _dbContext = new ApplicationDbContext();
            _dbContext.Database.Migrate();

            ShowLogin();
        }

        private void ShowLogin()
        {
            var loginControl = new LoginControl();
            loginControl.SwitchToRegister += ShowRegister;
            loginControl.LoginSuccessful += OnLoginSuccessful;

            MainContent.Content = loginControl;
        }

        private void ShowRegister()
        {
            var registerControl = new RegisterControl();
            registerControl.SwitchToLogin += ShowLogin;

            MainContent.Content = registerControl;
        }

        private void OnLoginSuccessful(User user)
        {
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));
            ShowMainControl();
        }

        private void ShowMainControl()
        {
            var mainControl = new MainControl(_currentUser, _dbContext);

            mainControl.ResourcesRequested += ShowRequests;
            mainControl.UserProfileRequested += ShowUserProfile;
            mainControl.HomeRequested += ShowMainControl;
            mainControl.LogoutRequested += Logout;
            mainControl.NewDealRequested += ShowNewDealWindow;
            mainControl.AddClientRequested += ShowAddClientWindow;
            mainControl.TasksRequested += ShowTasksPage;


            MainContent.Content = mainControl;
        }


        private void ShowUserProfile()
        {
            var userProfileControl = new UserProfileControl(_currentUser, _dbContext);
            userProfileControl.BackToMainRequested += ShowMainControl;

            MainContent.Content = userProfileControl;
        }

        private void ShowTasksPage()
        {
            var tasksPage = new TasksPage(_dbContext);
            tasksPage.HomeRequested += ShowMainControl;
            MainContent.Content = tasksPage;
        }



        private void ShowRequests()
        {
            var requestsPage = new RequestsPage(_dbContext);
            requestsPage.HomeRequested += ShowMainControl;
            MainContent.Content = requestsPage;
        }




        private void ShowAddClientWindow()
        {
            var addClientWindow = new NewClientWindow(_dbContext);
            bool? result = addClientWindow.ShowDialog();

            if (result == true)
            {
                var newClient = addClientWindow.NewClient;
                if (newClient != null)
                {
                    _dbContext.Clients.Add(newClient);
                    _dbContext.SaveChanges();
                }
                else
                {
                    MessageBox.Show("Ошибка: клиент не был создан.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void ShowNewDealWindow()
        {
            var selectClientWindow = new SelectClientWindow(_dbContext);
            if (selectClientWindow.ShowDialog() == true)
            {
                var selectedClient = selectClientWindow.SelectedClient;
                ShowNewDealWindow(selectedClient);
            }
            else
            {
                MessageBox.Show("Создание сделки отменено. Клиент не выбран.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ShowNewDealWindow(Client selectedClient)
        {
            if (selectedClient == null)
            {
                MessageBox.Show("Пожалуйста, выберите клиента для создания сделки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int clientId = selectedClient.Id;

            bool clientExists = _dbContext.Clients.Any(c => c.Id == clientId);

            if (_currentUser == null)
            {
                MessageBox.Show("Ошибка: текущий пользователь не определен.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int userId = _currentUser.Id;
            bool userExists = _dbContext.Users.Any(u => u.Id == userId);

            if (!clientExists)
            {
                MessageBox.Show($"Клиент с Id={clientId} не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!userExists)
            {
                MessageBox.Show($"Пользователь с Id={userId} не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dealWindow = new NewDealWindow(userId, clientId);
            bool? result = dealWindow.ShowDialog();

            if (result == true)
            {
                Deal newDeal = dealWindow.NewDeal;

                if (newDeal != null)
                {
                    _dbContext.Deals.Add(newDeal);
                    _dbContext.SaveChanges();
                    MessageBox.Show("Сделка успешно создана!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Ошибка: сделка не была создана.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void Logout()
        {
            var result = MessageBox.Show("Вы действительно хотите выйти?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _currentUser = null;
                ShowLogin();
            }
        }
    }
}
