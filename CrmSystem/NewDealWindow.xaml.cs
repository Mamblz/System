using System;
using System.Windows;
using System.Windows.Controls;
using CrmSystem.Models;

namespace CrmSystem.Views
{
    public partial class NewDealWindow : Window
    {
        public int OwnerId { get; }
        public int ClientId { get; }

        public Deal NewDeal { get; private set; }

        public NewDealWindow(int? ownerId, int? clientId)
        {
            InitializeComponent();

            if (ownerId == null) throw new ArgumentNullException(nameof(ownerId));
            if (clientId == null) throw new ArgumentNullException(nameof(clientId));

            OwnerId = ownerId.Value;
            ClientId = clientId.Value;

            StatusBox.SelectedIndex = 0;
        }

        public NewDealWindow(Deal deal) : this(deal.OwnerId, deal.ClientId)
        {
            if (deal == null) throw new ArgumentNullException(nameof(deal));
            NewDeal = deal;
            LoadDeal(deal);
        }

        private void LoadDeal(Deal deal)
        {
            NameBox.Text = deal.Name;
            BudgetBox.Text = deal.Amount.ToString("N2");
            ResourceUsageBox.Text = deal.Resources ?? "";
            TimelineBox.Text = deal.Duration ?? "";

            foreach (ComboBoxItem item in StatusBox.Items)
            {
                if ((string)item.Content == GetStatusString(deal.Stage))
                {
                    StatusBox.SelectedItem = item;
                    break;
                }
            }
        }

        private string GetStatusString(DealStage stage) => stage switch
        {
            DealStage.Лид => "Лид",
            DealStage.Предложение => "Предложение",
            DealStage.ВРаботе => "В работе",
            DealStage.Завершено => "Завершено",
            DealStage.Отменено => "Отменено",
            _ => "Неизвестно"
        };

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Введите название сделки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(BudgetBox.Text.Replace(" ", "").Replace("₽", "").Replace(",", "."), out decimal amount))
            {
                MessageBox.Show("Введите корректную сумму сделки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedStatus = (StatusBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

            DealStage stageEnum = selectedStatus switch
            {
                "Лид" => DealStage.Лид,
                "Предложение" => DealStage.Предложение,
                "В работе" => DealStage.ВРаботе,
                "Завершено" => DealStage.Завершено,
                "Отменено" => DealStage.Отменено,
                _ => DealStage.Лид
            };

            try
            {
                if (NewDeal == null)
                {
                    NewDeal = new Deal
                    {
                        CreatedAt = DateTime.Now,
                        ClientId = ClientId,
                        OwnerId = OwnerId
                    };
                }

                // обновление данных сделки
                NewDeal.Name = NameBox.Text.Trim();
                NewDeal.Amount = amount;
                NewDeal.Stage = stageEnum;
                NewDeal.Resources = ResourceUsageBox.Text.Trim();
                NewDeal.Duration = TimelineBox.Text.Trim();

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при создании сделки:\n{ex.Message}\n\n{ex.InnerException?.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
