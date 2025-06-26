using CrmSystem.ViewModels;
using Moq;
using Xunit;
using System;
using CrmSystem.Services;

namespace CrmSystem.Tests
{
    public class RegisterViewModelTests
    {
        private RegisterViewModel viewModel;

        public RegisterViewModelTests()
        {
            viewModel = new RegisterViewModel();
        }

        [Fact]
        public void Register_ShouldInvokeError_WhenFieldsAreEmpty()
        {
            string error = null;
            viewModel.OnError += msg => error = msg;

            viewModel.Register();

            Assert.Equal("Заполните все обязательные поля.", error);
        }

        [Fact]
        public void Register_ShouldInvokeError_WhenEmailInvalid()
        {
            viewModel.Username = "test";
            viewModel.Email = "invalid_email";
            viewModel.Password = "Password1";
            viewModel.ConfirmPassword = "Password1";

            string error = null;
            viewModel.OnError += msg => error = msg;

            viewModel.Register();

            Assert.Equal("Введите корректный email.", error);
        }

        [Fact]
        public void Register_ShouldInvokeError_WhenPasswordsDoNotMatch()
        {
            viewModel.Username = "test";
            viewModel.Email = "test@example.com";
            viewModel.Password = "Password1";
            viewModel.ConfirmPassword = "Password2";

            string error = null;
            viewModel.OnError += msg => error = msg;

            viewModel.Register();

            Assert.Equal("Пароли не совпадают.", error);
        }

        [Theory]
        [InlineData("short")]
        [InlineData("password")]
        [InlineData("12345678")]
        public void Register_ShouldInvokeError_WhenPasswordWeak(string password)
        {
            viewModel.Username = "test";
            viewModel.Email = "test@example.com";
            viewModel.Password = password;
            viewModel.ConfirmPassword = password;

            string error = null;
            viewModel.OnError += msg => error = msg;

            viewModel.Register();

            Assert.Equal("Пароль должен быть не менее 8 символов, содержать буквы и цифры.", error);
        }

        [Fact]
        public void Register_ShouldInvokeError_WhenAuthServiceFails()
        {
            var mockAuth = new Mock<IAuthService>();

            string expectedError = "Ошибка регистрации";
            string outError;
            mockAuth.Setup(a => a.Register(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                out expectedError
            )).Returns(false);

            var viewModel = new RegisterViewModel(mockAuth.Object);

            viewModel.Username = "test";
            viewModel.Email = "test@example.com";
            viewModel.Password = "Password1";
            viewModel.ConfirmPassword = "Password1";

            string actualError = null;
            viewModel.OnError += msg => actualError = msg;
            viewModel.Register();
            Assert.Equal("Ошибка регистрации", actualError);
        }


        [Fact]
        public void Register_ShouldInvokeSuccess_WhenRegistrationValid()
        {
            var mockAuth = new Mock<IAuthService>();

            string outError = null;
            mockAuth.Setup(a => a.Register(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                out outError))
                .Returns(true);

            var viewModel = new RegisterViewModel(mockAuth.Object);

            viewModel.Username = "test";
            viewModel.Email = "test@example.com";
            viewModel.Password = "Password1";
            viewModel.ConfirmPassword = "Password1";

            bool success = false;
            viewModel.OnSuccess += () => success = true;

            viewModel.Register();

            Assert.True(success);
        }


        [Fact]
        public void SetProperty_ShouldRaisePropertyChanged()
        {
            string changedProp = "";
            viewModel.PropertyChanged += (s, e) => changedProp = e.PropertyName;

            viewModel.Username = "newUser";

            Assert.Equal(nameof(viewModel.Username), changedProp);
        }

        [Theory]
        [InlineData("user@example.com", true)]
        [InlineData("bademail", false)]
        [InlineData("test@.com", false)]
        [InlineData("", false)]
        public void IsValidEmail_Test(string email, bool expected)
        {
            var method = typeof(RegisterViewModel).GetMethod("IsValidEmail", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            bool result = (bool)method.Invoke(viewModel, new object[] { email });

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("Password123", true)]
        [InlineData("pass", false)]
        [InlineData("12345678", false)]
        [InlineData("abcdefgh", false)]
        public void IsPasswordStrong_Test(string password, bool expected)
        {
            var method = typeof(RegisterViewModel).GetMethod("IsPasswordStrong", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            bool result = (bool)method.Invoke(viewModel, new object[] { password });

            Assert.Equal(expected, result);
        }
    }
}
