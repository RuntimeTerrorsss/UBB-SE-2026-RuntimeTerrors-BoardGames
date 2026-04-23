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
        private readonly DeliveryViewModel deliveryViewModel;

        public DeliveryViewModelTests()
        {
            fakeMapService = new FakeMapService();
            fakeUserService = new FakeUserService();
            fakeValidator = new FakeValidator();

            deliveryViewModel = new DeliveryViewModel(
                currentUserId: 1,
                mapService: fakeMapService,
                userRepository: fakeUserService,
                validator: fakeValidator);
        }

        [Fact]
        public void OpenMap_Invoked_SetsIsMapVisibleTrue()
        {
            deliveryViewModel.OpenMap();
            Assert.True(deliveryViewModel.IsMapVisible);
        }

        [Fact]
        public void OpenMap_Invoked_TriggersStateChanged()
        {
            bool fired = false;
            deliveryViewModel.StateChanged += () => fired = true;
            deliveryViewModel.OpenMap();
            Assert.True(fired);
        }

        [Fact]
        public void CloseMap_Invoked_SetsIsMapVisibleFalse()
        {
            deliveryViewModel.OpenMap();
            deliveryViewModel.CloseMap();

            Assert.False(deliveryViewModel.IsMapVisible);
        }

        [Fact]
        public void CloseMap_Invoked_TriggersStateChanged()
        {
            bool fired = false;
            deliveryViewModel.StateChanged += () => fired = true;
            deliveryViewModel.CloseMap();
            Assert.True(fired);
        }

        [Fact]
        public void SubmitDelivery_WithErrors_DoesNotNavigateToPayment()
        {
            bool navigated = false;
            deliveryViewModel.OnNavigateToPayment = () => navigated = true;
            fakeValidator.ErrorsToReturn = new Dictionary<string, string> { { "City", "City is required" } };

            deliveryViewModel.SubmitDelivery();

            Assert.False(navigated);
        }

        [Fact]
        public void SubmitDelivery_WithNoErrors_NavigatesToPayment()
        {
            bool navigated = false;
            deliveryViewModel.OnNavigateToPayment = () => navigated = true;
            fakeValidator.ErrorsToReturn = new Dictionary<string, string>();

            deliveryViewModel.SubmitDelivery();

            Assert.True(navigated);
        }

        [Fact]
        public void SubmitDelivery_Always_TriggersStateChanged()
        {
            bool fired = false;
            deliveryViewModel.StateChanged += () => fired = true;

            deliveryViewModel.SubmitDelivery();

            Assert.True(fired);
        }

        [Fact]
        public void SubmitDelivery_WithErrors_SetsValidationErrors()
        {
            fakeValidator.ErrorsToReturn = new Dictionary<string, string> { { "City", "City is required" } };

            deliveryViewModel.SubmitDelivery();

            Assert.True(deliveryViewModel.ValidationErrors.ContainsKey("City"));
        }

        [Fact]
        public void SubmitDelivery_IsSaveAddressAndUserNotNull_SavesAddress()
        {
            fakeUserService.UserToReturn = new User(1, "name", "Romania", "Cluj", "street", "no");
            fakeValidator.ErrorsToReturn = new Dictionary<string, string>();

            var deliveryViewModel = new DeliveryViewModel(1, fakeMapService, fakeUserService, fakeValidator);
            deliveryViewModel.IsSaveAddress = true;
            deliveryViewModel.OnNavigateToPayment = () => { };

            var exception = Record.Exception(() => deliveryViewModel.SubmitDelivery());

            Assert.Null(exception);
            Assert.True(fakeUserService.SaveAddressCalled);
        }

        [Fact]
        public void SubmitDelivery_IsSaveAddressFalse_DoesNotSaveAddress()
        {
            fakeUserService.UserToReturn = new User(1, "name", "Romania", "Cluj", "street", "no");
            fakeValidator.ErrorsToReturn = new Dictionary<string, string>();

            var deliveryViewModel = new DeliveryViewModel(1, fakeMapService, fakeUserService, fakeValidator);
            deliveryViewModel.IsSaveAddress = false;
            deliveryViewModel.OnNavigateToPayment = () => { };

            deliveryViewModel.SubmitDelivery();

            Assert.False(fakeUserService.SaveAddressCalled);
        }

        [Fact]
        public void SubmitDelivery_CurrentUserNull_DoesNotSaveAddress()
        {
            fakeUserService.UserToReturn = null;
            fakeValidator.ErrorsToReturn = new Dictionary<string, string>();

            var deliveryViewModel = new DeliveryViewModel(1, fakeMapService, fakeUserService, fakeValidator);
            deliveryViewModel.IsSaveAddress = true;
            deliveryViewModel.OnNavigateToPayment = () => { };

            deliveryViewModel.SubmitDelivery();

            Assert.False(fakeUserService.SaveAddressCalled);
        }

        [Fact]
        public void OnFieldChange_KeyExists_RemovesValidationError()
        {
            deliveryViewModel.ValidationErrors["City"] = "City is required";

            deliveryViewModel.OnFieldChange("City", "Cluj-Napoca");

            Assert.False(deliveryViewModel.ValidationErrors.ContainsKey("City"));
        }

        [Fact]
        public void OnFieldChange_ErrorRemoved_TriggersStateChanged()
        {
            deliveryViewModel.ValidationErrors["City"] = "City is required";
            bool fired = false;
            deliveryViewModel.StateChanged += () => fired = true;

            deliveryViewModel.OnFieldChange("City", "Cluj-Napoca");

            Assert.True(fired);
        }

        [Fact]
        public void OnFieldChange_NoErrorToRemove_DoesNotTriggerStateChanged()
        {
            bool fired = false;
            deliveryViewModel.StateChanged += () => fired = true;

            deliveryViewModel.OnFieldChange("City", "Cluj-Napoca");

            Assert.False(fired);
        }

        [Fact]
        public void OnFieldChange_ValidProperty_UpdatesFieldValue()
        {
            deliveryViewModel.CurrentAddress.City = "Old City";

            deliveryViewModel.OnFieldChange("City", "New City");

            Assert.Equal("New City", deliveryViewModel.CurrentAddress.City);
        }

        [Fact]
        public void OnFieldChange_InvalidProperty_DoesNothing()
        {
            var originalAddress = new Address("Country", "City", "Street", "Number");
            deliveryViewModel.CurrentAddress = new Address("Country", "City", "Street", "Number");

            deliveryViewModel.OnFieldChange("NonExistentProperty", "New Value");

            Assert.Equal(originalAddress.City, deliveryViewModel.CurrentAddress.City);
            Assert.Equal(originalAddress.Country, deliveryViewModel.CurrentAddress.Country);
        }

        [Fact]
        public async Task ConfirmMapLocationAsync_ValidAddress_UpdatesCurrentAddress()
        {
            fakeMapService.AddressToReturn = new Address("Romania", "Cluj-Napoca", "Strada Universitatii", "1");

            await deliveryViewModel.ConfirmMapLocationAsync(46.77, 23.59);

            Assert.Equal(
                new { Country = "Romania", City = "Cluj-Napoca" },
                new { deliveryViewModel.CurrentAddress.Country, deliveryViewModel.CurrentAddress.City });
        }

        [Fact]
        public async Task ConfirmMapLocationAsync_ValidAddress_ClosesMap()
        {
            fakeMapService.AddressToReturn = new Address("Romania", "Cluj-Napoca", "Strada Universitatii", "1");
            deliveryViewModel.OpenMap();

            await deliveryViewModel.ConfirmMapLocationAsync(46.77, 23.59);

            Assert.False(deliveryViewModel.IsMapVisible);
        }

        [Fact]
        public async Task ConfirmMapLocationAsync_NullAddress_DoesNotUpdateCurrentAddress()
        {
            fakeMapService.AddressToReturn = null;
            var originalAddress = deliveryViewModel.CurrentAddress;

            await deliveryViewModel.ConfirmMapLocationAsync(0.1, 0.1);

            Assert.Equal(originalAddress, deliveryViewModel.CurrentAddress);
        }

        [Fact]
        public async Task ConfirmMapLocationAsync_NullAddress_DoesNotCloseMap()
        {
            fakeMapService.AddressToReturn = null;
            deliveryViewModel.OpenMap();

            await deliveryViewModel.ConfirmMapLocationAsync(0.1, 0.1);

            Assert.True(deliveryViewModel.IsMapVisible);
        }

        [Fact]
        public void Initialize_ValidUser_UpdatesCurrentAddress()
        {
            fakeUserService.UserToReturn = new User(2, "name", "Romania", "Sibiu", "Strada Mare", "5");

            deliveryViewModel.Initialize(2);

            Assert.Equal(2, deliveryViewModel.CurrentId);
            Assert.Equal(
                new { Country = "Romania", City = "Sibiu" },
                new { deliveryViewModel.CurrentAddress.Country, deliveryViewModel.CurrentAddress.City });
        }

        [Fact]
        public void Initialize_NullUser_DoesNotThrow()
        {
            fakeUserService.UserToReturn = null;

            var exception = Record.Exception(() => deliveryViewModel.Initialize(99));

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