using CrmSystem.Data;
using CrmSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CrmSystem.Services
{
    public class DealService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DealService> _logger;

        public DealService(ApplicationDbContext context, ILogger<DealService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Deal CreateDeal(Deal deal)
        {
            if (deal == null)
                throw new ArgumentNullException(nameof(deal));

            if (deal.ClientId <= 0)
                throw new ArgumentException("Не указан идентификатор клиента", nameof(deal.ClientId));

            deal.Stage = DealStage.Лид; // Начальный этап сделки
            deal.CreatedAt = DateTime.UtcNow;

            _context.Deals.Add(deal);
            _context.SaveChanges();

            _logger.LogInformation($"Создана новая сделка ID: {deal.Id} для клиента ID: {deal.ClientId}");
            return deal;
        }

        public Deal GetDeal(int id)
        {
            return _context.Deals
                .Include(d => d.Client)
                .Include(d => d.Owner)
                .FirstOrDefault(d => d.Id == id);
        }

        public List<Deal> GetAllDeals()
        {
            return _context.Deals
                .Include(d => d.Client)
                .OrderByDescending(d => d.CreatedAt)
                .ToList();
        }

        public List<Deal> GetDealsByStage(DealStage stage)
        {
            return _context.Deals
                .Where(d => d.Stage == stage)
                .Include(d => d.Client)
                .OrderBy(d => d.ExpectedCloseDate)
                .ToList();
        }

        public List<Deal> GetDealsByClientId(int clientId)
        {
            return _context.Deals
                .Where(d => d.ClientId == clientId)
                .Include(d => d.Client)
                .Include(d => d.Owner)
                .OrderByDescending(d => d.CreatedAt)
                .ToList();
        }

        public void MoveDealToNextStage(int dealId)
        {
            var deal = _context.Deals.Find(dealId);
            if (deal == null)
                throw new ArgumentException($"Сделка с ID {dealId} не найдена");

            if (deal.Stage == DealStage.Завершено || deal.Stage == DealStage.Отменено)
                throw new InvalidOperationException("Закрытые сделки нельзя переводить на следующий этап");

            deal.Stage = deal.Stage switch
            {
                DealStage.Лид => DealStage.Предложение,
                DealStage.Предложение => DealStage.ВРаботе,
                DealStage.ВРаботе => DealStage.Завершено,
                _ => deal.Stage
            };

            _context.SaveChanges();
            _logger.LogInformation($"Сделка ID: {dealId} переведена на этап: {deal.Stage}");
        }

        public void CloseDealAsWon(int dealId)
        {
            var deal = _context.Deals.Find(dealId);
            if (deal == null)
                throw new ArgumentException($"Сделка с ID {dealId} не найдена");

            deal.Stage = DealStage.Завершено;
            deal.ClosedAt = DateTime.UtcNow;
            _context.SaveChanges();

            _logger.LogInformation($"Сделка ID: {dealId} закрыта как ВЫИГРАННАЯ");
        }

        public void CloseDealAsLost(int dealId, string reason)
        {
            var deal = _context.Deals.Find(dealId);
            if (deal == null)
                throw new ArgumentException($"Сделка с ID {dealId} не найдена");

            deal.Stage = DealStage.Отменено;
            deal.ClosedAt = DateTime.UtcNow;
            deal.LostReason = reason;
            _context.SaveChanges();

            _logger.LogInformation($"Сделка ID: {dealId} закрыта как ПРОИГРАННАЯ. Причина: {reason}");
        }

        public void ChangeDealOwner(int dealId, int newOwnerId)
        {
            var deal = _context.Deals.Find(dealId);
            if (deal == null)
                throw new ArgumentException($"Сделка с ID {dealId} не найдена");

            var newOwner = _context.Users.Find(newOwnerId);
            if (newOwner == null)
                throw new ArgumentException($"Пользователь с ID {newOwnerId} не найден");

            deal.OwnerId = newOwnerId;
            _context.SaveChanges();

            _logger.LogInformation($"Сделка ID: {dealId} назначена пользователю ID: {newOwnerId}");
        }

        public List<Deal> GetDealsByOwner(int ownerId)
        {
            return _context.Deals
                .Where(d => d.OwnerId == ownerId)
                .Include(d => d.Client)
                .OrderByDescending(d => d.ExpectedCloseDate)
                .ToList();
        }

        public void UpdateDealAmount(int dealId, decimal newAmount)
        {
            var deal = _context.Deals.Find(dealId);
            if (deal == null)
                throw new ArgumentException($"Сделка с ID {dealId} не найдена");

            deal.Amount = newAmount;
            _context.SaveChanges();

            _logger.LogInformation($"Сумма сделки ID: {dealId} обновлена до: {newAmount}");
        }

        public DealStatistics GetDealStatistics()
        {
            return new DealStatistics
            {
                TotalDeals = _context.Deals.Count(),
                TotalWon = _context.Deals.Count(d => d.Stage == DealStage.Завершено),
                TotalLost = _context.Deals.Count(d => d.Stage == DealStage.Отменено),
                TotalInProgress = _context.Deals.Count(d =>
                    d.Stage != DealStage.Завершено &&
                    d.Stage != DealStage.Отменено),
                TotalAmount = _context.Deals
                    .Where(d => d.Stage == DealStage.Завершено)
                    .Sum(d => d.Amount)
            };
        }
    }

    public class DealStatistics
    {
        public int TotalDeals { get; set; }
        public int TotalWon { get; set; }
        public int TotalLost { get; set; }
        public int TotalInProgress { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
