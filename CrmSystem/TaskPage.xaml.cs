using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CrmSystem.Data;
using CrmSystem.Models;
using CrmSystem.ViewModels;

namespace CrmSystem.Views
{
    public partial class TasksPage : UserControl
    {
        private readonly TasksPageViewModel _viewModel;

        public event Action HomeRequested;

        public TasksPage(ApplicationDbContext dbContext)
        {
            InitializeComponent();
            _viewModel = new TasksPageViewModel(dbContext);
            this.DataContext = _viewModel;
        }

        private void CreateTask_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_viewModel.NewTaskTitle))
            {
                MessageBox.Show("Пожалуйста, введите название задачи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _viewModel.AddNewTask();
        }

        private void GoHome_Click(object sender, RoutedEventArgs e)
        {
            HomeRequested?.Invoke();
        }

        private void TaskItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Обработка только при нажатии левой кнопкой мыши
            if (e.ChangedButton == MouseButton.Left && sender is FrameworkElement fe && fe.DataContext is TasksItems clickedTask)
            {
                _viewModel.SelectedTask = clickedTask;
            }
        }

        private void DeleteSelectedTask_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedTask == null)
            {
                MessageBox.Show("Сначала выберите задачу для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить задачу \"{_viewModel.SelectedTask.Title}\"?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _viewModel.DeleteTask(_viewModel.SelectedTask.Id);
                _viewModel.SelectedTask = null;
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.RefreshTasks();
        }
    }
}
