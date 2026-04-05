using BookingBoardgamesILoveBan.src.Chat.DTO;
using BookingBoardgamesILoveBan.src.Chat.Model;
using BookingBoardgamesILoveBan.src.Enum;
using BookingBoardgamesILoveBan.src.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Diagnostics;
using BookingBoardgamesILoveBan.src.PaymentCard.View;
using Microsoft.UI.Xaml.Controls;

namespace BookingBoardgamesILoveBan.src.Chat.ViewModel;

public class ChatViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    public event Action<MessageDTO> MessageSent;
    public event Action<int, int, bool, bool> BookingRequestUpdate;
    public event Action<int, int> CashAgreementAccept;

    public ChatViewModel(int currentUser)
    {
        CurrentUserId = currentUser;
    }

    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // Bound to the banner
    private string _displayName;
    public string DisplayName
    {
        get => _displayName;
        set { _displayName = value; OnPropertyChanged(); }
    }

    private string _initials;
    public string Initials
    {
        get => _initials;
        set { _initials = value; OnPropertyChanged(); }
    }

    private string _avatarUrl;
    public string AvatarUrl
    {
        get => _avatarUrl;
        set { _avatarUrl = value; OnPropertyChanged(nameof(AvatarUrl)); }
    }

    public int CurrentUserId { get; private set; }
    public int ConversationId { get; private set; }

    public ObservableCollection<MessageViewModel> Messages { get; } = new();


    /// <summary>
    /// Loads a conversation into the chat view model, replacing any existing messages. 
    /// Called when a conversation is selected from the list.
    /// </summary>
    /// <param name="conversation"></param>
    /// <param name="messages"></param>
    /// <param name="theirUnreadCount"></param>
    public void LoadConversation(ConversationPreviewModel conversation, List<MessageDTO> messages, int theirUnreadCount)
    {
        ConversationId = conversation.ConversationId;
        DisplayName = conversation.DisplayName;
        Initials = conversation.Initials;
        AvatarUrl = conversation.AvatarUrl;

        Messages.Clear();
        for (int i = 0; i < messages.Count; i++)
        {
            var msg = messages[i];
            var newMessageViewModel = new MessageViewModel(msg, CurrentUserId);
            if(i < messages.Count- theirUnreadCount) 
            {
                newMessageViewModel.IsRead = true;
            }
            Messages.Add(newMessageViewModel);
        }
    }

    /// <summary>
    /// Handles an incoming message for the active conversation. 
    /// This is called by the master view model when a new message received.
    /// </summary>
    /// <param name="message"></param>
    public void HandleIncomingMessage(MessageDTO message)
    {
        //FIX: UGLYYYY but this avoids weird duplicates when a message is received in the active conversation
        if (message.ConversationId != ConversationId) return;
        bool exists = Messages.Any(m =>
        m.Content == message.Content &&
        Math.Abs((m.SentAt - message.SentAt).TotalSeconds) < 1);

        if (exists) return;
        Messages.Add(new MessageViewModel(message, CurrentUserId));
    }

    private string _inputText = string.Empty;
    public string InputText
    {
        get => _inputText;
        set { _inputText = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanSend)); }
    }

    public bool CanSend => !string.IsNullOrWhiteSpace(InputText);

    /// <summary>
    /// Sends a message typed in the input box. 
    /// This creates a new MessageDTO and raises the MessageSent event, which is handled by the master view model to actually 
    /// send the message to the server. 
    /// </summary>
    public void SendMessage()
    {
        if (!CanSend) return;

        var dto = new MessageDTO(
            -1, 
            ConversationId,
            CurrentUserId,
            -1, 
            DateTime.Now,
            InputText,
            MessageType.Text,
            null,
            false,
            false,
            false, false,
            -1,
            -1
        );

        var vm = new MessageViewModel(dto, CurrentUserId);
        
        Messages.Add(vm);
        InputText = string.Empty;
        MessageSent.Invoke(dto); //notify master vm
    }

    /// <summary>
    /// Handles accepting or rejecting a booking request. 
    /// This is called by the booking request message when the accept/reject/cancel buttons are clicked.
    /// </summary>
    /// <param name="messageId"></param>
    /// <param name="accepted"></param>
    public void ResolveBookingRequest(int messageId, bool accepted)
    {
        var msg = Messages.FirstOrDefault(m => m.Id == messageId);
        if (msg == null) return;
        BookingRequestUpdate?.Invoke(messageId, msg.ConversationId, accepted, accepted? false :true);
    }

    /// <summary>
    /// Handles accepting a cash agreement. This is called by the cash agreement message when the accept button is clicked.
    /// </summary>
    /// <param name="messageId"></param>
    public void UpdateCashAgreement(int messageId)
    {
        var msg = Messages.FirstOrDefault(m => m.Id == messageId);
        if (msg == null) return;
        CashAgreementAccept?.Invoke(messageId, msg.ConversationId);
    }

    /// <summary>
    /// Handles proceeding to payment after a cash agreement is accepted. 
    /// This is called by the cash agreement message when the proceed to payment button is clicked.
    /// </summary>
    /// <param name="messageId"></param>
    public void ProceedToPayment(int messageId)
    {
        var msg = Messages.FirstOrDefault(m => m.Id == messageId);
    }


    /// <summary>
    /// Handles sending an image message.
    /// </summary>
    /// <param name="fileName"></param>
    public void SendImage(string fileName)
    {
        var dto = new MessageDTO(
            -1,
            ConversationId,
            CurrentUserId,
            -1,
            DateTime.Now,
            "",
            MessageType.Image,
            fileName,
            false,
            false,
            false, false,
            -1,
            -1
        );
        var vm = new MessageViewModel(dto, CurrentUserId);
        //Messages.Add(vm);
        InputText = string.Empty;
        MessageSent.Invoke(dto); 
    }

}