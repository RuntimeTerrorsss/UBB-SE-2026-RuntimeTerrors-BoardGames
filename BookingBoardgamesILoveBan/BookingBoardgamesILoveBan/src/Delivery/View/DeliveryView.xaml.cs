using BookingBoardgamesILoveBan.src.PaymentCard.Navigation;
using BookingBoardgamesILoveBan.src.PaymentCard.View;
using BookingBoardgamesILoveBan.src.Chat.Service;
using BookingBoardgamesILoveBan.src.Delivery.Model.Validators;
using BookingBoardgamesILoveBan.src.Delivery.Service.MapServices;
using BookingBoardgamesILoveBan.src.Delivery.ViewModel;
using BookingBoardgamesILoveBan.src.Mocks.UserMock;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookingBoardgamesILoveBan.src.Interface.View
{
    public sealed partial class DeliveryView : Page
    {
        private DeliveryViewModel DeliveryViewModel;

        private double PendingLatitude;
        private double PendingLongitude;

        private int _currentUserId;
        private int _requestId;
        private int _incomingMessageId;
        private ConversationService _conversationService;
        private Window _currentWindow;

        public DeliveryView()
        {
            InitializeComponent();

        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEvent)
        {
            base.OnNavigatedTo(navigationEvent);

            var args = ((int userId, int requestId, int messageId, ConversationService conversationService, Window window))navigationEvent.Parameter;
            _currentUserId = args.userId;
            _requestId = args.requestId;
            _incomingMessageId = args.messageId;
            _conversationService = args.conversationService;
            _currentWindow = args.window;

            DeliveryViewModel = new DeliveryViewModel(
                _currentUserId,
                App.MapService,
                App.UserService,
                new AddressValidator()
            );

            DeliveryViewModel.OnNavigateToPayment = () =>
            {
                var bookingArgs = new BookingNavigationArguments
                {
                    RequestId = _requestId,
                    DeliveryAddress = DeliveryViewModel.CurrentAddress.ToString(),
                    BookingMessageId = _incomingMessageId,
                    ConversationService = _conversationService,
                    CurrentWindow = _currentWindow
                };
                //Debug.WriteLine(_conversationService.UserId);
                if (CashPaymentRadio.IsChecked == true)
                    Frame.Navigate(typeof(PaymentCash.View.CashPaymentPage), bookingArgs);
                else
                    Frame.Navigate(typeof(CardPaymentPage), bookingArgs);
            };

            DeliveryViewModel.StateChanged += RefreshUi;
            DeliveryViewModel.Initialize(_currentUserId);
            RefreshUi();
        }


        private void RefreshUi()
        {
            // Sync all text fields from CurrentAddress (also handles map auto-fill)
            CountryInput.Text = DeliveryViewModel.CurrentAddress.Country;
            CityInput.Text = DeliveryViewModel.CurrentAddress.City;
            StreetInput.Text = DeliveryViewModel.CurrentAddress.Street;
            StreetNumberInput.Text = DeliveryViewModel.CurrentAddress.StreetNumber;

            // Show/hide the map overlay
            MapOverlay.Visibility = DeliveryViewModel.IsMapVisible
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
            if (DeliveryViewModel.ValidationErrors.TryGetValue(fieldName, out string? message))
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


        private void OnFieldChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb && tb.Tag is string fieldName)
                DeliveryViewModel.OnFieldChange(fieldName, tb.Text);
        }

        private void OnSaveAddressChecked(object sender, RoutedEventArgs e)
            => DeliveryViewModel.IsSaveAddress = true;

        private void OnSaveAddressUnchecked(object sender, RoutedEventArgs e)
            => DeliveryViewModel.IsSaveAddress = false;

        private void OnOpenMapClicked(object sender, RoutedEventArgs e)
            => _ = InitializeMapAsync();

        private void OnCloseMapClicked(object sender, RoutedEventArgs e)
            => DeliveryViewModel.CloseMap();

        private void OnSubmitClicked(object sender, RoutedEventArgs e)
            => DeliveryViewModel.SubmitDelivery();

        private async void OnConfirmLocationClicked(object sender, RoutedEventArgs e)
            => await DeliveryViewModel.ConfirmMapLocationAsync(PendingLatitude, PendingLongitude);


        private async Task InitializeMapAsync()
        {
            DeliveryViewModel.OpenMap();

            await MapWebView.EnsureCoreWebView2Async();

            
            MapWebView.CoreWebView2.Settings.UserAgent = "BookingBoardgamesApp/1.0 (Contact: your.email@gmail.com)";
            MapWebView.CoreWebView2.WebMessageReceived -= OnMapMessageReceived;
            MapWebView.CoreWebView2.WebMessageReceived += OnMapMessageReceived;

            ///VERY
            ///VERY
            ///IMPORTANT
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

        private void OnMapMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                // Get the raw string sent from Javascript
                string rawMessage = e.TryGetWebMessageAsString();

                using JsonDocument doc = JsonDocument.Parse(rawMessage);

                PendingLatitude = doc.RootElement.GetProperty("lat").GetDouble();
                PendingLongitude = doc.RootElement.GetProperty("lng").GetDouble();

                Debug.WriteLine($"MAP CLICK REGISTERED -> Lat: {PendingLatitude}, Lon: {PendingLongitude}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"JSON PARSE ERROR: {ex.Message}");
            }
        }
    }
}