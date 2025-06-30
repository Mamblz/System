using System;
using System.Windows;
using CrmSystem.Models;
using CrmSystem.Services;
using CrmSystem.ViewModels;
using Moq;
using Xunit;


namespace CrmSystem.Tests
{
    public class ChangePasswordViewModelTests
    {
        [Fact]
        public void SaveCommand_Should_ShowError_When_AnyFieldIsEmpty()
        {
            var user = new User { Username = "testuser", PasswordHash = "oldhash" };
            var messageBoxMock = new Mock<IMessageBoxService>();
            var vm = new ChangePasswordViewModel(user, null, messageBoxMock.Object);

            vm.OldPassword = "";
            vm.NewPassword = "123";
            vm.ConfirmPassword = "123";

            vm.SaveCommand.Execute(null);

            messageBoxMock.Verify(m => m.Show("Все поля обязательны.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning), Times.Once);
        }

        [Fact]
        public void CancelCommand_Should_InvokeRequestClose()
        {
            var user = new User { Username = "testuser", PasswordHash = "oldhash" };
            var vm = new ChangePasswordViewModel(user);

            bool closed = false;
            vm.RequestClose += () => closed = true;

            vm.CancelCommand.Execute(null);

            Assert.True(closed);
        }

    }
}
