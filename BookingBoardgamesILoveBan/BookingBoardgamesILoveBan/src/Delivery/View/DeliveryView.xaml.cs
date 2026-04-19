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

            var args = ((int userId, int requestId, int messageId, ConversationService conversationService, Window window))navigationEvent.Parameter;
            currentUserId = args.userId;
            requestId = args.requestId;
            incomingMessageId = args.messageId;
            conversationService = args.conversationService;
            currentWindow = args.window;

            deliveryViewModel = new DeliveryViewModel(
                currentUserId,
                App.MapService,
                App.UserRepository,
                new AddressValidator());

            deliveryViewModel.OnNavigateToPayment = () =>
            {
                var bookingArgs = new BookingNavigationArguments
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
                    Frame.Navigate(typeof(PaymentCash.View.CashPaymentPage), bookingArgs);
                }
                else
                {
                    Frame.Navigate(typeof(CardPaymentPage), bookingArgs);
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

        private void OnFieldChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb && tb.Tag is string fieldName)
            {
                deliveryViewModel.OnFieldChange(fieldName, tb.Text);
            }
        }

        private void OnSaveAddressChecked(object sender, RoutedEventArgs e)
            => deliveryViewModel.IsSaveAddress = true;

        private void OnSaveAddressUnchecked(object sender, RoutedEventArgs e)
            => deliveryViewModel.IsSaveAddress = false;

        private void OnOpenMapClicked(object sender, RoutedEventArgs e)
            => _ = InitializeMapAsync();

        private void OnCloseMapClicked(object sender, RoutedEventArgs e)
            => deliveryViewModel.CloseMap();

        private void OnSubmitClicked(object sender, RoutedEventArgs e)
            => deliveryViewModel.SubmitDelivery();

        private async void OnConfirmLocationClicked(object sender, RoutedEventArgs e)
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

        private void OnMapMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                // Get the raw string sent from Javascript
                string rawMessage = e.TryGetWebMessageAsString();

                using JsonDocument doc = JsonDocument.Parse(rawMessage);

                pendingLatitude = doc.RootElement.GetProperty("lat").GetDouble();
                pendingLongitude = doc.RootElement.GetProperty("lng").GetDouble();

                Debug.WriteLine($"MAP CLICK REGISTERED -> Lat: {pendingLatitude}, Lon: {pendingLongitude}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"JSON PARSE ERROR: {ex.Message}");
            }
        }
    }
}