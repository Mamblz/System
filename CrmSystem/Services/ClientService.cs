using CrmSystem.Data;
using CrmSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CrmSystem.Services
{
    public class ClientService
    {
        private readonly ApplicationDbContext _context;

        public ClientService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Добавление нового клиента
        public void AddClient(Client client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            client.CreatedAt = DateTime.UtcNow;
            _context.Clients.Add(client);
            _context.SaveChanges();
        }

        // Получение клиента по ID
        public Client GetClient(int id)
        {
            return _context.Clients
                .Include(c => c.Interactions) // Подгружаем историю взаимодействий
                .FirstOrDefault(c => c.Id == id);
        }

        // Получение всех клиентов
        public List<Client> GetAllClients()
        {
            return _context.Clients
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
        }

        // Обновление данных клиента
        public void UpdateClient(Client updatedClient)
        {
            var existingClient = _context.Clients.Find(updatedClient.Id);
            if (existingClient == null)
                throw new ArgumentException("Client not found");

            // Обновляем поля
            existingClient.Name = updatedClient.Name;
            existingClient.Email = updatedClient.Email;
            existingClient.Phone = updatedClient.Phone;
            existingClient.Address = updatedClient.Address;
            existingClient.Company = updatedClient.Company;

            _context.SaveChanges();
        }

        // Удаление клиента
        public void DeleteClient(int id)
        {
            var client = _context.Clients.Find(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
                _context.SaveChanges();
            }
        }

        // Добавление взаимодействия с клиентом
        public void AddInteraction(int clientId, Interaction interaction)
        {
            var client = _context.Clients.Find(clientId);
            if (client == null)
                throw new ArgumentException("Client not found");

            interaction.Date = DateTime.UtcNow;
            interaction.ClientId = clientId;
            _context.Interactions.Add(interaction);
            _context.SaveChanges();
        }

        // Получение истории взаимодействий клиента
        public List<Interaction> GetClientInteractions(int clientId)
        {
            return _context.Interactions
                .Where(i => i.ClientId == clientId)
                .OrderByDescending(i => i.Date)
                .ToList();
        }

        // Поиск клиентов по имени или компании
        public List<Client> SearchClients(string searchTerm)
        {
            return _context.Clients
                .Where(c => c.Name.Contains(searchTerm) ||
                            c.Company.Contains(searchTerm))
                .ToList();
        }
    }
}