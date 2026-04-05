using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using BookingBoardgamesILoveBan.src.PaymentHistory.ViewModel;
using BookingBoardgamesILoveBan.src.PaymentHistory.DTO;

namespace BookingBoardgamesILoveBan.src.PaymentHistory.View
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
            if (sender is Button btn && btn.DataContext is PaymentDto dto)
            {
                if (ViewModel.OpenReceiptCommand != null && ViewModel.OpenReceiptCommand.CanExecute(dto))
                {
                    ViewModel.OpenReceiptCommand.Execute(dto);
                }
            }
            // fallback for null
            else if (sender is Button btnFallback && btnFallback.Tag is PaymentDto dtoTag)
            {
                if (ViewModel.OpenReceiptCommand != null && ViewModel.OpenReceiptCommand.CanExecute(dtoTag))
                {
                    ViewModel.OpenReceiptCommand.Execute(dtoTag);
                }
            }
        }

        private void OnBackToDashboardClicked(object sender, RoutedEventArgs e)
        {
            var parent = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(this);
            while (parent != null && !(parent is Frame))
            {
                parent = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(parent);
            }
            if (parent is Frame frame)
            {
                frame.Navigate(typeof(BookingBoardgamesILoveBan.src.View.DashboardView));
            }
        }
    }
}
