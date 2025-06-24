using System;

namespace CrmSystem.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TicketStatus Status { get; set; }
        public Priority Priority { get; set; }
        public int? ClientId { get; set; }
        public DateTime CreatedAt { get; set; }
    }



    public enum TicketStatus
    {
        Новый,
        ВПроцессе,
        ВОжидании,
        Завершён,
        Отменён
    }

    public enum Priority
    {
        Низкий,
        Средний,
        Высокий,
        Срочный
    }

}
