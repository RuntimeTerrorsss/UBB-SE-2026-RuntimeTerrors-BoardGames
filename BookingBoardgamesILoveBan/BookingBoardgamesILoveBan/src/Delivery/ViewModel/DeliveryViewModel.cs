using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.Delivery.Model;
using BookingBoardgamesILoveBan.Src.Delivery.Model.Validators;
using BookingBoardgamesILoveBan.Src.Delivery.Service.MapServices;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;

namespace BookingBoardgamesILoveBan.Src.Delivery.ViewModel
{
    public class DeliveryViewModel
    {
        public DeliveryViewModel(
            int currentUserId,
            IMapService mapService,
            IUserRepository userService,
            IValidator<Dictionary<string, string>, Address> validator)
        {
            MapService = mapService;
            UserService = userService;
            Validator = validator;
            CurrentId = currentUserId;
            CurrentUser = UserService.GetById(currentUserId);
            CurrentAddress = CurrentUser != null
                ? new Address(CurrentUser.Country, CurrentUser.City, CurrentUser.Street, CurrentUser.StreetNumber)
                : new Address();
        }

        public event Action StateChanged;

        public Address CurrentAddress { get; set; }

        public bool IsMapVisible { get; set; } = false;

        public bool IsSaveAddress { get; set; } = false;

        public Dictionary<string, string> ValidationErrors { get; set; } = new Dictionary<string, string>();

        public User CurrentUser { get; set; }

        public int CurrentId { get; set; } = 1;

        public Action OnNavigateToPayment { get; set; }

        private IMapService MapService { get; set; }

        private IUserRepository UserService { get; set; }

        private IValidator<Dictionary<string, string>, Address> Validator { get; set; }

        public void Initialize(int userId)
        {
            CurrentId = userId;
            CurrentUser = UserService.GetById(userId);

            if (CurrentUser != null)
            {
                CurrentAddress = new Address(
                    CurrentUser.Country,
                    CurrentUser.City,
                    CurrentUser.Street,
                    CurrentUser.StreetNumber);
            }
        }

        public void OnFieldChange(string fieldName, string newValue)
        {
            typeof(Address).GetProperty(fieldName)?.SetValue(CurrentAddress, newValue);

            if (ValidationErrors.Remove(fieldName))
            {
                StateChanged?.Invoke();
            }
        }

        public void OpenMap()
        {
            IsMapVisible = true;
            StateChanged?.Invoke();
        }

        public void CloseMap()
        {
            IsMapVisible = false;
            StateChanged?.Invoke();
        }

        public async Task ConfirmMapLocationAsync(double latitude, double longitude)
        {
            Debug.WriteLine($"--- CONFIRM LOCATION CLICKED --- Lat: {latitude}, Lon: {longitude}");
            Address resolved = await MapService.GetAddressFromMapAsync(latitude, longitude);

            if (resolved != null)
            {
                CurrentAddress = resolved;
                IsMapVisible = false;
                StateChanged?.Invoke();
            }
            else
            {
                Debug.WriteLine($"Address not valid, received: Lat={latitude}, Lon={longitude}");
            }
        }

        public void SubmitDelivery()
        {
            ValidationErrors = Validator.Validate(CurrentAddress);
            StateChanged?.Invoke();

            if (ValidationErrors.Count == 0)
            {
                if (IsSaveAddress && CurrentUser is not null)
                {
                    UserService.SaveAddress(CurrentUser.Id, CurrentAddress);
                }

                OnNavigateToPayment?.Invoke();
            }
        }
    }
}