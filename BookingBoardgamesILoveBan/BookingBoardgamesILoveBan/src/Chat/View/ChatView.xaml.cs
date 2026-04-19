using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using BookingBoardgamesILoveBan.Src.Chat.DTO;
using BookingBoardgamesILoveBan.Src.Chat.ViewModel;
using BookingBoardgamesILoveBan.Src.Enum;
using BookingBoardgamesILoveBan.Src.PaymentCard.View;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace BookingBoardgamesILoveBan.Src.Chat.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChatView : UserControl
    {
        public event EventHandler<(int userId, int requestId, int messageId)>? ProceedToPaymentRequested;

        private ChatViewModel chatViewModel;

        // Holds the file name of a pasted image that is staged but not yet sent
        private string? pendingImageFileName = null;

        public ChatViewModel ViewModel
        {
            get => chatViewModel;
            set
            {
                // Unsubscribe from old viewmodel
                if (chatViewModel != null)
                {
                    chatViewModel.Messages.CollectionChanged -= OnMessagesChanged;
                    chatViewModel.PropertyChanged -= OnViewModelPropertyChanged;
                }

                chatViewModel = value;

                if (chatViewModel != null)
                {
                    chatViewModel.Messages.CollectionChanged += OnMessagesChanged;
                    chatViewModel.PropertyChanged += OnViewModelPropertyChanged;

                    BannerDisplayName.Text = chatViewModel.DisplayName;
                    SetupAvatar();

                    RefreshMessages();
                }
            }
        }

        public int CurrentUserId { get; set; }

        public ChatView()
        {
            InitializeComponent();
        }

        private void RefreshMessages()
        {
            MessagesPanel.Children.Clear();
            foreach (var vm in chatViewModel.Messages)
            {
                var itemView = new MessageItemView();
                itemView.SetMessage(viewModel, CurrentUserId);

                itemView.AcceptRequested += OnAcceptRequested;
                itemView.DeclineRequested += OnDeclineRequested;
                itemView.CancelRequested += OnCancelRequested;
                itemView.AgreementAccepted += OnAcceptCashAgreement;
                itemView.ProceedToPaymentRequested += (s, e) => ProceedToPaymentRequested.Invoke(s, e);

                MessagesPanel.Children.Add(itemView);
            }

            ScrollToBottom();
        }

        private void OnMessagesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs eventArguments)
        {
            if (eventArguments.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (MessageViewModel viewModel in eventArguments.NewItems)
                {
                    var itemView = new MessageItemView();
                    itemView.SetMessage(viewModel, CurrentUserId);

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
                RefreshMessages(); // for Clear() and other operations
            }
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs eventArguments)
        {
            if (eventArguments.PropertyName == nameof(ChatViewModel.DisplayName))
            {
                BannerDisplayName.Text = chatViewModel.DisplayName;
            }
            else if (eventArguments.PropertyName == nameof(ChatViewModel.InputText))
            {
                MessageInput.Text = chatViewModel.InputText;
            }
            else if (eventArguments.PropertyName == nameof(ChatViewModel.AvatarUrl))
            {
                SetupAvatar();
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs eventArguments)
        {
            if (pendingImageFileName != null)
            {
                ViewModel?.SendImage(pendingImageFileName);
                ClearPendingImage();
            }
            else if (!string.IsNullOrWhiteSpace(ViewModel?.InputText))
            {
                ViewModel?.SendMessage();
            }
        }

        private void SetupAvatar()
        {
            AvatarPicture.DisplayName = chatViewModel.DisplayName;
            if (!string.IsNullOrEmpty(chatViewModel.AvatarUrl))
            {
                try
                {
                    string fullPath = Path.Combine(AppContext.BaseDirectory, "Images", chatViewModel.AvatarUrl);
                    AvatarPicture.ProfilePicture = new BitmapImage(new Uri(fullPath));
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"Error loading avatar: {exception.Message}");
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

        private async void MessageInput_Paste(object sender, TextControlPasteEventArgs eventArguments)
        {
            var clipboard = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
            var formats = clipboard.AvailableFormats;
            System.Diagnostics.Debug.WriteLine("Clipboard formats: " + string.Join(", ", formats));

            if (clipboard.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.Bitmap))
            {
                eventArguments.Handled = true; // prevent pasting raw text

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

                pendingImageFileName = fileName;
            }
        }

        private void ClearPendingImage()
        {
            pendingImageFileName = null;
            ImagePreview.Source = null;
            ImagePreviewPanel.Visibility = Visibility.Collapsed;
        }

        private void RemoveImageButton_Click(object sender, RoutedEventArgs eventArguments)
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