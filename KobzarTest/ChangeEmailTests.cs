using CrmSystem.ViewModels;

namespace CrmSystem.Tests
{
    public class ChangeEmailViewModelTests
    {
        [Fact]
        public void SaveCommand_Should_ShowError_When_Email_IsEmpty()
        {
            var messages = new List<(string msg, string title)>();
            var vm = new ChangeEmailViewModel("old@example.com")
            {
                ShowMessage = (msg, title) => messages.Add((msg, title))
            };

            vm.NewEmail = "";
            vm.SaveCommand.Execute(null);

            Assert.Single(messages);
            Assert.Equal("Введите корректный Email.", messages[0].msg);
            Assert.Equal("Ошибка", messages[0].title);
            Assert.Null(vm.DialogResult);
        }

        [Fact]
        public void SaveCommand_Should_ShowError_When_Email_IsInvalid()
        {
            var messages = new List<(string msg, string title)>();
            var vm = new ChangeEmailViewModel("old@example.com")
            {
                ShowMessage = (msg, title) => messages.Add((msg, title))
            };

            vm.NewEmail = "invalidemail";
            vm.SaveCommand.Execute(null);

            Assert.Single(messages);
            Assert.Equal("Введите корректный Email.", messages[0].msg);
            Assert.Equal("Ошибка", messages[0].title);
            Assert.Null(vm.DialogResult);
        }

        [Fact]
        public void SaveCommand_Should_SetDialogResult_True_And_Close_OnSuccess()
        {
            var messages = new List<(string msg, string title)>();
            bool wasClosed = false;

            var vm = new ChangeEmailViewModel("old@example.com")
            {
                ShowMessage = (msg, title) => messages.Add((msg, title))
            };
            vm.RequestClose += () => wasClosed = true;

            vm.NewEmail = "new@example.com";
            vm.SaveCommand.Execute(null);

            Assert.Single(messages);
            Assert.Equal("Email успешно изменён.", messages[0].msg);
            Assert.Equal("Успех", messages[0].title);
            Assert.True(vm.DialogResult);
            Assert.True(wasClosed);
        }

        [Fact]
        public void CancelCommand_Should_SetDialogResult_False_And_Close()
        {
            bool wasClosed = false;
            var vm = new ChangeEmailViewModel("old@example.com");
            vm.RequestClose += () => wasClosed = true;

            vm.CancelCommand.Execute(null);

            Assert.False(vm.DialogResult);
            Assert.True(wasClosed);
        }

        [Fact]
        public void IsPlaceholderVisible_Should_Be_True_When_NewEmail_IsEmpty()
        {
            var vm = new ChangeEmailViewModel("old@example.com");
            vm.NewEmail = "";

            Assert.True(vm.IsPlaceholderVisible);
        }

        [Fact]
        public void IsPlaceholderVisible_Should_Be_False_When_NewEmail_IsNotEmpty()
        {
            var vm = new ChangeEmailViewModel("old@example.com");
            vm.NewEmail = "something@example.com";

            Assert.False(vm.IsPlaceholderVisible);
        }

        [Fact]
        public void NewEmail_Should_Raise_PropertyChanged_For_NewEmail_And_IsPlaceholderVisible()
        {
            var vm = new ChangeEmailViewModel("old@example.com");
            var changedProps = new List<string>();

            vm.PropertyChanged += (s, e) => changedProps.Add(e.PropertyName);
            vm.NewEmail = "abc@x.com";

            Assert.Contains("NewEmail", changedProps);
            Assert.Contains("IsPlaceholderVisible", changedProps);
        }
    }
}
