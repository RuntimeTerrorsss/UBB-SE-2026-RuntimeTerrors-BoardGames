using BookingBoardgamesILoveBan.src.PaymentCard.Navigation;
using BookingBoardgamesILoveBan.src.Chat.View;
using BookingBoardgamesILoveBan.src.PaymentCash.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BookingBoardgamesILoveBan.src.PaymentCash.View
{
    public sealed partial class CashPaymentPage : Page
    {
        public CashPaymentViewModel ViewModel { get; set; }
        private Window _currentWindow;

        public CashPaymentPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

			if (e.Parameter is BookingNavigationArguments booking)
            {
                ViewModel = new CashPaymentViewModel(
                    App.CashPaymentService,
                    App.UserService,
                    App.RequestService,
                    App.GameService,
                    booking.RequestId,
                    booking.DeliveryAddress,
                    booking.BookingMessageId,
                    booking.ConversationService
                );

                DataContext = ViewModel;
                _currentWindow = booking.CurrentWindow;
            }
        }
        private void NavigateToChatButton_Click(object sender, RoutedEventArgs e)
        {
            _currentWindow.Close();
            /*
            if (Frame.CanGoBack)
            {
                Frame.Navigate(typeof(ChatPageView), App.CURRENT_USER_WILL_DELETE);
            }*/
        }
    }
}