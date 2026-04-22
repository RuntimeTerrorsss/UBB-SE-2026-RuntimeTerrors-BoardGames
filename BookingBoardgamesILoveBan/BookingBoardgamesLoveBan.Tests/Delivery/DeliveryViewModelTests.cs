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
        public void OpenMap_Invoked_SetsIsMapVisibleTrue()
        {
            viewModel.OpenMap();
            Assert.True(viewModel.IsMapVisible);
        }

        [Fact]
        public void OpenMap_Invoked_TriggersStateChanged()
        {
            bool fired = false;
            viewModel.StateChanged += () => fired = true;
            viewModel.OpenMap();
            Assert.True(fired);
        }

        [Fact]
        public void CloseMap_Invoked_SetsIsMapVisibleFalse()
        {
            viewModel.OpenMap();
            viewModel.CloseMap();

            Assert.False(viewModel.IsMapVisible);
        }

        [Fact]
        public void CloseMap_Invoked_TriggersStateChanged()
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
        public void SubmitDelivery_Always_TriggersStateChanged()
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
        public void SubmitDelivery_IsSaveAddressAndUserNotNull_SavesAddress()
        {
            fakeUserService.UserToReturn = new User(1, "name", "Romania", "Cluj", "street", "no");
            fakeValidator.ErrorsToReturn = new Dictionary<string, string>();

            var vm = new DeliveryViewModel(1, fakeMapService, fakeUserService, fakeValidator);
            vm.IsSaveAddress = true;
            vm.OnNavigateToPayment = () => { };

            var exception = Record.Exception(() => vm.SubmitDelivery());

            Assert.Null(exception);
            Assert.True(fakeUserService.SaveAddressCalled);
        }

        [Fact]
        public void SubmitDelivery_IsSaveAddressFalse_DoesNotSaveAddress()
        {
            fakeUserService.UserToReturn = new User(1, "name", "Romania", "Cluj", "street", "no");
            fakeValidator.ErrorsToReturn = new Dictionary<string, string>();

            var vm = new DeliveryViewModel(1, fakeMapService, fakeUserService, fakeValidator);
            vm.IsSaveAddress = false;
            vm.OnNavigateToPayment = () => { };

            vm.SubmitDelivery();

            Assert.False(fakeUserService.SaveAddressCalled);
        }

        [Fact]
        public void SubmitDelivery_CurrentUserNull_DoesNotSaveAddress()
        {
            fakeUserService.UserToReturn = null;
            fakeValidator.ErrorsToReturn = new Dictionary<string, string>();

            var vm = new DeliveryViewModel(1, fakeMapService, fakeUserService, fakeValidator);
            vm.IsSaveAddress = true;
            vm.OnNavigateToPayment = () => { };

            vm.SubmitDelivery();

            Assert.False(fakeUserService.SaveAddressCalled);
        }

        [Fact]
        public void OnFieldChange_KeyExists_RemovesValidationError()
        {
            viewModel.ValidationErrors["City"] = "City is required";

            viewModel.OnFieldChange("City", "Cluj-Napoca");

            Assert.False(viewModel.ValidationErrors.ContainsKey("City"));
        }

        [Fact]
        public void OnFieldChange_ErrorRemoved_TriggersStateChanged()
        {
            viewModel.ValidationErrors["City"] = "City is required";
            bool fired = false;
            viewModel.StateChanged += () => fired = true;

            viewModel.OnFieldChange("City", "Cluj-Napoca");

            Assert.True(fired);
        }

        [Fact]
        public void OnFieldChange_NoErrorToRemove_DoesNotTriggerStateChanged()
        {
            bool fired = false;
            viewModel.StateChanged += () => fired = true;

            viewModel.OnFieldChange("City", "Cluj-Napoca");

            Assert.False(fired);
        }

        [Fact]
        public void OnFieldChange_ValidProperty_UpdatesFieldValue()
        {
            viewModel.CurrentAddress.City = "Old City";

            viewModel.OnFieldChange("City", "New City");

            Assert.Equal("New City", viewModel.CurrentAddress.City);
        }

        [Fact]
        public void OnFieldChange_InvalidProperty_DoesNothing()
        {
            var originalAddress = new Address("Country", "City", "Street", "Number");
            viewModel.CurrentAddress = new Address("Country", "City", "Street", "Number");

            viewModel.OnFieldChange("NonExistentProperty", "New Value");

            Assert.Equal(originalAddress.City, viewModel.CurrentAddress.City);
            Assert.Equal(originalAddress.Country, viewModel.CurrentAddress.Country);
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
        public void Initialize_ValidUser_UpdatesCurrentAddress()
        {
            fakeUserService.UserToReturn = new User(2, "name", "Romania", "Sibiu", "Strada Mare", "5");

            viewModel.Initialize(2);

            Assert.Equal(2, viewModel.CurrentId);
            Assert.Equal(
                new { Country = "Romania", City = "Sibiu" },
                new { viewModel.CurrentAddress.Country, viewModel.CurrentAddress.City });
        }

        [Fact]
        public void Initialize_NullUser_DoesNotThrow()
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
            public bool SaveAddressCalled { get; set; } = false;

            public User GetById(int id)
            {
                return UserToReturn;
            }

            public void SaveAddress(int id, Address address)
            {
                SaveAddressCalled = true;
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