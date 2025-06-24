using CrmSystem.Data;
using CrmSystem.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace CrmSystem.ViewModels
{
    public class TasksPageViewModel : INotifyPropertyChanged
    {
        private readonly ApplicationDbContext _dbContext;

        public ObservableCollection<TasksItems> TaskList { get; set; } = new ObservableCollection<TasksItems>();

        private TasksItems _selectedTask;
        public TasksItems SelectedTask
        {
            get => _selectedTask;
            set
            {
                if (_selectedTask != value)
                {
                    _selectedTask = value;
                    OnPropertyChanged(nameof(SelectedTask));
                }
            }
        }

        private string _newTaskTitle;
        public string NewTaskTitle
        {
            get => _newTaskTitle;
            set
            {
                if (_newTaskTitle != value)
                {
                    _newTaskTitle = value;
                    OnPropertyChanged(nameof(NewTaskTitle));
                }
            }
        }

        private string _newTaskDescription;
        public string NewTaskDescription
        {
            get => _newTaskDescription;
            set
            {
                if (_newTaskDescription != value)
                {
                    _newTaskDescription = value;
                    OnPropertyChanged(nameof(NewTaskDescription));
                }
            }
        }

        private DateTime? _newTaskDate;
        public DateTime? NewTaskDate
        {
            get => _newTaskDate;
            set
            {
                if (_newTaskDate != value)
                {
                    _newTaskDate = value;
                    OnPropertyChanged(nameof(NewTaskDate));
                }
            }
        }

        public TasksPageViewModel(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            LoadTasksFromDatabase();
        }

        public void LoadTasksFromDatabase()
        {
            TaskList.Clear();

            var tasks = _dbContext.TaskItems
                                  .OrderBy(t => t.DueDate)
                                  .ToList();

            foreach (var task in tasks)
            {
                TaskList.Add(task);
            }

            SelectedTask = null;
            OnPropertyChanged(nameof(TaskList));
        }

        public void DeleteTask(int taskId)
        {
            var entityToDelete = _dbContext.TaskItems.FirstOrDefault(t => t.Id == taskId);
            if (entityToDelete == null)
                return;

            _dbContext.TaskItems.Remove(entityToDelete);
            _dbContext.SaveChanges();

            var taskItemToRemove = TaskList.FirstOrDefault(t => t.Id == taskId);
            if (taskItemToRemove != null)
                TaskList.Remove(taskItemToRemove);

            if (SelectedTask != null && SelectedTask.Id == taskId)
            {
                SelectedTask = null;
            }
        }

        public void AddNewTask()
        {
            if (string.IsNullOrWhiteSpace(NewTaskTitle))
                return;

            var newTask = new TasksItems
            {
                Title = NewTaskTitle.Trim(),
                Description = NewTaskDescription?.Trim(),
                DueDate = NewTaskDate ?? DateTime.Now
            };

            _dbContext.TaskItems.Add(newTask);
            _dbContext.SaveChanges();

            TaskList.Add(newTask);
            SelectedTask = null;
            NewTaskTitle = string.Empty;
            NewTaskDescription = string.Empty;
            NewTaskDate = null;
        }

        public void RefreshTasks()
        {
            LoadTasksFromDatabase();
        }

        private DateTime _selectedCalendarDate = DateTime.Today;
        public DateTime SelectedCalendarDate
        {
            get => _selectedCalendarDate;
            set
            {
                if (_selectedCalendarDate != value)
                {
                    _selectedCalendarDate = value;
                    OnPropertyChanged(nameof(SelectedCalendarDate));
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
