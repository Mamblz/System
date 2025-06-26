using System.Windows;
using System.Windows.Controls;
using CrmSystem.Data;
using CrmSystem.ViewModels;
using CrmSystem.Models;

namespace CrmSystem.Views
{
    public partial class RequestsPage : UserControl
    {
        private readonly RequestsPageViewModel _viewModel;
        public event Action HomeRequested;

        public RequestsPage(ApplicationDbContext dbContext)
        {
            InitializeComponent();

            _viewModel = new RequestsPageViewModel(dbContext);
            DataContext = _viewModel;

            RequestsListView.ItemsSource = _viewModel.FilteredTickets;
            _viewModel.LoadTicketsFromDb();
        }

        private void MainPage_Click(object sender, RoutedEventArgs e)
        {
            HomeRequested?.Invoke();
        }

        private void CreateRequest_Click(object sender, RoutedEventArgs e)
        {
            var newRequestWindow = new NewRequestWindow(_viewModel.DbContext)
            {
                Owner = Window.GetWindow(this)
            };

            if (newRequestWindow.ShowDialog() == true)
            {
                _viewModel.CreateTicket(newRequestWindow.NewTicket);
                MessageBox.Show("Заявка создана!");
            }
        }

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StatusFilterComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var statusText = selectedItem.Content.ToString();

                _viewModel.SelectedStatus = statusText switch
                {
                    "Все статусы" => null,
                    "Новая" => TicketStatus.Новый,
                    "В процессе" => TicketStatus.ВПроцессе,
                    "В ожидании" => TicketStatus.ВОжидании,
                    "Завершена" => TicketStatus.Завершён,
                    "Отменена" => TicketStatus.Отменён,
                    _ => null
                };

                _viewModel.ApplyFilter();
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel.SearchText = SearchTextBox.Text;
            _viewModel.ApplyFilter();
        }

        private void EditRequest_Click(object sender, RoutedEventArgs e)
        {
            if (RequestsListView.SelectedItem is Ticket selectedTicket)
            {
                var editWindow = new NewRequestWindow(_viewModel.DbContext, selectedTicket)
                {
                    Owner = Window.GetWindow(this)
                };

                if (editWindow.ShowDialog() == true)
                {
                    _viewModel.LoadTicketsFromDb();
                    MessageBox.Show($"Заявка '{selectedTicket.Title}' успешно обновлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите заявку для редактирования.", "Редактирование", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteRequest_Click(object sender, RoutedEventArgs e)
        {
            if (RequestsListView.SelectedItem is Ticket selectedTicket)
            {
                var result = MessageBox.Show($"Вы действительно хотите удалить заявку '{selectedTicket.Title}'?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    if (_viewModel.DeleteTicket(selectedTicket))
                    {
                        MessageBox.Show("Заявка успешно удалена.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при удалении заявки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите заявку для удаления.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
