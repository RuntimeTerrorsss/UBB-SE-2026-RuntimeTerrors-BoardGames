using BookingBoardgamesILoveBan.src.Chat.ViewModel;
using BookingBoardgamesILoveBan.src.Interface.View;
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
using Windows.Security.Authentication.OnlineId;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BookingBoardgamesILoveBan.src.Chat.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChatPageView : Page
    {
        private ChatPageViewModel _vm;
        private int currentUserId;
        
        public ChatPageView()
        {
            InitializeComponent();
        }
        public void Initialize(int currentUserId)
        {
            _vm = new ChatPageViewModel(currentUserId);
            LeftPanel.ViewModel = _vm.LeftPanel;
            RightPanel.ChatViewModel = _vm.Chat;
            RightPanel.CurrentUserId = currentUserId;
            RightPanel.ProceedToPaymentRequested += OnProceedToPayment;

            _vm.LeftPanel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != nameof(LeftPanelViewModel.SelectedConversation)) return;
                RightPanel.IsConversationSelected = _vm.LeftPanel.SelectedConversation != null;
            };
        }

        private void OnProceedToPayment(object sender, (int userId, int requestId, int messageId) args)
        {
            var deliveryWindow = new Window();
            var deliveryFrame = new Frame();
            deliveryWindow.Content = deliveryFrame;
            deliveryFrame.Navigate(typeof(DeliveryView), (args.userId, args.requestId, args.messageId, _vm.ConversationService, deliveryWindow));
            deliveryWindow.Activate();
            //this.Frame?.Navigate(typeof(DeliveryView), args);
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            currentUserId = (int)e.Parameter;

            Initialize(currentUserId);
        }
    }
}
