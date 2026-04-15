using BookingBoardgamesILoveBan.Src.PaymentCard.Navigation;
using BookingBoardgamesILoveBan.Src.Chat.View;
using BookingBoardgamesILoveBan.Src.PaymentCash.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BookingBoardgamesILoveBan.Src.PaymentCash.View
{
    public sealed partial class CashPaymentPage : Page
    {
        public CashPaymentViewModel ViewModel { get; set; }
        private Window currentWindow;

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
                    booking.RequestIdentifier,
                    booking.DeliveryAddress,
                    booking.BookingMessageIdentifier,
                    booking.ConversationService);

                DataContext = ViewModel;
                currentWindow = booking.CurrentWindow;
            }
        }
        private void NavigateToChatButton_Click(object sender, RoutedEventArgs e)
        {
            currentWindow.Close();
            /*
            if (Frame.CanGoBack)
            {
                Frame.Navigate(typeof(ChatPageView), App.CURRENT_USER_WILL_DELETE);
            }*/
        }
    }
}