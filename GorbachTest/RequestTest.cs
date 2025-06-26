using CrmSystem.Data;
using CrmSystem.Models;
using CrmSystem.ViewModels;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CrmSystem.Tests
{
    public class RequestsPageViewModelTests
    {
        private ApplicationDbContext CreateMockContext(List<Ticket> tickets)
        {
            var data = tickets.AsQueryable();

            var mockSet = new Mock<DbSet<Ticket>>();
            mockSet.As<IQueryable<Ticket>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Ticket>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Ticket>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Ticket>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            mockSet.Setup(m => m.Find(It.IsAny<object[]>()))
                .Returns<object[]>(ids => tickets.FirstOrDefault(t => t.Id == (int)ids[0]));

            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Tickets).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockContext.Setup(c => c.Tickets.Remove(It.IsAny<Ticket>())).Callback<Ticket>(t => tickets.Remove(t));
            mockContext.Setup(c => c.Tickets.Add(It.IsAny<Ticket>())).Callback<Ticket>(t => tickets.Add(t));

            return mockContext.Object;
        }

        [Fact]
        public void Constructor_ShouldThrowOnNullDbContext()
        {
            Assert.Throws<ArgumentNullException>(() => new RequestsPageViewModel(null));
        }

        [Fact]
        public void LoadTicketsFromDb_ShouldLoadTickets()
        {
            var ticketList = new List<Ticket>
        {
            new() { Id = 1, Title = "T1", CreatedAt = DateTime.Now },
            new() { Id = 2, Title = "T2", CreatedAt = DateTime.Now.AddDays(-1) }
        };
            var context = CreateMockContext(ticketList);
            var vm = new RequestsPageViewModel(context);
            vm.LoadTicketsFromDb();
            Assert.Equal(2, vm.Tickets.Count);
            Assert.Equal(2, vm.FilteredTickets.Count);
        }

        [Fact]
        public void CreateTicket_ShouldAddTicketAndApplyFilter()
        {
            var tickets = new List<Ticket>();
            var context = CreateMockContext(tickets);
            var vm = new RequestsPageViewModel(context);
            vm.LoadTicketsFromDb();
            var newTicket = new Ticket { Id = 1, Title = "New", CreatedAt = DateTime.Now };
            vm.CreateTicket(newTicket);
            Assert.Contains(newTicket, vm.Tickets);
            Assert.Contains(newTicket, vm.FilteredTickets);
            Assert.Single(vm.Tickets);
        }

        [Fact]
        public void CreateTicket_ShouldThrowOnNullTicket()
        {
            var context = CreateMockContext(new List<Ticket>());
            var vm = new RequestsPageViewModel(context);

            Assert.Throws<ArgumentNullException>(() => vm.CreateTicket(null));
        }

        [Fact]
        public void DeleteTicket_ShouldReturnFalseIfNull()
        {
            var context = CreateMockContext(new List<Ticket>());
            var vm = new RequestsPageViewModel(context);

            var result = vm.DeleteTicket(null);

            Assert.False(result);
        }

        [Fact]
        public void DeleteTicket_ShouldReturnFalseIfTicketNotExists()
        {
            var tickets = new List<Ticket>();
            var context = CreateMockContext(tickets);
            var vm = new RequestsPageViewModel(context);

            var ticket = new Ticket { Id = 1 };

            var result = vm.DeleteTicket(ticket);

            Assert.False(result);
        }

        [Fact]
        public void DeleteTicket_ShouldRemoveTicketIfExists()
        {
            var ticket = new Ticket { Id = 1, Title = "Test" };
            var tickets = new List<Ticket> { ticket };
            var context = CreateMockContext(tickets);
            var vm = new RequestsPageViewModel(context);
            vm.LoadTicketsFromDb();
            var result = vm.DeleteTicket(ticket);
            Assert.True(result);
            Assert.DoesNotContain(ticket, vm.Tickets);
            Assert.DoesNotContain(ticket, vm.FilteredTickets);
        }

        [Fact]
        public void ApplyFilter_ShouldFilterBySearchText()
        {
            var ticket1 = new Ticket { Id = 1, Title = "Hello World", Description = "Desc", CreatedAt = DateTime.Now };
            var ticket2 = new Ticket { Id = 2, Title = "Test", Description = "Another", CreatedAt = DateTime.Now };
            var tickets = new List<Ticket> { ticket1, ticket2 };
            var context = CreateMockContext(tickets);
            var vm = new RequestsPageViewModel(context);
            vm.LoadTicketsFromDb();

            vm.SearchText = "hello";
            vm.ApplyFilter();

            Assert.Single(vm.FilteredTickets);
            Assert.Equal(ticket1, vm.FilteredTickets[0]);
        }

        [Fact]
        public void ApplyFilter_ShouldFilterByStatus()
        {
            var ticket1 = new Ticket { Id = 1, Status = TicketStatus.Новый, CreatedAt = DateTime.Now };
            var ticket2 = new Ticket { Id = 2, Status = TicketStatus.ВПроцессе, CreatedAt = DateTime.Now };
            var tickets = new List<Ticket> { ticket1, ticket2 };
            var context = CreateMockContext(tickets);
            var vm = new RequestsPageViewModel(context);
            vm.LoadTicketsFromDb();

            vm.SelectedStatus = TicketStatus.Новый;
            vm.ApplyFilter();

            Assert.Single(vm.FilteredTickets);
            Assert.Equal(ticket1, vm.FilteredTickets[0]);
        }

        [Fact]
        public void ApplyFilter_ShouldFilterByStatusAndSearchText()
        {
            var ticket1 = new Ticket { Id = 1, Title = "Hello", Status = TicketStatus.Новый, CreatedAt = DateTime.Now };
            var ticket2 = new Ticket { Id = 2, Title = "World", Status = TicketStatus.ВПроцессе, CreatedAt = DateTime.Now };
            var tickets = new List<Ticket> { ticket1, ticket2 };
            var context = CreateMockContext(tickets);
            var vm = new RequestsPageViewModel(context);
            vm.LoadTicketsFromDb();

            vm.SelectedStatus = TicketStatus.Новый;
            vm.SearchText = "hello";
            vm.ApplyFilter();

            Assert.Single(vm.FilteredTickets);
            Assert.Equal(ticket1, vm.FilteredTickets[0]);
        }
    }
}