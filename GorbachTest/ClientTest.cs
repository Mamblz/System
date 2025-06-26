using CrmSystem.Models;
using CrmSystem.Services;
using CrmSystem.ViewModels;
using Moq;

namespace CrmSystem.Tests
{
    public class ClientEditViewModelTests
    {
        [Fact]
        public void Save_NewClient_CallsAddClient_AndRequestsCloseWithTrue()
        {
            var client = new Client { Id = 0 };
            var mockService = new Mock<IClientService>();
            mockService.Setup(s => s.AddClient(client)).Verifiable();

            var vm = new ClientEditViewModel(client, mockService.Object);

            bool? closedWith = null;
            vm.RequestClose += (result) => closedWith = result;

            vm.SaveCommand.Execute(null);

            mockService.Verify(s => s.AddClient(client), Times.Once);
            Assert.True(closedWith);
        }

        [Fact]
        public void Save_ExistingClient_CallsUpdateClient_AndRequestsCloseWithTrue()
        {
            var client = new Client { Id = 123 };
            var mockService = new Mock<IClientService>();
            mockService.Setup(s => s.UpdateClient(client)).Verifiable();

            var vm = new ClientEditViewModel(client, mockService.Object);

            bool? closedWith = null;
            vm.RequestClose += (result) => closedWith = result;

            vm.SaveCommand.Execute(null);

            mockService.Verify(s => s.UpdateClient(client), Times.Once);
            Assert.True(closedWith);
        }

        [Fact]
        public void CancelCommand_InvokesRequestCloseWithFalse()
        {
            var client = new Client();
            var mockService = new Mock<IClientService>();
            var vm = new ClientEditViewModel(client, mockService.Object);

            bool? closedWith = null;
            vm.RequestClose += (result) => closedWith = result;

            vm.CancelCommand.Execute(null);

            Assert.False(closedWith);
        }
    }
}
