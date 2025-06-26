using CrmSystem.Models;
using CrmSystem.Services;
using CrmSystem.ViewModels;
using Moq;

namespace CrmSystem.Tests
{
    public class LoginViewModelTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly LoginViewModel _viewModel;

        public LoginViewModelTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _viewModel = new LoginViewModel(_mockAuthService.Object);
        }

        [Fact]
        public void Constructor_NullAuthService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new LoginViewModel(null));
        }

        [Fact]
        public void LoginProperty_SetValue_RaisesPropertyChanged()
        {
            bool eventRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(LoginViewModel.Login))
                    eventRaised = true;
            };

            _viewModel.Login = "newlogin";

            Assert.True(eventRaised);
            Assert.Equal("newlogin", _viewModel.Login);
        }

        [Fact]
        public void PasswordProperty_SetValue_RaisesPropertyChanged()
        {
            bool eventRaised = false;
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(LoginViewModel.Password))
                    eventRaised = true;
            };

            _viewModel.Password = "newpass";

            Assert.True(eventRaised);
            Assert.Equal("newpass", _viewModel.Password);
        }

        [Theory]
        [InlineData(null, "Password1", false)]
        [InlineData("us", "Password1", false)]
        [InlineData("user", null, false)]
        [InlineData("user", "short", false)]
        [InlineData("user", "password", false)]
        [InlineData("user", "12345678", false)]
        [InlineData("user", "Password1", true)]
        public void IsInputValid_ValidatesCorrectly(string login, string password, bool expected)
        {
            _viewModel.Login = login;
            _viewModel.Password = password;

            bool result = _viewModel.IsInputValid(login, password);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ExecuteLogin_InvalidInput_DoesNotCallAuthService()
        {
            _viewModel.Login = "us";
            _viewModel.Password = "Password1";

            _viewModel.LoginCommand.Execute(null);

            _mockAuthService.Verify(a => a.Login(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ExecuteLogin_ValidInput_CallsAuthServiceLogin()
        {
            _viewModel.Login = "validUser";
            _viewModel.Password = "Password1";

            _mockAuthService.Setup(a => a.Login("validUser", "Password1"))
                .Returns(new User { Username = "validUser" });

            _viewModel.LoginCommand.Execute(null);

            _mockAuthService.Verify(a => a.Login("validUser", "Password1"), Times.Once);
        }

        [Fact]
        public void ExecuteLogin_ValidInput_UserReturned_InvokesLoginSuccessful()
        {
            var user = new User { Username = "validUser" };

            _mockAuthService.Setup(a => a.Login(It.IsAny<string>(), It.IsAny<string>())).Returns(user);

            _viewModel.Login = "validUser";
            _viewModel.Password = "Password1";

            User receivedUser = null;
            _viewModel.LoginSuccessful += u => receivedUser = u;

            _viewModel.LoginCommand.Execute(null);

            Assert.NotNull(receivedUser);
            Assert.Equal(user.Username, receivedUser.Username);
        }

        [Fact]
        public void ExecuteLogin_ValidInput_NoUserReturned_ShowsErrorMessage()
        {
            _mockAuthService.Setup(a => a.Login(It.IsAny<string>(), It.IsAny<string>())).Returns((User)null);

            _viewModel.Login = "validUser";
            _viewModel.Password = "Password1";

            bool loginSuccessfulCalled = false;
            _viewModel.LoginSuccessful += _ => loginSuccessfulCalled = true;

            _viewModel.LoginCommand.Execute(null);

            Assert.False(loginSuccessfulCalled);
        }

        [Fact]
        public void RegisterCommand_InvokesSwitchToRegister()
        {
            bool switchInvoked = false;
            _viewModel.SwitchToRegister += () => switchInvoked = true;

            _viewModel.RegisterCommand.Execute(null);

            Assert.True(switchInvoked);
        }

        [Theory]
        [InlineData("Password1", true)]
        [InlineData("pass", false)]
        [InlineData("12345678", false)]
        [InlineData("abcdefgh", false)]
        [InlineData("abc12345", true)]
        public void IsPasswordStrong_CheckVariousPasswords(string password, bool expected)
        {
            _viewModel.Login = "user";
            _viewModel.Password = password;

            bool result = _viewModel.IsInputValid("user", password);

            Assert.Equal(expected, result);
        }
    }
}