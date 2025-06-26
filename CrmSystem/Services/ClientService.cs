using CrmSystem.Data;
using CrmSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CrmSystem.Services
{
    public interface IClientService
    {
        void AddClient(Client client);
        Client GetClient(int id);
        List<Client> GetAllClients();
        void UpdateClient(Client updatedClient);
        void DeleteClient(int id);
        void AddInteraction(int clientId, Interaction interaction);
        List<Interaction> GetClientInteractions(int clientId);
        List<Client> SearchClients(string searchTerm);
    }

    public class ClientService : IClientService
    {
        private readonly ApplicationDbContext _context;

        public ClientService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void AddClient(Client client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            client.CreatedAt = DateTime.UtcNow;
            _context.Clients.Add(client);
            _context.SaveChanges();
        }

        public Client GetClient(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "Id должен быть больше 0");

            return _context.Clients
                .Include(c => c.Interactions)
                .FirstOrDefault(c => c.Id == id);
        }

        public List<Client> GetAllClients()
        {
            return _context.Clients
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
        }

        public void UpdateClient(Client updatedClient)
        {
            if (updatedClient == null)
                throw new ArgumentNullException(nameof(updatedClient));

            var existingClient = _context.Clients.Find(updatedClient.Id);
            if (existingClient == null)
                throw new ArgumentException("Client not found", nameof(updatedClient));

            existingClient.Name = updatedClient.Name;
            existingClient.Email = updatedClient.Email;
            existingClient.Phone = updatedClient.Phone;
            existingClient.Address = updatedClient.Address;
            existingClient.Company = updatedClient.Company;

            _context.SaveChanges();
        }

        public void DeleteClient(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "Id должен быть больше 0");

            var client = _context.Clients.Find(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
                _context.SaveChanges();
            }
        }

        public void AddInteraction(int clientId, Interaction interaction)
        {
            if (interaction == null)
                throw new ArgumentNullException(nameof(interaction));
            if (clientId <= 0)
                throw new ArgumentOutOfRangeException(nameof(clientId), "Id должен быть больше 0");

            var client = _context.Clients.Find(clientId);
            if (client == null)
                throw new ArgumentException("Client not found", nameof(clientId));

            interaction.Date = DateTime.UtcNow;
            interaction.ClientId = clientId;
            _context.Interactions.Add(interaction);
            _context.SaveChanges();
        }

        public List<Interaction> GetClientInteractions(int clientId)
        {
            if (clientId <= 0)
                throw new ArgumentOutOfRangeException(nameof(clientId), "Id должен быть больше 0");

            return _context.Interactions
                .Where(i => i.ClientId == clientId)
                .OrderByDescending(i => i.Date)
                .ToList();
        }

        public List<Client> SearchClients(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Client>();

            string term = searchTerm.Trim();

            return _context.Clients
                .Where(c => (!string.IsNullOrEmpty(c.Name) && c.Name.Contains(term)) ||
                            (!string.IsNullOrEmpty(c.Company) && c.Company.Contains(term)))
                .ToList();
        }
    }
}
