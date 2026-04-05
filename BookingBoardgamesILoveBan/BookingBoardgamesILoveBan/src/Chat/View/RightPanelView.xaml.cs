using BookingBoardgamesILoveBan.src.Chat.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BookingBoardgamesILoveBan.src.Chat.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    
    public sealed partial class RightPanelView : UserControl
    {
        public event EventHandler<(int userId, int requestId, int messageId)>? ProceedToPaymentRequested;
        public RightPanelView()
        {
            InitializeComponent();

            ActiveChat.ProceedToPaymentRequested += (s, e) => ProceedToPaymentRequested?.Invoke(s, e);
        }
        public ChatViewModel ChatViewModel
        {
            set
            {
                ActiveChat.ViewModel = value;
            }
        }
        public int CurrentUserId
        {
            set => ActiveChat.CurrentUserId = value;
        }

        private bool _isConversationSelected = false;
        public bool IsConversationSelected
        {
            get => _isConversationSelected;
            set
            {
                _isConversationSelected = value;
                WelcomePlaceholder.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
                ActiveChat.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

    }
}
