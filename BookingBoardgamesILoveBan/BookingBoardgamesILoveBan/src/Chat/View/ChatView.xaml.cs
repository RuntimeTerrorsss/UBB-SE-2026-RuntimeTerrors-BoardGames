using BookingBoardgamesILoveBan.src.PaymentCard.View;
using BookingBoardgamesILoveBan.src.Chat.DTO;
using BookingBoardgamesILoveBan.src.Chat.ViewModel;
using BookingBoardgamesILoveBan.src.Enum;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    public sealed partial class ChatView : UserControl
    {
        public event EventHandler<(int userId, int requestId, int messageId)>? ProceedToPaymentRequested;

        private ChatViewModel _viewModel;

        // Holds the file name of a pasted image that is staged but not yet sent
        private string? _pendingImageFileName = null;

        public ChatViewModel ViewModel
        {
            get => _viewModel;
            set
            {
                // Unsubscribe from old viewmodel
                if (_viewModel != null)
                {
                    _viewModel.Messages.CollectionChanged -= OnMessagesChanged;
                    _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                }

                _viewModel = value;

                if (_viewModel != null)
                {
                    _viewModel.Messages.CollectionChanged += OnMessagesChanged;
                    _viewModel.PropertyChanged += OnViewModelPropertyChanged;

                    BannerDisplayName.Text = _viewModel.DisplayName;
                    SetupAvatar();

                    RebuildMessages();
                }
            }
        }

        public int CurrentUserId { get; set; }

        public ChatView()
        {
            InitializeComponent();
        }

        private void RebuildMessages()
        {
            MessagesPanel.Children.Clear();
            foreach (var vm in _viewModel.Messages)
            {
                var itemView = new MessageItemView();
                itemView.SetMessage(vm, CurrentUserId);

                itemView.AcceptRequested += OnAcceptRequested;
                itemView.DeclineRequested += OnDeclineRequested;
                itemView.CancelRequested += OnCancelRequested;
                itemView.AgreementAccepted += OnAcceptCashAgreement;
                itemView.ProceedToPaymentRequested += (s, e) => ProceedToPaymentRequested.Invoke(s, e);

                MessagesPanel.Children.Add(itemView);
            }

            ScrollToBottom();
        }

        private void OnMessagesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (MessageViewModel vm in e.NewItems)
                {
                    var itemView = new MessageItemView();
                    itemView.SetMessage(vm, CurrentUserId);

                    itemView.AcceptRequested += OnAcceptRequested;
                    itemView.DeclineRequested += OnDeclineRequested;
                    itemView.CancelRequested += OnCancelRequested;
                    itemView.AgreementAccepted += OnAcceptCashAgreement;
                    itemView.ProceedToPaymentRequested += (s, e) => ProceedToPaymentRequested?.Invoke(s, e);

                    MessagesPanel.Children.Add(itemView);
                }
            }
            else
            {
                RebuildMessages(); // for Clear() and other operations
            }
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ChatViewModel.DisplayName))
                BannerDisplayName.Text = _viewModel.DisplayName;
            else if (e.PropertyName == nameof(ChatViewModel.InputText))
                MessageInput.Text = _viewModel.InputText;
            else if (e.PropertyName == nameof(ChatViewModel.AvatarUrl))
                SetupAvatar();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (_pendingImageFileName != null)
            {
                ViewModel?.SendImage(_pendingImageFileName);
                ClearPendingImage();
            }

            else if (!string.IsNullOrWhiteSpace(ViewModel?.InputText))
                ViewModel?.SendMessage();
        }

        private void SetupAvatar()
        {
            AvatarPicture.DisplayName = _viewModel.DisplayName;
            if (!string.IsNullOrEmpty(_viewModel.AvatarUrl))
            {
                try
                {
                    string fullPath = Path.Combine(AppContext.BaseDirectory, "Images", _viewModel.AvatarUrl);
                    AvatarPicture.ProfilePicture = new BitmapImage(new Uri(fullPath));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading avatar: {ex.Message}");
                    AvatarPicture.ProfilePicture = null;
                }
            }
            else
            {
                AvatarPicture.ProfilePicture = null;
            }
        }

        private void ScrollToBottom()
        {
            // Use the Dispatcher to wait until the UI has finished drawing the new message
            DispatcherQueue.TryEnqueue(() =>
            {
                ScrollContainer.UpdateLayout();
                ScrollContainer.ChangeView(null, ScrollContainer.ExtentHeight, null);
            });
        }

        private async void MessageInput_Paste(object sender, TextControlPasteEventArgs e)
        {
            var clipboard = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
            var formats = clipboard.AvailableFormats;
            System.Diagnostics.Debug.WriteLine("Clipboard formats: " + string.Join(", ", formats));

            if (clipboard.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.Bitmap))
            {
                e.Handled = true; // prevent pasting raw text

                var streamRef = await clipboard.GetBitmapAsync();
                var stream = await streamRef.OpenReadAsync();

                var bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(stream);
                ImagePreview.Source = bitmapImage;
                ImagePreviewPanel.Visibility = Visibility.Visible;

                stream.Seek(0); // rewind before writing

                string fileName = $"{Guid.NewGuid()}.jpg";
                string fullPath = Path.Combine(AppContext.BaseDirectory, "Images", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                using var fileStream = File.Create(fullPath);
                await stream.AsStreamForRead().CopyToAsync(fileStream);

                _pendingImageFileName = fileName;
            }
        }

        private void ClearPendingImage()
        {
            _pendingImageFileName = null;
            ImagePreview.Source = null;
            ImagePreviewPanel.Visibility = Visibility.Collapsed;
        }

        private void RemoveImageButton_Click(object sender, RoutedEventArgs e)
        {
            ClearPendingImage();
        }

        private void OnAcceptRequested(object? sender, int messageId)
        {
            ViewModel?.ResolveBookingRequest(messageId, true);
        }

        private void OnDeclineRequested(object? sender, int messageId)
        {
            ViewModel?.ResolveBookingRequest(messageId, false);
        }

        private void OnCancelRequested(object? sender, int messageId)
        {
            ViewModel?.ResolveBookingRequest(messageId, false);
        }

        private void OnAcceptCashAgreement(object? sender, int messageId)
        {
            ViewModel?.UpdateCashAgreement(messageId);
        }
    }
}