using System;
using System.Collections.ObjectModel;
using System.Linq;
using CrmSystem.Data;
using CrmSystem.Models;

namespace CrmSystem.ViewModels
{
    public class RequestsPageViewModel
    {
        private readonly ApplicationDbContext _dbContext;
        public ObservableCollection<Ticket> Tickets { get; private set; }
        public ObservableCollection<Ticket> FilteredTickets { get; private set; }

        public string SearchText { get; set; } = "";
        public TicketStatus? SelectedStatus { get; set; } = null;
        public ApplicationDbContext DbContext => _dbContext;

        public RequestsPageViewModel(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            Tickets = new ObservableCollection<Ticket>();
            FilteredTickets = new ObservableCollection<Ticket>();
        }

        public void LoadTicketsFromDb()
        {
            Tickets.Clear();

            var ticketsFromDb = _dbContext.Tickets.ToList();
            foreach (var ticket in ticketsFromDb)
            {
                Tickets.Add(ticket);
            }
            ApplyFilter();
        }

        public void CreateTicket(Ticket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException(nameof(ticket));

            _dbContext.Tickets.Add(ticket);
            _dbContext.SaveChanges();

            Tickets.Add(ticket);
            ApplyFilter();
        }

        public bool DeleteTicket(Ticket ticket)
        {
            if (ticket == null) return false;
            var existing = _dbContext.Tickets.Find(ticket.Id);
            if (existing == null) return false;

            _dbContext.Tickets.Remove(existing);
            _dbContext.SaveChanges();

            Tickets.Remove(ticket);
            FilteredTickets.Remove(ticket);

            return true;
        }

        public void ApplyFilter()
        {
            var searchLower = SearchText?.ToLower() ?? "";

            var filtered = Tickets.Where(t =>
                (SelectedStatus == null || t.Status == SelectedStatus) &&
                (string.IsNullOrWhiteSpace(SearchText) ||
                 (t.Title?.ToLower().Contains(searchLower) == true ||
                  t.Description?.ToLower().Contains(searchLower) == true))
            )
            .OrderByDescending(t => t.CreatedAt)
            .ToList();

            FilteredTickets.Clear();
            foreach (var ticket in filtered)
            {
                FilteredTickets.Add(ticket);
            }
        }
    }
}
