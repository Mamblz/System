// --- файл ViewModels/ClientEditViewModel.cs ---
using System;
using System.Windows.Input;
using CrmSystem.Commands;
using CrmSystem.Models;
using CrmSystem.Services;
using CrmSystem.Data;

namespace CrmSystem.ViewModels
{
    public class ClientEditViewModel : BaseViewModel
    {
        private readonly IClientService _clientService;

        public Client Client { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<bool> RequestClose;
        public ClientEditViewModel(Client client, IClientService clientService = null)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            _clientService = clientService ?? new ClientService(new ApplicationDbContext());

            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        private void Save()
        {
            if (Client.Id == 0)
                _clientService.AddClient(Client);
            else
                _clientService.UpdateClient(Client);

            RequestClose?.Invoke(true);
        }

        private void Cancel()
        {
            RequestClose?.Invoke(false);
        }
    }
}
