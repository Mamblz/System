using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CrmSystem.Data;
using CrmSystem.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CrmSystem.Tests
{
    public class MainViewModelTests
    {
        private readonly Mock<Func<ApplicationDbContext>> _mockDbFactory;
        private readonly Mock<ApplicationDbContext> _mockDbContext;
        private readonly Mock<DbSet<Client>> _mockClientsDbSet;
        private readonly Mock<DbSet<Deal>> _mockDealsDbSet;
        private readonly Mock<IDispatcher> _mockDispatcher;

        public MainViewModelTests()
        {
            _mockDbFactory = new Mock<Func<ApplicationDbContext>>();
            _mockDbContext = new Mock<ApplicationDbContext>();
            _mockClientsDbSet = new Mock<DbSet<Client>>();
            _mockDealsDbSet = new Mock<DbSet<Deal>>();
            _mockDispatcher = new Mock<IDispatcher>();

            _mockDbFactory.Setup(f => f()).Returns(_mockDbContext.Object);

            _mockDbContext.Setup(c => c.Clients).Returns(_mockClientsDbSet.Object);
            _mockDbContext.Setup(c => c.Deals).Returns(_mockDealsDbSet.Object);
            _mockDispatcher.Setup(d => d.Invoke(It.IsAny<Action>()))
                .Callback<Action>(a => a());
        }

        private MainViewModel CreateViewModel()
        {
            return new MainViewModel(_mockDbFactory.Object, _mockDispatcher.Object);
        }


        [Fact]
        public void Timer_Elapsed_UpdatesServerTimeProperty()
        {
            var vm = CreateViewModel();

            string initialTime = vm.ServerTime;
            vm.GetType().GetMethod("Timer_Elapsed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(vm, new object[] { null, null });

            string updatedTime = vm.ServerTime;

            Assert.NotNull(updatedTime);
            Assert.NotEqual(initialTime, updatedTime);
        }

        [Fact]
        public async Task AddClient_NewClient_AddsToRecentClientsAndIncrementsCount()
        {
            var vm = CreateViewModel();

            int initialCount = vm.ActiveClientsCount;
            var client = new ClientViewModel { Id = 0, Name = "Test Client", Contact = "test@test.com" };

            vm.AddClient(client);

            Assert.Contains(client, vm.RecentClients);
            Assert.Equal(initialCount + 1, vm.ActiveClientsCount);
            Assert.True(client.Id >= 1000);
        }

        [Fact]
        public void AddClient_NullClient_DoesNothing()
        {
            var vm = CreateViewModel();

            int initialCount = vm.ActiveClientsCount;

            vm.AddClient(null);

            Assert.Equal(initialCount, vm.ActiveClientsCount);
            Assert.Empty(vm.RecentClients);
        }

        [Fact]
        public void AddProject_NewProject_AddsToProjectsAndIncrementsCountAndRaisesPropertyChanged()
        {
            var vm = CreateViewModel();

            var project = new DealViewModel { Name = "Project 1", Amount = 123.45m };
            int initialCount = vm.OpenProjectsCount;

            bool propertyChangedRaised = false;
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(vm.FilteredProjects))
                    propertyChangedRaised = true;
            };

            vm.AddProject(project);

            Assert.Contains(project, vm.Projects);
            Assert.Equal(initialCount + 1, vm.OpenProjectsCount);
            Assert.True(propertyChangedRaised);
        }

        [Fact]
        public void AddProject_NullProject_DoesNothing()
        {
            var vm = CreateViewModel();

            int initialCount = vm.OpenProjectsCount;
            vm.AddProject(null);

            Assert.Equal(initialCount, vm.OpenProjectsCount);
            Assert.Empty(vm.Projects);
        }
    }
}
