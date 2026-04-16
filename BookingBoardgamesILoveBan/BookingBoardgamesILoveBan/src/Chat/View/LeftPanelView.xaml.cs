using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using BookingBoardgamesILoveBan.Src.Chat.DTO;
using BookingBoardgamesILoveBan.Src.Chat.ViewModel;
using BookingBoardgamesILoveBan.Src.Enum;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;

namespace BookingBoardgamesILoveBan.Src.Chat.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LeftPanelView : UserControl
    {
        private LeftPanelViewModel viewModel;
        public LeftPanelViewModel ViewModel
        {
            get => viewModel;
            set
            {
                viewModel = value;
                viewModel.PropertyChanged += (s, e) => RefreshVisibility();
                RefreshVisibility(); // catch initial state
                // Bindings.Update();
            }
        }
        private void RefreshVisibility()
        {
            EmptyStatePanel.Visibility = ViewModel.IsEmptyStateVisible ? Visibility.Visible : Visibility.Collapsed;
            NoMatchesPanel.Visibility = ViewModel.IsNoMatchesVisible ? Visibility.Visible : Visibility.Collapsed;
            ConversationList.Visibility = ViewModel.IsListVisible ? Visibility.Visible : Visibility.Collapsed;
        }
        public LeftPanelView()
        {
            InitializeComponent();
        }
        private void ConversationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConversationList.SelectedItem is ConversationPreviewModel selected)
            {
                ViewModel.SelectedConversation = selected;
            }
        }
    }
}
