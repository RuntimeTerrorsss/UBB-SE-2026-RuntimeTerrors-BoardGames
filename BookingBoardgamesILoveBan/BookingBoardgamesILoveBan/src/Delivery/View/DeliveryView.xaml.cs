using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.Chat.Service;
using BookingBoardgamesILoveBan.Src.Delivery.Model.Validators;
using BookingBoardgamesILoveBan.Src.Delivery.Service.MapServices;
using BookingBoardgamesILoveBan.Src.Delivery.ViewModel;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using BookingBoardgamesILoveBan.Src.PaymentCard.Navigation;
using BookingBoardgamesILoveBan.Src.PaymentCard.View;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;

namespace BookingBoardgamesILoveBan.Src.Delivery.View
{
    public sealed partial class DeliveryView : Page
    {
        private DeliveryViewModel deliveryViewModel;

        private double pendingLatitude;
        private double pendingLongitude;

        private int currentUserId;
        private int requestId;
        private int incomingMessageId;
        private ConversationService conversationService;
        private Window currentWindow;

        public DeliveryView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEvent)
        {
            base.OnNavigatedTo(navigationEvent);

            var arguments = ((int userId, int requestId, int messageId, ConversationService conversationService, Window window))navigationEvent.Parameter;
            currentUserId = arguments.userId;
            requestId = arguments.requestId;
            incomingMessageId = arguments.messageId;
            conversationService = arguments.conversationService;
            currentWindow = arguments.window;

            deliveryViewModel = new DeliveryViewModel(
                currentUserId,
                App.MapService,
                App.UserRepository,
                new AddressValidator());

            deliveryViewModel.OnNavigateToPayment = () =>
            {
                var bookingArguments = new BookingNavigationArguments
                {
                    RequestIdentifier = requestId,
                    DeliveryAddress = deliveryViewModel.CurrentAddress.ToString(),
                    BookingMessageIdentifier = incomingMessageId,
                    ConversationService = conversationService,
                    CurrentWindow = currentWindow
                };
                // Debug.WriteLine(_conversationService.UserId);
                if (CashPaymentRadio.IsChecked == true)
                {
                    Frame.Navigate(typeof(PaymentCash.View.CashPaymentPage), bookingArguments);
                }
                else
                {
                    Frame.Navigate(typeof(CardPaymentPage), bookingArguments);
                }
            };

            deliveryViewModel.StateChanged += RefreshUi;
            deliveryViewModel.Initialize(currentUserId);
            RefreshUi();
        }

        private void RefreshUi()
        {
            // Sync all text fields from CurrentAddress (also handles map auto-fill)
            CountryInput.Text = deliveryViewModel.CurrentAddress.Country;
            CityInput.Text = deliveryViewModel.CurrentAddress.City;
            StreetInput.Text = deliveryViewModel.CurrentAddress.Street;
            StreetNumberInput.Text = deliveryViewModel.CurrentAddress.StreetNumber;

            // Show/hide the map overlay
            MapOverlay.Visibility = deliveryViewModel.IsMapVisible
                ? Visibility.Visible
                : Visibility.Collapsed;

            // Show or clear validation errors per field
            ShowFieldError(CountryInput, CountryError, "Country");
            ShowFieldError(CityInput, CityError, "City");
            ShowFieldError(StreetInput, StreetError, "Street");
            ShowFieldError(StreetNumberInput, StreetNumberError, "StreetNumber");
        }

        private void ShowFieldError(TextBox input, TextBlock errorBlock, string fieldName)
        {
            if (deliveryViewModel.ValidationErrors.TryGetValue(fieldName, out string? message))
            {
                errorBlock.Text = message;
                errorBlock.Visibility = Visibility.Visible;
                VisualStateManager.GoToState(input, "InvalidUnfocused", true);
            }
            else
            {
                errorBlock.Visibility = Visibility.Collapsed;
                VisualStateManager.GoToState(input, "Normal", true);
            }
        }

        private void OnFieldChanged(object sender, TextChangedEventArgs textEventArguments)
        {
            if (sender is TextBox tb && tb.Tag is string fieldName)
            {
                deliveryViewModel.OnFieldChange(fieldName, tb.Text);
            }
        }

        private void OnSaveAddressChecked(object sender, RoutedEventArgs routedEventArguments)
            => deliveryViewModel.IsSaveAddress = true;

        private void OnSaveAddressUnchecked(object sender, RoutedEventArgs routedEventArguments)
            => deliveryViewModel.IsSaveAddress = false;

        private void OnOpenMapClicked(object sender, RoutedEventArgs routedEventArguments)
            => _ = InitializeMapAsync();

        private void OnCloseMapClicked(object sender, RoutedEventArgs routedEventArguments)
            => deliveryViewModel.CloseMap();

        private void OnSubmitClicked(object sender, RoutedEventArgs routedEventArguments)
            => deliveryViewModel.SubmitDelivery();

        private async void OnConfirmLocationClicked(object sender, RoutedEventArgs routedEventArguments)
            => await deliveryViewModel.ConfirmMapLocationAsync(pendingLatitude, pendingLongitude);

        private async Task InitializeMapAsync()
        {
            deliveryViewModel.OpenMap();
            await MapWebView.EnsureCoreWebView2Async();

            MapWebView.CoreWebView2.Settings.UserAgent = "BookingBoardgamesApp/1.0 (Contact: your.email@gmail.com)";
            MapWebView.CoreWebView2.WebMessageReceived -= OnMapMessageReceived;
            MapWebView.CoreWebView2.WebMessageReceived += OnMapMessageReceived;

            /// VERY
            /// VERY
            /// IMPORTANT
            /// GO to your device settings to Time and Language
            /// Select Region
            /// Select region format
            /// Change to English US
            MapWebView.CoreWebView2.NavigateToString("""
                <!DOCTYPE html>
                <html>
                <head>
                  <meta charset="utf-8"/>
                  <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"/>
                  <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
                  <style>html, body, #map { height: 100%; margin: 0; padding: 0; }</style>
                </head>
                <body>
                  <div id="map"></div>
                  <script>
                    var map = L.map('map').setView([46.7712, 23.5897], 13);
                    var marker = null;
                    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                        attribution: '© OpenStreetMap contributors'
                    }).addTo(map);
                    map.on('click', function(e) {
                        if (marker) marker.setLatLng(e.latlng);
                        else marker = L.marker(e.latlng).addTo(map);
                        window.chrome.webview.postMessage(
                            JSON.stringify({ lat: e.latlng.lat, lng: e.latlng.lng }));
                    });
                  </script>
                </body>
                </html>
                """);
        }

        private void OnMapMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs eventArguments)
        {
            try
            {
                string rawMessage = eventArguments.TryGetWebMessageAsString();

                using JsonDocument jsonDocument = JsonDocument.Parse(rawMessage);

                pendingLatitude = jsonDocument.RootElement.GetProperty("lat").GetDouble();
                pendingLongitude = jsonDocument.RootElement.GetProperty("lng").GetDouble();

                Debug.WriteLine($"MAP CLICK REGISTERED -> Lat: {pendingLatitude}, Lon: {pendingLongitude}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"JSON PARSE ERROR: {ex.Message}");
            }
        }
    }
}