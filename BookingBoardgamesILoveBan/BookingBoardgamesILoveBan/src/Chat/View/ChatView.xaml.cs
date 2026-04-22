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
    public sealed partial class ChatView : UserControl
    {
        public event EventHandler<(int userId, int requestId, int messageId)>? ProceedToPaymentRequested;

        private ChatViewModel chatViewModel;
        private string? pendingImageFileName = null;

        public ChatViewModel ViewModel
        {
            get => chatViewModel;
            set
            {
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
            foreach (var messageViewModel in chatViewModel.Messages)
            {
                var itemView = new MessageItemView();
                itemView.SetMessage(messageViewModel, CurrentUserId);

                itemView.AcceptRequested += OnAcceptRequested;
                itemView.DeclineRequested += OnDeclineRequested;
                itemView.CancelRequested += OnCancelRequested;
                itemView.AgreementAccepted += OnAcceptCashAgreement;
                itemView.ProceedToPaymentRequested += (sender, paymentArguments) => ProceedToPaymentRequested?.Invoke(sender, paymentArguments);

                MessagesPanel.Children.Add(itemView);
            }

            ScrollToBottom();
        }

        private void OnMessagesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs collectionChangedEventArgs)
        {
            if (collectionChangedEventArgs.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (MessageViewModel addedMessageViewModel in collectionChangedEventArgs.NewItems)
                {
                    var itemView = new MessageItemView();
                    itemView.SetMessage(addedMessageViewModel, CurrentUserId);

                    itemView.AcceptRequested += OnAcceptRequested;
                    itemView.DeclineRequested += OnDeclineRequested;
                    itemView.CancelRequested += OnCancelRequested;
                    itemView.AgreementAccepted += OnAcceptCashAgreement;
                    itemView.ProceedToPaymentRequested += (eventSender, paymentArguments) => ProceedToPaymentRequested?.Invoke(eventSender, paymentArguments);

                    MessagesPanel.Children.Add(itemView);
                }
            }
            else
            {
                RefreshMessages();
            }
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(ChatViewModel.DisplayName))
            {
                BannerDisplayName.Text = chatViewModel.DisplayName;
            }
            else if (propertyChangedEventArgs.PropertyName == nameof(ChatViewModel.InputText))
            {
                MessageInput.Text = chatViewModel.InputText;
            }
            else if (propertyChangedEventArgs.PropertyName == nameof(ChatViewModel.AvatarUrl))
            {
                SetupAvatar();
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs routedEventArgs)
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
            DispatcherQueue.TryEnqueue(() =>
            {
                ScrollContainer.UpdateLayout();
                ScrollContainer.ChangeView(null, ScrollContainer.ExtentHeight, null);
            });
        }

        private async void MessageInput_Paste(object sender, TextControlPasteEventArgs pasteEventArgs)
        {
            var clipboardData = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
            var clipboardFormats = clipboardData.AvailableFormats;
            System.Diagnostics.Debug.WriteLine("Clipboard formats: " + string.Join(", ", clipboardFormats));

            if (clipboardData.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.Bitmap))
            {
                pasteEventArgs.Handled = true;

                var bitmapStreamReference = await clipboardData.GetBitmapAsync();
                var rawStream = await bitmapStreamReference.OpenReadAsync();

                var bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(rawStream);
                ImagePreview.Source = bitmapImage;
                ImagePreviewPanel.Visibility = Visibility.Visible;

                rawStream.Seek(0);

                string generatedFileName = $"{Guid.NewGuid()}.jpg";
                string fullImagePath = Path.Combine(AppContext.BaseDirectory, "Images", generatedFileName);
                Directory.CreateDirectory(Path.GetDirectoryName(fullImagePath));

                using var fileStream = File.Create(fullImagePath);
                await rawStream.AsStreamForRead().CopyToAsync(fileStream);

                pendingImageFileName = generatedFileName;
            }
        }

        private void ClearPendingImage()
        {
            pendingImageFileName = null;
            ImagePreview.Source = null;
            ImagePreviewPanel.Visibility = Visibility.Collapsed;
        }

        private void RemoveImageButton_Click(object sender, RoutedEventArgs routedEventArgs)
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