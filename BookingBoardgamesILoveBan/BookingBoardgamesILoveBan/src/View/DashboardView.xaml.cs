using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BookingBoardgamesILoveBan.Src.PaymentHistory.View;
using BookingBoardgamesILoveBan.Src.Chat.View;
using BookingBoardgamesILoveBan.Src.Interface.View;

namespace BookingBoardgamesILoveBan.Src.View
{
    public sealed partial class DashboardView : Page
    {
        public DashboardView()
        {
            this.InitializeComponent();
        }

        private void PaymentHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame?.Navigate(typeof(PaymentHistoryView));
        }

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            App.ConversationRepository.CreateConversation(3, 1);
            var window1 = new Window();
            var frame1 = new Frame();
            window1.Content = frame1;
            window1.Title = "Carol";
            frame1.Navigate(typeof(ChatPageView), 3);
            window1.Activate();
        }
        private void SeeEmptyChat_Click(object sender, RoutedEventArgs e)
        {
            var window1 = new Window();
            var frame1 = new Frame();
            window1.Content = frame1;
            frame1.Navigate(typeof(ChatPageView), App.NO_CHATS_USER);
            window1.Activate();
        }
    }
}
