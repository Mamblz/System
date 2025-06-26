using System;
using System.Windows;
using System.Windows.Controls;
using CrmSystem.Data;
using CrmSystem.Models;
using CrmSystem.ViewModels;

namespace CrmSystem.Views
{
    public partial class MainControl : UserControl
    {
        private readonly User _currentUser;
        private readonly MainViewModel _viewModel;

        public event Action ResourcesRequested;
        public event Action UserProfileRequested;
        public event Action LogoutRequested;
        public event Action HomeRequested;
        public event Action BuildingsRequested;
        public event Action AddClientRequested;
        public event Action AddProjectRequested;
        public event Action NewDealRequested;
        public event Action TasksRequested;

        public MainControl(User user, ApplicationDbContext dbContext)
        {
            InitializeComponent();

            _currentUser = user ?? throw new ArgumentNullException(nameof(user));
            if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));

            // Используем обертку DispatcherWrapper, реализующую IDispatcher
            var dispatcherWrapper = new DispatcherWrapper(Application.Current.Dispatcher);

            _viewModel = new MainViewModel(() => new ApplicationDbContext(), dispatcherWrapper);
            this.DataContext = _viewModel;

            GreetingText.Text = $"Привет, {_currentUser.Username}!";

            Loaded += async (_, __) => await _viewModel.LoadDataFromDatabaseAsync();
        }

        private void ShowUserProfile_Click(object sender, RoutedEventArgs e)
        {
            UserProfileRequested?.Invoke();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            LogoutRequested?.Invoke();
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                await vm.LoadDataFromDatabaseAsync();
            }
        }

        private void AddClient_Click(object sender, RoutedEventArgs e)
        {
            AddClientRequested?.Invoke();
        }

        private void AddProject_Click(object sender, RoutedEventArgs e)
        {
            AddProjectRequested?.Invoke();
        }

        private void NewDeal_Click(object sender, RoutedEventArgs e)
        {
            NewDealRequested?.Invoke();
        }

        private void GoHome_Click(object sender, RoutedEventArgs e)
        {
            HomeRequested?.Invoke();
        }

        private void Requests_Click(object sender, RoutedEventArgs e)
        {
            ResourcesRequested?.Invoke();
        }

        private void Tasks_Click(object sender, RoutedEventArgs e)
        {
            TasksRequested?.Invoke();
        }
    }
}
