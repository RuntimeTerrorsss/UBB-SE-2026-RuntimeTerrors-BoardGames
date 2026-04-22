using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using BookingBoardgamesILoveBan.Src.Chat.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace BookingBoardgamesILoveBan.Src.Chat.View
{
    public sealed partial class RightPanelView : UserControl
    {
        public event EventHandler<(int userId, int requestId, int messageId)>? ProceedToPaymentRequested;

        public RightPanelView()
        {
            InitializeComponent();

            ActiveChat.ProceedToPaymentRequested += (sender, paymentArguments) => ProceedToPaymentRequested?.Invoke(sender, paymentArguments);
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

        private bool isConversationSelected = false;

        public bool IsConversationSelected
        {
            get => isConversationSelected;
            set
            {
                isConversationSelected = value;
                WelcomePlaceholder.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
                ActiveChat.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}