using CrmSystem.Data;
using CrmSystem.ViewModels;
using CrmSystem.Models;
using Microsoft.EntityFrameworkCore;


namespace CrmSystem.Tests
{
    public class TasksPageViewModelTests
    {
        [Fact]
        public void LoadTasksFromDatabase_ShouldLoadAndSortTasks()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;
            using var context = new ApplicationDbContext(options);

            context.TaskItems.AddRange(
                new TasksItems { Title = "Task1", DueDate = DateTime.Today.AddDays(2), Description = "desc1" },
                new TasksItems { Title = "Task2", DueDate = DateTime.Today, Description = "desc2" }
            );
            context.SaveChanges();

            var vm = new TasksPageViewModel(context);
            vm.LoadTasksFromDatabase();
            Assert.Equal(2, vm.TaskList.Count);
            Assert.Equal("Task2", vm.TaskList[0].Title);
        }

        [Fact]
        public void AddNewTask_ShouldAdd_WhenValidTitle()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                            .UseInMemoryDatabase("AddTaskTest")
                            .Options;
            using var context = new ApplicationDbContext(options);

            var vm = new TasksPageViewModel(context)
            {
                NewTaskTitle = "Test Task",
                NewTaskDescription = "Description",
                NewTaskDate = DateTime.Today
            };
            vm.AddNewTask();
            Assert.Single(vm.TaskList);
            Assert.Equal("Test Task", vm.TaskList[0].Title);
            Assert.Equal(string.Empty, vm.NewTaskTitle);
            Assert.Null(vm.NewTaskDate);
        }

        [Fact]
        public void AddNewTask_ShouldNotAdd_WhenTitleIsEmpty()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                            .UseInMemoryDatabase("EmptyTitleTest")
                            .Options;
            using var context = new ApplicationDbContext(options);
            var vm = new TasksPageViewModel(context)
            {
                NewTaskTitle = " "
            };
            vm.AddNewTask();
            Assert.Empty(vm.TaskList);
            Assert.Equal(0, context.TaskItems.Count());
        }

        [Fact]
        public void DeleteTask_ShouldRemove_WhenTaskExists()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;

            using var context = new ApplicationDbContext(options);
            var task = new TasksItems
            {
                Title = "To delete",
                Description = "This will be deleted",
                DueDate = DateTime.Now
            };

            context.TaskItems.Add(task);
            context.SaveChanges();

            var vm = new TasksPageViewModel(context);
            vm.LoadTasksFromDatabase();
            vm.DeleteTask(task.Id);
            Assert.Empty(vm.TaskList);
            Assert.Null(context.TaskItems.FirstOrDefault(t => t.Id == task.Id));
        }


        [Fact]
        public void DeleteTask_ShouldNotRemove_WhenTaskDoesNotExist()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                            .UseInMemoryDatabase("DeleteNonExistent")
                            .Options;
            using var context = new ApplicationDbContext(options);
            var vm = new TasksPageViewModel(context);
            vm.DeleteTask(999);
            Assert.Empty(vm.TaskList);
            Assert.Equal(0, context.TaskItems.Count());
        }
    }
}