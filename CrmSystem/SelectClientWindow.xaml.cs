// SelectClientWindow.xaml.cs
using System.Windows;
using CrmSystem.Models;
using CrmSystem.Data;
using System.Collections.Generic;
using System.Linq;

namespace CrmSystem.Views
{
    public partial class SelectClientWindow : Window
    {
        private readonly ApplicationDbContext _dbContext;

        public Client SelectedClient { get; private set; }

        public SelectClientWindow(ApplicationDbContext dbContext)
        {
            InitializeComponent();
            _dbContext = dbContext;

            LoadClients();
        }

        private void LoadClients()
        {
            List<Client> clients = _dbContext.Clients.OrderBy(c => c.Name).ToList();
            ClientsListBox.ItemsSource = clients;
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsListBox.SelectedItem is Client client)
            {
                SelectedClient = client;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите клиента.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
