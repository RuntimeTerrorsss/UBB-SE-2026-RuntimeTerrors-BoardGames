using BookingBoardgamesILoveBan.src.Chat.DTO;
using BookingBoardgamesILoveBan.src.Chat.ViewModel;
using BookingBoardgamesILoveBan.src.Enum;
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
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BookingBoardgamesILoveBan.src.Chat.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LeftPanelView : UserControl
    {
        private LeftPanelViewModel _viewModel;
        public LeftPanelViewModel ViewModel {
            get => _viewModel; 
            set
            {
                _viewModel = value;
                _viewModel.PropertyChanged += (s, e) => RefreshVisibility();
                RefreshVisibility(); //catch initial state
                //Bindings.Update();
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
