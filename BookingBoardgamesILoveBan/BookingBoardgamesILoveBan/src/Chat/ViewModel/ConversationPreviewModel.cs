namespace BookingBoardgamesILoveBan.src.Chat.ViewModel;

using BookingBoardgamesILoveBan.src.Chat.DTO;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

public class ConversationPreviewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public int ConversationId { get; init; }
    public string DisplayName { get; init; }
    public string Initials { get; init; }
    public string AvatarUrl { get; init; }

    private string _lastMessageText;
    public string LastMessageText
    {
        get => _lastMessageText;
        set { _lastMessageText = value; OnPropertyChanged(); }
    }

    private DateTime _timestamp;
    public DateTime Timestamp
    {
        get => _timestamp;
        set { _timestamp = value; OnPropertyChanged(); OnPropertyChanged(nameof(TimestampString)); }
    }

    private int _unreadCount;
    public int UnreadCount
    {
        get => _unreadCount;
        set { _unreadCount = value; OnPropertyChanged(); }
    }

    public string TimestampString => _timestamp.ToString("HH:mm");


    public ConversationPreviewModel(int conversationId, string displayName, string initials,
        string lastMessageText, DateTime timestamp, int unreadCount, string avatarUrl)
    {
        ConversationId = conversationId;
        DisplayName = displayName;
        Initials = initials;
        AvatarUrl = avatarUrl;
        _lastMessageText = lastMessageText;
        _timestamp = timestamp;
        _unreadCount = unreadCount;
    }

    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}