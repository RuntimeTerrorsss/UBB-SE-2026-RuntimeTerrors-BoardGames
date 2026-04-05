using BookingBoardgamesILoveBan.src.Chat.DTO;
using BookingBoardgamesILoveBan.src.Enum;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace BookingBoardgamesILoveBan.src.Chat.ViewModel;
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
    private bool _isResolved;
    public bool IsResolved
    {
        get => _isResolved;
        set { _isResolved = value; OnPropertyChanged(); }
    }
    public bool IsAccepted { get; set; }

    private int[]? _acceptedBy;
    public int[]? AcceptedBy
    {
        get => _acceptedBy;
        set { _acceptedBy = value; OnPropertyChanged(); OnPropertyChanged(nameof(BothAccepted)); }
    }
    public bool BothAccepted => _acceptedBy?.Length == 2;

    // Whether the other user has read this message — false by default, set to true when read receipt arrives
    private bool _isRead;
    public bool IsRead
    {
        get => _isRead;
        set { _isRead = value; OnPropertyChanged(); }
    }

    public MessageViewModel(MessageDTO message, int currentUserId)
    {
        Id = message.Id;
        ConversationId = message.ConversationId;
        SenderId = message.SenderId;
        Type = message.Type;
        Content = message.Content;
        IsMine = message.SenderId == currentUserId;
        SentAt = message.SentAt;
        ImageUrl = message.ImageUrl;
        RequestId = message.RequestId;
        IsAccepted = message.IsAccepted;
        _isResolved = message.IsResolved;
        _acceptedBy = new int[] { message.IsAcceptedByBuyer ? message.ReceiverId : 0, message.IsAcceptedBySeller ? message.SenderId : 0 };
        _isRead = false;
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