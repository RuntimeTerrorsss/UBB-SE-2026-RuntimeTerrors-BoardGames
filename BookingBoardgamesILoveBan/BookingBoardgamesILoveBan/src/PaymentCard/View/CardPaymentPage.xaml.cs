using System;
using BookingBoardgamesILoveBan.Src.PaymentCard.Navigation;
using BookingBoardgamesILoveBan.Src.PaymentCard.ViewModel;
using BookingBoardgamesILoveBan.Src.View;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;

namespace BookingBoardgamesILoveBan.Src.PaymentCard.View
{
    public sealed partial class CardPaymentPage : Page
    {
        public CardPaymentViewModel PaymentViewModel { get; set; }
        private Window activeCurrentWindow;

        public CardPaymentPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArguments)
        {
            base.OnNavigatedTo(navigationEventArguments);
            var bookingArguments = (BookingNavigationArguments)navigationEventArguments.Parameter;

            PaymentViewModel = new CardPaymentViewModel(
                App.CardPaymentService,
                App.UserRepository,
                bookingArguments.RequestIdentifier,
                bookingArguments.DeliveryAddress,
                bookingArguments.BookingMessageIdentifier,
                bookingArguments.ConversationService);

            DataContext = PaymentViewModel;
            activeCurrentWindow = bookingArguments.CurrentWindow;

            Bindings.Update();

            PaymentViewModel.NavigateBackwardsAction = () =>
            {
                activeCurrentWindow.Close();
            };
            PaymentViewModel.NavigateToExitAction = () =>
            {
                activeCurrentWindow.Close();
            };

            PaymentViewModel.OnPageActivated();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs onNavigatedFromEventArguments)
        {
            base.OnNavigatedFrom(onNavigatedFromEventArguments);
            PaymentViewModel.OnPageDeactivated();
        }

        protected override void OnPointerMoved(PointerRoutedEventArgs onPointerMovedEventArguments)
        {
            base.OnPointerMoved(onPointerMovedEventArguments);
            PaymentViewModel.ResetInactivityCommand.Execute(null);
        }

        protected override void OnKeyDown(KeyRoutedEventArgs onKeyDownEventArguments)
        {
            base.OnKeyDown(onKeyDownEventArguments);
            PaymentViewModel.ResetInactivityCommand.Execute(null);
        }

        private async void OnTermsLinkClick(Hyperlink hyperlinkSender, HyperlinkClickEventArgs onTermsClickedEventArguments)
        {
            var termsDialog = new ContentDialog
            {
                Title = "Terms of Service",
                Content = "By completing this payment you agree to our refund policy. " +
                                 "Rentals are non-refundable once the rental period has started.",
                CloseButtonText = "Close",
                XamlRoot = this.XamlRoot
            };
            await termsDialog.ShowAsync();
        }
    }
}