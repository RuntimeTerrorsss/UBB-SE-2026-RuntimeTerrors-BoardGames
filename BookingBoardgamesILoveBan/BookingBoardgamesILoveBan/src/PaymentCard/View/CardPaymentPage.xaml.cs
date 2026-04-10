using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using BookingBoardgamesILoveBan.Src.PaymentCard.Navigation;
using BookingBoardgamesILoveBan.Src.PaymentCard.ViewModel;
using BookingBoardgamesILoveBan.Src.PaymentCard.View;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using BookingBoardgamesILoveBan.Src.View;

// To learn more about WinUI, the WinUI project structure,
/// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BookingBoardgamesILoveBan.Src.PaymentCard.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CardPaymentPage : Page
    {
        public CardPaymentViewModel ViewModel { get; set; }
        private Window currentWindow;

        public CardPaymentPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEvent)
        {
            base.OnNavigatedTo(navigationEvent);
            var booking = (BookingNavigationArguments)navigationEvent.Parameter;

            ViewModel = new CardPaymentViewModel(
                App.CardPaymentService,
                App.UserService,
                booking.RequestId,
                booking.DeliveryAddress,
                booking.BookingMessageId,
                booking.ConversationService);

            DataContext = ViewModel;
            currentWindow = booking.CurrentWindow;

            Bindings.Update();

            ViewModel.NavigateBack = () =>
            {
                currentWindow.Close();
            };
            ViewModel.NavigateToExit = () =>
            {
                currentWindow.Close();
            };

            ViewModel.OnActivated();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs onNavigatedFormEvent)
        {
            base.OnNavigatedFrom(onNavigatedFormEvent);
            ViewModel.OnDeactivated();
        }

        protected override void OnPointerMoved(PointerRoutedEventArgs onPointerMovedEvent)
        {
            base.OnPointerMoved(onPointerMovedEvent);
            ViewModel.ResetInactivityCommand.Execute(null);
        }

        protected override void OnKeyDown(KeyRoutedEventArgs onKeyDownEvent)
        {
            base.OnKeyDown(onKeyDownEvent);
            ViewModel.ResetInactivityCommand.Execute(null);
        }

        private async void OnTermsLinkClick(Hyperlink sender, HyperlinkClickEventArgs onTermsClickedEvent)
        {
            var dialog = new ContentDialog
            {
                Title = "Terms of Service",
                Content = "By completing this payment you agree to our refund policy. " +
                                 "Rentals are non-refundable once the rental period has started.",
                CloseButtonText = "Close",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
