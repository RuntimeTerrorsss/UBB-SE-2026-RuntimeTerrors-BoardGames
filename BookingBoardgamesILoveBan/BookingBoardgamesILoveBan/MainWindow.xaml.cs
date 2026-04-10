using BookingBoardgamesILoveBan.Src.Chat.View;
using BookingBoardgamesILoveBan.Src.Interface.View;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace BookingBoardgamesILoveBan
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Root.Navigate(typeof(Src.View.DashboardView));

            var window1 = new Window();
            var frame1 = new Frame();
            window1.Content = frame1;
            frame1.Navigate(typeof(ChatPageView), 1);
            window1.Title = "Alice";
            window1.Activate();

            var window2 = new Window();
            var frame2 = new Frame();
            window2.Content = frame2;
            frame2.Navigate(typeof(ChatPageView), 2); // user id 2
            window2.Title = "Bob";
            window2.Activate();
        }
    }
}
