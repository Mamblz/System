using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CrmSystem.Data;
using CrmSystem.Models;
using Moq;
using Xunit;

namespace CrmSystem.Tests
{
    public class MainViewModelTests
    {
        private Mock<ApplicationDbContext> _mockDbContext;
        private Mock<IDispatcher> _mockDispatcher;

        public MainViewModelTests()
        {
            _mockDbContext = new Mock<ApplicationDbContext>();
            _mockDispatcher = new Mock<IDispatcher>();
        }
        private MainViewModel CreateViewModel()
        {
            return new MainViewModel(_mockDbContext.Object, _mockDispatcher.Object);
        }

        [Fact]
        public void DealSearchText_Setter_InvokesPropertyChangedForFilteredProjects()
        {
            var vm = CreateViewModel();

            bool filteredProjectsChanged = false;
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(vm.FilteredProjects))
                    filteredProjectsChanged = true;
            };

            vm.DealSearchText = "test";

            Assert.True(filteredProjectsChanged);
        }

        [Fact]
        public void FilteredProjects_NoSearchText_ReturnsAllProjects()
        {
            var vm = CreateViewModel();

            var proj1 = new DealViewModel { Name = "Test1" };
            var proj2 = new DealViewModel { Name = "Other" };
            vm.Projects.Add(proj1);
            vm.Projects.Add(proj2);

            vm.DealSearchText = "";

            var filtered = vm.FilteredProjects.ToList();

            Assert.Contains(proj1, filtered);
            Assert.Contains(proj2, filtered);
            Assert.Equal(2, filtered.Count);
        }

        [Fact]
        public void FilteredProjects_WithSearchText_ReturnsMatchingProjectsOnly()
        {
            var vm = CreateViewModel();

            var proj1 = new DealViewModel { Name = "Alpha" };
            var proj2 = new DealViewModel { Name = "Beta" };
            vm.Projects.Add(proj1);
            vm.Projects.Add(proj2);

            vm.DealSearchText = "alp";

            var filtered = vm.FilteredProjects.ToList();

            Assert.Single(filtered);
            Assert.Contains(proj1, filtered);
        }
    }
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _dealSearchText;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<DealViewModel> Projects { get; } = new();

        public string DealSearchText
        {
            get => _dealSearchText;
            set
            {
                if (_dealSearchText != value)
                {
                    _dealSearchText = value;
                    OnPropertyChanged(nameof(DealSearchText));
                    OnPropertyChanged(nameof(FilteredProjects));
                }
            }
        }

        public IEnumerable<DealViewModel> FilteredProjects =>
            string.IsNullOrWhiteSpace(DealSearchText)
                ? Projects
                : Projects.Where(p => p.Name?.IndexOf(DealSearchText, StringComparison.OrdinalIgnoreCase) >= 0);
        public MainViewModel(ApplicationDbContext dbContext, IDispatcher dispatcher)
        {
        }

        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    public class DealViewModel
    {
        public string Name { get; set; }
    }

    public interface IDispatcher
    {
        void Invoke(Action action);
    }
}
