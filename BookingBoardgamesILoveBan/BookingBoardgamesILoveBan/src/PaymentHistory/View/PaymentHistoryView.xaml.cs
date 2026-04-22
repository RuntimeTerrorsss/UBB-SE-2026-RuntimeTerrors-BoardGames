using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using BookingBoardgamesILoveBan.Src.PaymentHistory.ViewModel;
using BookingBoardgamesILoveBan.Src.PaymentHistory.DTO;

namespace BookingBoardgamesILoveBan.Src.PaymentHistory.View
{
    public sealed partial class PaymentHistoryView : Page
    {
        public PaymentHistoryViewModel ViewModel { get; }

        public PaymentHistoryView()
        {
            this.InitializeComponent();
            this.ViewModel = new PaymentHistoryViewModel(App.ServicePayment);
        }

        public PaymentHistoryView(PaymentHistoryViewModel viewModel)
        {
            this.InitializeComponent();
            this.ViewModel = viewModel;
        }

        private void OnReceiptButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton && clickedButton.DataContext is PaymentDataTransferObject selectedPayment)
            {
                if (ViewModel.OpenReceiptCommand != null && ViewModel.OpenReceiptCommand.CanExecute(selectedPayment))
                {
                    ViewModel.OpenReceiptCommand.Execute(selectedPayment);
                }
            }
            // fallback for null
            else if (sender is Button fallbackButton && fallbackButton.Tag is PaymentDataTransferObject fallbackPayment)
            {
                if (ViewModel.OpenReceiptCommand != null && ViewModel.OpenReceiptCommand.CanExecute(fallbackPayment))
                {
                    ViewModel.OpenReceiptCommand.Execute(fallbackPayment);
                }
            }
        }

        private void OnBackToDashboardClicked(object sender, RoutedEventArgs e)
        {
            var currentParentElement = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(this);
            while (currentParentElement != null && !(currentParentElement is Frame))
            {
                currentParentElement = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(currentParentElement);
            }
            if (currentParentElement is Frame navigationFrame)
            {
                navigationFrame.Navigate(typeof(BookingBoardgamesILoveBan.Src.View.DashboardView));
            }
        }
    }
}
