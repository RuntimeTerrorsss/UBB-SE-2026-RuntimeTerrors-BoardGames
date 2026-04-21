using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.Delivery.Model;
using BookingBoardgamesILoveBan.Src.Delivery.Model.Validators;
using BookingBoardgamesILoveBan.Src.Delivery.Service.MapServices;
using BookingBoardgamesILoveBan.Src.Delivery.ViewModel;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using Xunit;

namespace BookingBoardgamesLoveBan.Tests.Delivery
{
    public class DeliveryViewModelTests
    {
        private readonly FakeMapService fakeMapService;
        private readonly FakeUserService fakeUserService;
        private readonly FakeValidator fakeValidator;
        private readonly DeliveryViewModel viewModel;

        public DeliveryViewModelTests()
        {
            fakeMapService = new FakeMapService();
            fakeUserService = new FakeUserService();
            fakeValidator = new FakeValidator();

            viewModel = new DeliveryViewModel(
                currentUserId: 1,
                mapService: fakeMapService,
                userRepository: fakeUserService,
                validator: fakeValidator);
        }

        [Fact]
        public void OpenMap_SetsIsMapVisibleTrue()
        {
            viewModel.OpenMap();
            Assert.True(viewModel.IsMapVisible);
        }

        [Fact]
        public void OpenMap_TriggersStateChanged()
        {
            bool fired = false;
            viewModel.StateChanged += () => fired = true;
            viewModel.OpenMap();
            Assert.True(fired);
        }

        [Fact]
        public void CloseMap_SetsIsMapVisibleFalse()
        {
            viewModel.OpenMap();
            viewModel.CloseMap();

            Assert.False(viewModel.IsMapVisible);
        }

        [Fact]
        public void CloseMap_TriggersStateChanged()
        {
            bool fired = false;
            viewModel.StateChanged += () => fired = true;
            viewModel.CloseMap();
            Assert.True(fired);
        }

        [Fact]
        public void SubmitDelivery_WithErrors_DoesNotNavigateToPayment()
        {
            bool navigated = false;
            viewModel.OnNavigateToPayment = () => navigated = true;
            fakeValidator.ErrorsToReturn = new Dictionary<string, string> { { "City", "City is required" } };

            viewModel.SubmitDelivery();

            Assert.False(navigated);
        }

        [Fact]
        public void SubmitDelivery_WithNoErrors_NavigatesToPayment()
        {
            bool navigated = false;
            viewModel.OnNavigateToPayment = () => navigated = true;
            fakeValidator.ErrorsToReturn = new Dictionary<string, string>();

            viewModel.SubmitDelivery();

            Assert.True(navigated);
        }

        [Fact]
        public void SubmitDelivery_TriggersStateChanged()
        {
            bool fired = false;
            viewModel.StateChanged += () => fired = true;

            viewModel.SubmitDelivery();

            Assert.True(fired);
        }

        [Fact]
        public void SubmitDelivery_WithErrors_SetsValidationErrors()
        {
            fakeValidator.ErrorsToReturn = new Dictionary<string, string> { { "City", "City is required" } };

            viewModel.SubmitDelivery();

            Assert.True(viewModel.ValidationErrors.ContainsKey("City"));
        }

        [Fact]
        public void SubmitDelivery_WithNoErrors_SavesAddress_WhenIsSaveAddressAndUserNotNull()
        {
            fakeUserService.UserToReturn = new User(1, "name", "Romania", "Cluj", "street", "no");
            fakeValidator.ErrorsToReturn = new Dictionary<string, string>();

            var vm = new DeliveryViewModel(1, fakeMapService, fakeUserService, fakeValidator);
            vm.IsSaveAddress = true;
            vm.OnNavigateToPayment = () => { };

            var exception = Record.Exception(() => vm.SubmitDelivery());

            Assert.Null(exception);
        }

        [Fact]
        public void OnFieldChange_RemovesValidationError_WhenKeyExists()
        {
            viewModel.ValidationErrors["City"] = "City is required";

            viewModel.OnFieldChange("City", "Cluj-Napoca");

            Assert.False(viewModel.ValidationErrors.ContainsKey("City"));
        }

        [Fact]
        public void OnFieldChange_TriggersStateChanged_WhenErrorRemoved()
        {
            viewModel.ValidationErrors["City"] = "City is required";
            bool fired = false;
            viewModel.StateChanged += () => fired = true;

            viewModel.OnFieldChange("City", "Cluj-Napoca");

            Assert.True(fired);
        }

        [Fact]
        public void OnFieldChange_DoesNotTriggerStateChanged_WhenNoErrorToRemove()
        {
            bool fired = false;
            viewModel.StateChanged += () => fired = true;

            viewModel.OnFieldChange("City", "Cluj-Napoca");

            Assert.False(fired);
        }

        [Fact]
        public async Task ConfirmMapLocationAsync_ValidAddress_UpdatesCurrentAddress()
        {
            fakeMapService.AddressToReturn = new Address("Romania", "Cluj-Napoca", "Strada Universitatii", "1");

            await viewModel.ConfirmMapLocationAsync(46.77, 23.59);

            Assert.Equal(
                new { Country = "Romania", City = "Cluj-Napoca" },
                new { viewModel.CurrentAddress.Country, viewModel.CurrentAddress.City });
        }

        [Fact]
        public async Task ConfirmMapLocationAsync_ValidAddress_ClosesMap()
        {
            fakeMapService.AddressToReturn = new Address("Romania", "Cluj-Napoca", "Strada Universitatii", "1");
            viewModel.OpenMap();

            await viewModel.ConfirmMapLocationAsync(46.77, 23.59);

            Assert.False(viewModel.IsMapVisible);
        }

        [Fact]
        public async Task ConfirmMapLocationAsync_NullAddress_DoesNotUpdateCurrentAddress()
        {
            fakeMapService.AddressToReturn = null;
            var originalAddress = viewModel.CurrentAddress;

            await viewModel.ConfirmMapLocationAsync(0.1, 0.1);

            Assert.Equal(originalAddress, viewModel.CurrentAddress);
        }

        [Fact]
        public async Task ConfirmMapLocationAsync_NullAddress_DoesNotCloseMap()
        {
            fakeMapService.AddressToReturn = null;
            viewModel.OpenMap();

            await viewModel.ConfirmMapLocationAsync(0.1, 0.1);

            Assert.True(viewModel.IsMapVisible);
        }

        [Fact]
        public void Initialize_WithValidUser_UpdatesCurrentAddress()
        {
            fakeUserService.UserToReturn = new User(2, "name", "Romania", "Sibiu", "Strada Mare", "5");

            viewModel.Initialize(2);

            Assert.Equal(
                new { Country = "Romania", City = "Sibiu" },
                new { viewModel.CurrentAddress.Country, viewModel.CurrentAddress.City });
        }

        [Fact]
        public void Initialize_WithNullUser_DoesNotThrow()
        {
            fakeUserService.UserToReturn = null;

            var exception = Record.Exception(() => viewModel.Initialize(99));

            Assert.Null(exception);
        }

        private class FakeMapService : IMapService
        {
            public Address AddressToReturn { get; set; } = null;

            public Task<Address> GetAddressFromMapAsync(double latitude, double longitude)
            {
                return Task.FromResult(AddressToReturn);
            }
        }

        private class FakeUserService : IUserRepository
        {
            public User UserToReturn { get; set; } = null;

            public User GetById(int id)
            {
                return UserToReturn;
            }

            public void SaveAddress(int id, Address address)
            {
            }

            public decimal GetUserBalance(int userId)
            {
                return 0;
            }

            public void UpdateBalance(int userId, decimal newBalance)
            {
            }
        }

        private class FakeValidator : IValidator<Dictionary<string, string>, Address>
        {
            public Dictionary<string, string> ErrorsToReturn { get; set; } = new Dictionary<string, string>();

            public Dictionary<string, string> Validate(Address address)
            {
                return ErrorsToReturn;
            }
        }
    }
}