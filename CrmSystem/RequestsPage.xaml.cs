using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CrmSystem.Data;
using CrmSystem.Models;

namespace CrmSystem.Views
{
    public partial class RequestsPage : UserControl
    {
        private readonly ApplicationDbContext _dbContext;
        private ObservableCollection<Ticket> Tickets { get; set; }
        private ObservableCollection<Ticket> FilteredTickets { get; set; }

        public event Action HomeRequested;

        public RequestsPage(ApplicationDbContext dbContext)
        {
            InitializeComponent();

            _dbContext = dbContext;

            // Инициализируем коллекции
            Tickets = new ObservableCollection<Ticket>();
            FilteredTickets = new ObservableCollection<Ticket>();

            // Привязываем FilteredTickets к ListView
            RequestsListView.ItemsSource = FilteredTickets;

            // Загружаем данные из базы
            LoadTicketsFromDb();
        }

        private void MainPage_Click(object sender, RoutedEventArgs e)
        {
            HomeRequested?.Invoke();
        }

        private readonly Dictionary<string, TicketStatus?> _statusMap = new Dictionary<string, TicketStatus?>()
        {
            { "Все статусы", null },
            { "Новая", TicketStatus.Новый },
            { "В процессе", TicketStatus.ВПроцессе },
            { "В ожидании", TicketStatus.ВОжидании },
            { "Завершена", TicketStatus.Завершён },
            { "Отменена", TicketStatus.Отменён }
        };

        private void DeleteRequest_Click(object sender, RoutedEventArgs e)
        {
            if (RequestsListView.SelectedItem is Ticket selectedTicket)
            {
                var result = MessageBox.Show($"Вы действительно хотите удалить заявку '{selectedTicket.Title}'?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _dbContext.Tickets.Remove(selectedTicket);
                        _dbContext.SaveChanges();

                        Tickets.Remove(selectedTicket);
                        FilteredTickets.Remove(selectedTicket);

                        MessageBox.Show("Заявка успешно удалена.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении заявки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите заявку для удаления.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EditRequest_Click(object sender, RoutedEventArgs e)
        {
            if (RequestsListView.SelectedItem is Ticket selectedTicket)
            {
                var editWindow = new NewRequestWindow(_dbContext, selectedTicket)
                {
                    Owner = Window.GetWindow(this)
                };

                if (editWindow.ShowDialog() == true)
                {
                    RefreshFilter();
                    MessageBox.Show($"Заявка '{selectedTicket.Title}' успешно обновлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите заявку для редактирования.", "Редактирование", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LoadTicketsFromDb()
        {
            Tickets.Clear();
            var ticketsFromDb = _dbContext.Tickets.ToList();

            foreach (var ticket in ticketsFromDb)
            {
                Tickets.Add(ticket);
            }

            RefreshFilter();
        }

        private void CreateRequest_Click(object sender, RoutedEventArgs e)
        {
            var newRequestWindow = new NewRequestWindow(_dbContext)
            {
                Owner = Window.GetWindow(this)
            };

            if (newRequestWindow.ShowDialog() == true)
            {
                var newTicket = newRequestWindow.NewTicket;
                Tickets.Add(newTicket);
                RefreshFilter();

                MessageBox.Show($"Заявка '{newTicket.Title}' успешно создана!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshFilter();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshFilter();
        }

        private void RefreshFilter()
        {
            if (Tickets == null) return;

            string searchText = SearchTextBox.Text?.Trim().ToLower() ?? string.Empty;
            string selectedStatus = (StatusFilterComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Все статусы";

            _statusMap.TryGetValue(selectedStatus, out TicketStatus? filterStatus);

            var filtered = Tickets.Where(t =>
                (filterStatus == null || t.Status == filterStatus) &&
                (string.IsNullOrEmpty(searchText) ||
                 (t.Title?.ToLower().Contains(searchText) == true || t.Description?.ToLower().Contains(searchText) == true))
            ).OrderByDescending(t => t.CreatedAt).ToList();

            FilteredTickets.Clear();
            foreach (var ticket in filtered)
            {
                FilteredTickets.Add(ticket);
            }
        }
    }
}
