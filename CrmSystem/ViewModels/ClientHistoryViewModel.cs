using CrmSystem.Data;
using CrmSystem.Models;
using CrmSystem.Services;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace CrmSystem.ViewModels
{
    public class ClientHistoryViewModel : BaseViewModel
    {
        private readonly DealService _dealService;

        public Client Client { get; }
        public ObservableCollection<Deal> Deals { get; }

        public ClientHistoryViewModel(Client client)
        {
            Client = client;

            var context = new ApplicationDbContext();

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<DealService>();

            _dealService = new DealService(context, logger);

            var clientDeals = _dealService.GetDealsByClientId(Client.Id);
            Deals = new ObservableCollection<Deal>(clientDeals);
        }
    }
}
