using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BookingBoardgamesILoveBan.Src.Chat.DTO;
using BookingBoardgamesILoveBan.Src.Enum;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
namespace BookingBoardgamesILoveBan.Src.Chat.ViewModel;
public class MessageViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // Immutable fields — init only
    public int Id { get; init; }
    public int ConversationId { get; init; }
    public int SenderId { get; init; }
    public MessageType Type { get; init; }
    public string Content { get; init; }
    public bool IsMine { get; init; }
    public DateTime SentAt { get; init; }
    public string ImageUrl { get; init; }
    public int RequestId { get; init; }
    public string TimestampString => SentAt.ToString("HH:mm");

    // Mutable fields — these change after the fact
    private bool isResolved;
    public bool IsResolved
    {
        get => isResolved;
        set
        {
            isResolved = value;
            OnPropertyChanged();
        }
    }
    public bool IsAccepted { get; set; }

    private int[] acceptedBy = Array.Empty<int>();
    public int[] AcceptedBy
    {
        get => acceptedBy;
        set
        {
            acceptedBy = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BothAccepted));
        }
    }
    public bool BothAccepted => acceptedBy?.Length == 2;

    // Whether the other user has read this message — false by default, set to true when read receipt arrives
    private bool isRead;
    public bool IsRead
    {
        get => isRead;
        set
        {
            isRead = value;
            OnPropertyChanged();
        }
    }

    public MessageViewModel(MessageDTO message, int currentUserId)
    {
        Id = message.id;
        ConversationId = message.conversationId;
        SenderId = message.senderId;
        Type = message.type;
        Content = message.content;
        IsMine = message.senderId == currentUserId;
        SentAt = message.sentAt;
        ImageUrl = message.imageUrl;
        RequestId = message.requestId;
        IsAccepted = message.isAccepted;
        isResolved = message.isResolved;
        acceptedBy = new int[] { message.isAcceptedByBuyer ? message.receiverId : 0, message.isAcceptedByBuyer ? message.receiverId : 0 };
        isRead = false;
    }

    public HorizontalAlignment IsMineToAlignment(bool isMine)
        => isMine ? HorizontalAlignment.Right : HorizontalAlignment.Left;
    public Brush IsMineToBackground(bool isMine)
        => isMine
            ? (Brush)Application.Current.Resources["AccentFillColorDefaultBrush"]
            : (Brush)Application.Current.Resources["LayerFillColorDefaultBrush"];
    public Brush IsMineToForeground(bool isMine)
        => isMine
            ? new SolidColorBrush(Microsoft.UI.Colors.White)
            : (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"];
    public CornerRadius IsMineToCornerRadius(bool isMine)
        => isMine ? new CornerRadius(12, 12, 2, 12) : new CornerRadius(12, 12, 12, 2);
    public Thickness IsMineToTheirsOnlyBorderThickness(bool isMine)
        => isMine ? new Thickness(0) : new Thickness(1);
}