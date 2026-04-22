using System;
using System.Diagnostics;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.UI;

namespace BookingBoardgamesILoveBan.Src.Chat.View
{
    public sealed partial class ConversationItemView : UserControl
    {
        private const string DefaultAvatarColorHexCode = "#888888";
        private const int EmptyUnreadMessagesCount = 0;

        public ConversationItemView()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register(
                nameof(DisplayName),
                typeof(string),
                typeof(ConversationItemView),
                new PropertyMetadata(string.Empty, OnDisplayNameChanged));

        public string DisplayName
        {
            get => (string)GetValue(DisplayNameProperty);
            set => SetValue(DisplayNameProperty, value);
        }

        private static void OnDisplayNameChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArguments)
        {
            var conversationItemView = (ConversationItemView)dependencyObject;
            conversationItemView.DisplayNameText.Text = (string)eventArguments.NewValue;
            conversationItemView.AvatarPicture.DisplayName = (string)eventArguments.NewValue;
        }

        public static readonly DependencyProperty AvatarUrlProperty =
            DependencyProperty.Register(
                nameof(AvatarUrl),
                typeof(string),
                typeof(ConversationItemView),
                new PropertyMetadata(string.Empty, OnAvatarUrlChanged));

        public string AvatarUrl
        {
            get => (string)GetValue(AvatarUrlProperty);
            set => SetValue(AvatarUrlProperty, value);
        }

        private static void OnAvatarUrlChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArguments)
        {
            var conversationItemView = (ConversationItemView)dependencyObject;
            var imageResourceLocator = (string)eventArguments.NewValue;

            if (!string.IsNullOrEmpty(imageResourceLocator))
            {
                try
                {
                    string fullImagePath = Path.Combine(AppContext.BaseDirectory, "Images", imageResourceLocator);
                    conversationItemView.AvatarPicture.ProfilePicture = new BitmapImage(new Uri(fullImagePath));
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"Error loading avatar: {exception.Message}");
                }
            }
            else
            {
                conversationItemView.AvatarPicture.ProfilePicture = null;
            }
        }

        public static readonly DependencyProperty AvatarColorProperty =
            DependencyProperty.Register(
                nameof(AvatarColor),
                typeof(string),
                typeof(ConversationItemView),
                new PropertyMetadata(DefaultAvatarColorHexCode));

        public string AvatarColor
        {
            get => (string)GetValue(AvatarColorProperty);
            set => SetValue(AvatarColorProperty, value);
        }

        public static readonly DependencyProperty MessagePreviewProperty =
            DependencyProperty.Register(
                nameof(MessagePreview),
                typeof(string),
                typeof(ConversationItemView),
                new PropertyMetadata(string.Empty, OnMessagePreviewChanged));

        public string MessagePreview
        {
            get => (string)GetValue(MessagePreviewProperty);
            set => SetValue(MessagePreviewProperty, value);
        }

        private static void OnMessagePreviewChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArguments)
        {
            var conversationItemView = (ConversationItemView)dependencyObject;
            conversationItemView.PreviewText.Text = (string)eventArguments.NewValue;
        }

        public static readonly DependencyProperty TimestampProperty =
            DependencyProperty.Register(
                nameof(Timestamp),
                typeof(string),
                typeof(ConversationItemView),
                new PropertyMetadata(string.Empty, OnTimestampChanged));

        public string Timestamp
        {
            get => (string)GetValue(TimestampProperty);
            set => SetValue(TimestampProperty, value);
        }

        private static void OnTimestampChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArguments)
        {
            var conversationItemView = (ConversationItemView)dependencyObject;
            conversationItemView.TimestampText.Text = (string)eventArguments.NewValue;
        }

        public static readonly DependencyProperty UnreadCountProperty =
            DependencyProperty.Register(
                nameof(UnreadCount),
                typeof(int),
                typeof(ConversationItemView),
                new PropertyMetadata(EmptyUnreadMessagesCount, OnUnreadCountChanged));

        public int UnreadCount
        {
            get => (int)GetValue(UnreadCountProperty);
            set => SetValue(UnreadCountProperty, value);
        }

        private static void OnUnreadCountChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArguments)
        {
            var conversationItemView = (ConversationItemView)dependencyObject;
            conversationItemView.UnreadCountText.Text = eventArguments.NewValue.ToString();
        }

        public static readonly DependencyProperty HasUnreadProperty =
            DependencyProperty.Register(
                nameof(HasUnread),
                typeof(bool),
                typeof(ConversationItemView),
                new PropertyMetadata(false, OnHasUnreadChanged));

        public bool HasUnread
        {
            get => (bool)GetValue(HasUnreadProperty);
            set => SetValue(HasUnreadProperty, value);
        }

        private static void OnHasUnreadChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArguments)
        {
            var conversationItemView = (ConversationItemView)dependencyObject;
            conversationItemView.UnreadBadge.Visibility = (bool)eventArguments.NewValue
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public Visibility GetBadgeVisibility(int currentUnreadCount)
        {
            return currentUnreadCount > EmptyUnreadMessagesCount ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}