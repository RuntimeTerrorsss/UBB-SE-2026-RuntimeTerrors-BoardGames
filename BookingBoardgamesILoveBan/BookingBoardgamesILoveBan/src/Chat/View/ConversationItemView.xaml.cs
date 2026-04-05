using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Diagnostics;
using System.IO;
using Windows.UI;

namespace BookingBoardgamesILoveBan.src.Chat.View
{
    public sealed partial class ConversationItemView : UserControl
    {
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

        private static void OnDisplayNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (ConversationItemView)d;
            view.DisplayNameText.Text = (string)e.NewValue;
            view.AvatarPicture.DisplayName = (string)e.NewValue;
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

        private static void OnAvatarUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (ConversationItemView)d;
            var url = (string)e.NewValue;
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    string fullPath = Path.Combine(AppContext.BaseDirectory, "Images", url);
                    view.AvatarPicture.ProfilePicture = new BitmapImage(new Uri(fullPath));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading avatar: {ex.Message}");
                }
            }
            else
            {
                view.AvatarPicture.ProfilePicture = null;
            }
        }

        public static readonly DependencyProperty AvatarColorProperty =
            DependencyProperty.Register(
                nameof(AvatarColor),
                typeof(string),
                typeof(ConversationItemView),
                new PropertyMetadata("#888888"));

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

        private static void OnMessagePreviewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (ConversationItemView)d;
            view.PreviewText.Text = (string)e.NewValue;
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

        private static void OnTimestampChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (ConversationItemView)d;
            view.TimestampText.Text = (string)e.NewValue;
        }

        public static readonly DependencyProperty UnreadCountProperty =
            DependencyProperty.Register(
                nameof(UnreadCount),
                typeof(int),
                typeof(ConversationItemView),
                new PropertyMetadata(0, OnUnreadCountChanged));

        public int UnreadCount
        {
            get => (int)GetValue(UnreadCountProperty);
            set => SetValue(UnreadCountProperty, value);
        }

        private static void OnUnreadCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (ConversationItemView)d;
            view.UnreadCountText.Text = e.NewValue.ToString();
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

        private static void OnHasUnreadChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (ConversationItemView)d;
            view.UnreadBadge.Visibility = (bool)e.NewValue
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public Visibility GetBadgeVisibility(int count)
        {
            return count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}