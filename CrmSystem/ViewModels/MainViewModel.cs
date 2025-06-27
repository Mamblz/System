using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using CrmSystem.Data;
using CrmSystem.Models;
using Microsoft.EntityFrameworkCore;
using Timer = System.Timers.Timer;

public class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private readonly Func<ApplicationDbContext> _dbContextFactory;
    private readonly Timer _timer;
    private readonly IDispatcher _dispatcher;

    private int _activeClientsCount;
    private int _openProjectsCount;
    private decimal _monthlyRevenue;
    private string _serverTime;

    private string _dealSearchText = string.Empty;

    public int ActiveClientsCount
    {
        get => _activeClientsCount;
        private set => SetProperty(ref _activeClientsCount, value);
    }

    public int OpenProjectsCount
    {
        get => _openProjectsCount;
        private set => SetProperty(ref _openProjectsCount, value);
    }

    public decimal MonthlyRevenue
    {
        get => _monthlyRevenue;
        private set => SetProperty(ref _monthlyRevenue, value);
    }

    public string ServerTime
    {
        get => _serverTime;
        private set => SetProperty(ref _serverTime, value);
    }

    public string DealSearchText
    {
        get => _dealSearchText;
        set
        {
            if (SetProperty(ref _dealSearchText, value))
            {
                OnPropertyChanged(nameof(FilteredProjects));
            }
        }
    }

    public ObservableCollection<DealViewModel> Projects { get; }

    public IEnumerable<DealViewModel> FilteredProjects
    {
        get
        {
            if (string.IsNullOrWhiteSpace(DealSearchText))
                return Projects;

            var lowerSearch = DealSearchText.ToLowerInvariant();

            return Projects.Where(d => d.Name != null && d.Name.ToLowerInvariant().Contains(lowerSearch));
        }
    }

    public ObservableCollection<ClientViewModel> RecentClients { get; }
    public ObservableCollection<TodoItemViewModel> TodoItems { get; }
    public ObservableCollection<TaskStatisticViewModel> TaskStatistics { get; }

    public MainViewModel(Func<ApplicationDbContext> dbContextFactory, IDispatcher dispatcher)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

        Projects = new ObservableCollection<DealViewModel>();
        RecentClients = new ObservableCollection<ClientViewModel>();
        TodoItems = new ObservableCollection<TodoItemViewModel>();
        TaskStatistics = new ObservableCollection<TaskStatisticViewModel>();

        _timer = new Timer(1000);
        _timer.Elapsed += Timer_Elapsed;
        _timer.Start();

        Task.Run(() => LoadDataFromDatabaseAsync());
    }

    private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        _dispatcher.Invoke(() =>
        {
            ServerTime = DateTime.Now.ToString("HH:mm:ss");
        });
    }

    public async void Refresh()
    {
        await LoadDataFromDatabaseAsync();
    }

    public async Task LoadDataFromDatabaseAsync()
    {
        using var dbContext = _dbContextFactory();

        var recentClients = await dbContext.Clients
            .OrderByDescending(c => c.Id)
            .Take(1)
            .ToListAsync();

        var recentDeals = await dbContext.Deals
            .OrderByDescending(d => d.Id)
            .Take(50)
            .ToListAsync();

        var activeClientsCount = await dbContext.Clients.CountAsync();
        var openProjectsCount = await dbContext.Deals.CountAsync();

        var now = DateTime.Now;
        var monthlyRevenueDouble = await dbContext.Deals
            .Where(d => d.CreatedAt.Month == now.Month && d.CreatedAt.Year == now.Year)
            .SumAsync(d => (double?)d.Amount) ?? 0;

        var monthlyRevenue = (decimal)monthlyRevenueDouble;

        _dispatcher.Invoke(() =>
        {
            Projects.Clear();
            foreach (var deal in recentDeals)
            {
                Projects.Add(new DealViewModel
                {
                    Name = deal.Name,
                    Amount = deal.Amount,
                    Stage = deal.Stage,
                    Resources = deal.Resources,
                    Duration = deal.Duration
                });
            }

            RecentClients.Clear();
            foreach (var client in recentClients)
            {
                RecentClients.Add(new ClientViewModel
                {
                    Id = client.Id,
                    Name = client.Name,
                    Contact = client.Email ?? client.Phone ?? "-"
                });
            }

            ActiveClientsCount = activeClientsCount;
            OpenProjectsCount = openProjectsCount;
            MonthlyRevenue = monthlyRevenue;

            TodoItems.Clear();
            TodoItems.Add(new TodoItemViewModel { Task = "Позвонить клиенту", IsDone = false });
            TodoItems.Add(new TodoItemViewModel { Task = "Отправить коммерческое предложение", IsDone = true });

            TaskStatistics.Clear();
            TaskStatistics.Add(new TaskStatisticViewModel { Name = "Продажи", Progress = 70 });
            TaskStatistics.Add(new TaskStatisticViewModel { Name = "Встречи", Progress = 45 });
            TaskStatistics.Add(new TaskStatisticViewModel { Name = "Email-рассылки", Progress = 90 });

            OnPropertyChanged(nameof(FilteredProjects));
        });
    }

    public void AddClient(ClientViewModel client)
    {
        if (client == null) return;

        if (client.Id == 0)
        {
            client.Id = GenerateTemporaryClientId();
        }

        RecentClients.Insert(0, client);
        ActiveClientsCount++;
    }

    public void AddProject(DealViewModel project)
    {
        if (project == null) return;

        Projects.Add(project);
        OpenProjectsCount++;

        OnPropertyChanged(nameof(FilteredProjects));
    }

    private int _tempClientIdCounter = 1000;
    private int GenerateTemporaryClientId() => _tempClientIdCounter++;

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        return false;
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class DealViewModel
{
    public string Name { get; set; }
    public decimal Amount { get; set; }
    public DealStage Stage { get; set; }
    public string? Resources { get; set; }
    public string? Duration { get; set; }

    public string StageString => Stage.ToString();
}

public class ClientViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Contact { get; set; }
}

public class TodoItemViewModel
{
    public string Task { get; set; }
    public bool IsDone { get; set; }
}

public class TaskStatisticViewModel
{
    public string Name { get; set; }
    public double Progress { get; set; }
}

public interface IDispatcher
{
    void Invoke(Action action);
}

public class DispatcherWrapper : IDispatcher
{
    private readonly Dispatcher _dispatcher;

    public DispatcherWrapper(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    public void Invoke(Action action)
    {
        _dispatcher.Invoke(action);
    }
}
