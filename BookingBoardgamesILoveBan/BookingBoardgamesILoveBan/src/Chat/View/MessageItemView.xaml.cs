using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BookingBoardgamesILoveBan.Src.Chat.DTO;
using BookingBoardgamesILoveBan.Src.Chat.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace BookingBoardgamesILoveBan.Src.Chat.View
{
    public sealed partial class MessageItemView : UserControl
    {
        private TextBlock statusIcon;
        public ChatViewModel ViewModel { get; set; }

        public event EventHandler<int>? AcceptRequested;
        public event EventHandler<int>? DeclineRequested;
        public event EventHandler<int>? CancelRequested;
        public event EventHandler<int>? AgreementAccepted;
        public event EventHandler<(int userId, int requestId, int messageId)>? ProceedToPaymentRequested;

        public MessageItemView()
        {
            this.InitializeComponent();
        }

        // Called externally (e.g. from ViewModel) when a read receipt arrives for this message
        public void MarkAsRead()
        {
            if (statusIcon != null)
            {
                statusIcon.Text = "\uE73E\uE73E";
            }
        }

        private TextBlock CreateStatusIcon(bool isRead)
        {
            statusIcon = new TextBlock
            {
                // Single check = sent, double check = read
                Text = isRead ? "\uE73E\uE73E" : "\uE73E",
                Margin = new Thickness(0, 1, 2, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"],
                FontSize = 11,
                Foreground = (Brush)Application.Current.Resources["TextFillColorTertiaryBrush"]
            };
            return statusIcon;
        }

        public void SetMessage(MessageViewModel message, int currentUserId)
        {
            switch (message.Type)
            {
                case Enum.MessageType.MessageSystem:
                    RenderSystemMessage(message);
                    break;

                case Enum.MessageType.MessageText:
                    RenderTextMessage(message, currentUserId);
                    break;

                case Enum.MessageType.MessageImage:
                    RenderImageMessage(message, currentUserId);
                    break;

                case Enum.MessageType.MessageRentalRequest:
                    RenderBookingRequest(message, currentUserId);
                    break;

                case Enum.MessageType.MessageCashAgreement:
                    RenderCashAgreement(message, currentUserId);
                    break;
            }
        }

        private void RenderSystemMessage(MessageViewModel message)
        {
            var textBlock = new TextBlock
            {
                Margin = new Thickness(0, 4, 0, 8),
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 11,
                Foreground = (Brush)Application.Current.Resources["TextFillColorTertiaryBrush"],
                TextWrapping = TextWrapping.Wrap
            };
            foreach (var part in Regex.Split(message.Content, @"(\S+\.pdf)"))
            {
                var path = part;
                if (path.EndsWith(".pdf"))
                {
                    var hl = new Hyperlink();
                    hl.Inlines.Add(new Run { Text = "File" });
                    hl.Click += (s, e) => System.Diagnostics.Process.Start("explorer.exe", path);
                    textBlock.Inlines.Add(hl);
                }
                else
                {
                    textBlock.Inlines.Add(new Run { Text = path });
                }
            }
            MessagePresenter.Content = textBlock;
        }

        private void RenderTextMessage(MessageViewModel message, int currentUserId)
        {
            bool isMine = message.SenderId == currentUserId;

            var stackPanel = new StackPanel
            {
                MaxWidth = 480,
                HorizontalAlignment = isMine ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Spacing = 2
            };

            var border = new Border
            {
                Padding = new Thickness(12, 8, 12, 8),
                CornerRadius = isMine ? new CornerRadius(12, 12, 2, 12) : new CornerRadius(12, 12, 12, 2)
            };

            var textBlock = new TextBlock { TextWrapping = TextWrapping.Wrap };
            foreach (var part in Regex.Split(message.Content, @"(https://\S+)"))
            {
                if (part.StartsWith("https://") && Uri.TryCreate(part, UriKind.Absolute, out var uri))
                {
                    var hl = new Hyperlink { NavigateUri = uri };
                    hl.Inlines.Add(new Run { Text = part });
                    textBlock.Inlines.Add(hl);
                }
                else
                {
                    textBlock.Inlines.Add(new Run { Text = part });
                }
            }

            if (isMine)
            {
                // My message - accent background
                border.Background = (Brush)Application.Current.Resources["AccentFillColorDefaultBrush"];
                textBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.White);
            }
            else
            {
                // Their message - light background with border
                border.Background = (Brush)Application.Current.Resources["LayerFillColorDefaultBrush"];
                border.BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"];
                border.BorderThickness = new Thickness(1);
                textBlock.Foreground = (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"];
            }

            border.Child = textBlock;
            stackPanel.Children.Add(border);

            // Show status icon for my messages; reflects IsRead from the viewmodel
            if (isMine)
            {
                stackPanel.Children.Add(CreateStatusIcon(message.IsRead));
            }
            MessagePresenter.Content = stackPanel;
        }

        private void RenderImageMessage(MessageViewModel message, int currentUserId)
        {
            bool isMine = message.SenderId == currentUserId;

            var stackPanel = new StackPanel
            {
                MaxWidth = 320,
                HorizontalAlignment = isMine ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Spacing = 2
            };

            var outerBorder = new Border
            {
                Padding = new Thickness(4),
                CornerRadius = isMine ? new CornerRadius(12, 12, 2, 12) : new CornerRadius(12, 12, 12, 2)
            };

            if (isMine)
            {
                outerBorder.Background = (Brush)Application.Current.Resources["AccentFillColorDefaultBrush"];
            }
            else
            {
                outerBorder.Background = (Brush)Application.Current.Resources["LayerFillColorDefaultBrush"];
                outerBorder.BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"];
                outerBorder.BorderThickness = new Thickness(1);
            }

            var imageBorder = new Border
            {
                Width = 280,
                Height = 180,
                Background = (Brush)Application.Current.Resources["SubtleFillColorSecondaryBrush"],
                CornerRadius = new CornerRadius(8)
            };

            if (message.ImageUrl != null && message.ImageUrl.Length > 0)
            {
                var chatImage = new Image
                {
                    Stretch = Stretch.UniformToFill,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                try
                {
                    string fullPath = Path.Combine(AppContext.BaseDirectory, "Images", message.ImageUrl);
                    var bitmap = new BitmapImage(new Uri(fullPath));

                    chatImage.Source = bitmap;
                    imageBorder.Child = chatImage;
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading image: {exception.Message}");
                    imageBorder.Child = CreateImagePlaceholder();
                }
            }
            else
            {
                // No image data, show placeholder
                imageBorder.Child = CreateImagePlaceholder();
            }

            outerBorder.Child = imageBorder;
            stackPanel.Children.Add(outerBorder);

            // Show status icon for my messages; reflects IsRead from the viewmodel
            if (isMine)
            {
                stackPanel.Children.Add(CreateStatusIcon(message.IsRead));
            }

            MessagePresenter.Content = stackPanel;
        }

        private StackPanel CreateImagePlaceholder()
        {
            var placeholder = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 6
            };

            var icon = new FontIcon
            {
                Glyph = "\uEB9F",
                FontSize = 32,
                Foreground = (Brush)Application.Current.Resources["TextFillColorTertiaryBrush"]
            };

            var text = new TextBlock
            {
                Text = "Image",
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 12,
                Foreground = (Brush)Application.Current.Resources["TextFillColorTertiaryBrush"]
            };

            placeholder.Children.Add(icon);
            placeholder.Children.Add(text);

            return placeholder;
        }

        private void RenderBookingRequest(MessageViewModel message, int currentUserId)
        {
            var border = new Border
            {
                MaxWidth = 400,
                Margin = new Thickness(0, 10, 0, 10),
                Padding = new Thickness(16, 12, 16, 12),
                HorizontalAlignment = HorizontalAlignment.Center,
                Background = (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"],
                BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10)
            };

            var stackPanel = new StackPanel { Spacing = 10 };

            var title = new TextBlock
            {
                Text = "Rental Request",
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 13,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"]
            };

            var contentText = new TextBlock
            {
                Text = message.Content,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 12,
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
                TextWrapping = TextWrapping.Wrap
            };

            stackPanel.Children.Add(title);
            stackPanel.Children.Add(contentText);

            // Show buttons only if not resolved | and isnt mine
            if ((!message.IsResolved && !message.IsAccepted) && message.SenderId != currentUserId)
            {
                var buttonPanel = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Orientation = Orientation.Horizontal,
                    Spacing = 8
                };

                var acceptButton = new Button
                {
                    Content = "Accept",
                    MinWidth = 90,
                    Style = (Style)Application.Current.Resources["AccentButtonStyle"]
                };

                var declineButton = new Button
                {
                    Content = "Decline",
                    MinWidth = 90
                };
                acceptButton.Click += (s, e) => AcceptRequested?.Invoke(this, message.Id);
                declineButton.Click += (s, e) => DeclineRequested?.Invoke(this, message.Id);
                buttonPanel.Children.Add(acceptButton);
                buttonPanel.Children.Add(declineButton);
                stackPanel.Children.Add(buttonPanel);
            }
            else if (message.SenderId == currentUserId && !message.IsResolved)
            {
                if (!message.IsAccepted)
                {
                    var buttonPanel = new StackPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Orientation = Orientation.Horizontal,
                        Spacing = 8
                    };

                    var cancelButton = new Button
                    {
                        Content = "Cancel",
                        MinWidth = 90
                    };

                    cancelButton.Click += (s, e) => CancelRequested?.Invoke(this, message.Id);

                    buttonPanel.Children.Add(cancelButton);
                    stackPanel.Children.Add(buttonPanel);
                }
                else
                {
                    var buttonPanel = new StackPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Orientation = Orientation.Horizontal,
                        Spacing = 8
                    };

                    var cancelButton = new Button
                    {
                        Content = "Proceed to payment",
                        MinWidth = 90
                    };
                    cancelButton.Click += (s, e) => ProceedToPaymentRequested?.Invoke(this, (currentUserId, message.RequestId, message.Id));

                    buttonPanel.Children.Add(cancelButton);
                    stackPanel.Children.Add(buttonPanel);
                }
            }

                border.Child = stackPanel;
            MessagePresenter.Content = border;
        }

        private void RenderCashAgreement(MessageViewModel message, int currentUserId)
        {
            var border = new Border
            {
                MaxWidth = 380,
                Margin = new Thickness(0, 4, 0, 4),
                Padding = new Thickness(14, 10, 14, 10),
                HorizontalAlignment = HorizontalAlignment.Center,
                Background = (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"],
                BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10)
            };

            var stackPanel = new StackPanel { Spacing = 8 };

            var title = new TextBlock
            {
                Text = "Cash Transaction Agreement",
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 13,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"]
            };

            var contentText = new TextBlock
            {
                Text = message.Content,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 12,
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
                TextWrapping = TextWrapping.Wrap
            };

            stackPanel.Children.Add(title);
            stackPanel.Children.Add(contentText);

            bool currentUserAccepted = message.AcceptedBy != null && message.AcceptedBy.Contains(currentUserId);
            bool isSeller = message.SenderId == currentUserId;

            if (!currentUserAccepted)
            {
                var agreeButton = new Button
                {
                    Content = isSeller ? "I received the cash payment" : "I received the boardgame",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Style = (Style)Application.Current.Resources["AccentButtonStyle"]
                };
                agreeButton.Click += (s, e) => AgreementAccepted(this, message.Id);
                stackPanel.Children.Add(agreeButton);
            }

            border.Child = stackPanel;
            MessagePresenter.Content = border;
        }
    }
}