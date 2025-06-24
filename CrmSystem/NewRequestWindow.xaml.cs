using System;
using System.Linq;
using System.Windows;
using CrmSystem.Models;
using CrmSystem.Data;

namespace CrmSystem.Views
{
    public partial class NewRequestWindow : Window
    {
        private readonly ApplicationDbContext _dbContext;
        private Ticket _editingTicket;

        public Ticket NewTicket { get; private set; }

        // Конструктор для создания новой заявки
        public NewRequestWindow(ApplicationDbContext dbContext)
        {
            InitializeComponent();

            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

            InitializeComboBoxes();

            SaveButton.Content = "Сохранить";
            Title = "Создать заявку";
        }

        // Конструктор для редактирования существующей заявки
        public NewRequestWindow(ApplicationDbContext dbContext, Ticket ticketToEdit) : this(dbContext)
        {
            if (ticketToEdit == null) throw new ArgumentNullException(nameof(ticketToEdit));

            _editingTicket = ticketToEdit;

            // Заполняем поля значениями заявки
            TitleTextBox.Text = _editingTicket.Title;
            DescriptionTextBox.Text = _editingTicket.Description;
            StatusComboBox.SelectedItem = _editingTicket.Status;
            PriorityComboBox.SelectedItem = _editingTicket.Priority;

            SaveButton.Content = "Обновить";
            Title = "Редактирование заявки";
        }

        private void InitializeComboBoxes()
        {
            StatusComboBox.ItemsSource = Enum.GetValues(typeof(TicketStatus)).Cast<TicketStatus>();
            PriorityComboBox.ItemsSource = Enum.GetValues(typeof(Priority)).Cast<Priority>();

            // По умолчанию выбираем первый статус и второй приоритет, если не редактируем
            if (_editingTicket == null)
            {
                StatusComboBox.SelectedIndex = 0;
                PriorityComboBox.SelectedIndex = 1;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Введите название заявки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TitleTextBox.Focus();
                return;
            }

            if (StatusComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите статус заявки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                StatusComboBox.Focus();
                return;
            }

            if (PriorityComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите приоритет заявки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                PriorityComboBox.Focus();
                return;
            }

            try
            {
                if (_editingTicket == null)
                {
                    // Создаем новую заявку
                    NewTicket = new Ticket
                    {
                        Number = $"T-{DateTime.Now:yyyyMMddHHmmss}",
                        Title = TitleTextBox.Text.Trim(),
                        Description = DescriptionTextBox.Text.Trim(),
                        Status = (TicketStatus)StatusComboBox.SelectedItem,
                        Priority = (Priority)PriorityComboBox.SelectedItem,
                        CreatedAt = DateTime.Now
                    };

                    _dbContext.Tickets.Add(NewTicket);
                }
                else
                {
                    // Обновляем существующую заявку
                    _editingTicket.Title = TitleTextBox.Text.Trim();
                    _editingTicket.Description = DescriptionTextBox.Text.Trim();
                    _editingTicket.Status = (TicketStatus)StatusComboBox.SelectedItem;
                    _editingTicket.Priority = (Priority)PriorityComboBox.SelectedItem;

                    _dbContext.Tickets.Update(_editingTicket);
                }

                await _dbContext.SaveChangesAsync();

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (ex.InnerException != null)
                    message += "\n\n" + ex.InnerException.Message;

                MessageBox.Show($"Ошибка при сохранении заявки:\n{message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
