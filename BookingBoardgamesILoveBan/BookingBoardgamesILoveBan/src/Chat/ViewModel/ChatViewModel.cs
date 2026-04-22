using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using BookingBoardgamesILoveBan.Src.Chat.DTO;
using BookingBoardgamesILoveBan.Src.Chat.Model;
using BookingBoardgamesILoveBan.Src.Enum;
using BookingBoardgamesILoveBan.Src.Model;
using BookingBoardgamesILoveBan.Src.PaymentCard.View;
using Microsoft.UI.Xaml.Controls;

namespace BookingBoardgamesILoveBan.Src.Chat.ViewModel;

public class ChatViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    public event Action<MessageDataTransferObject> MessageSent;
    public event Action<int, int, bool, bool> BookingRequestUpdate;
    public event Action<int, int> CashAgreementAccept;

    public ChatViewModel(int currentUser)
    {
        CurrentUserId = currentUser;
    }

    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private string displayName;
    public string DisplayName
    {
        get => displayName;
        set
        {
            displayName = value;
            OnPropertyChanged();
        }
    }

    private string initials;
    public string Initials
    {
        get => initials;
        set
        {
            initials = value;
            OnPropertyChanged();
        }
    }

    private string avatarUrl;
    public string AvatarUrl
    {
        get => avatarUrl;
        set
        {
            avatarUrl = value;
            OnPropertyChanged(nameof(AvatarUrl));
        }
    }

    public int CurrentUserId { get; private set; }
    public int ConversationId { get; private set; }

    public ObservableCollection<MessageViewModel> Messages { get; } = new ();

    public void LoadConversation(ConversationPreviewModel conversation, List<MessageDataTransferObject> messages, int theirUnreadCount)
    {
        ConversationId = conversation.ConversationId;
        DisplayName = conversation.DisplayName;
        Initials = conversation.Initials;
        AvatarUrl = conversation.AvatarUrl;

        Messages.Clear();
        for (int i = 0; i < messages.Count; i++)
        {
            var currentMessage = messages[i];
            var newMessageViewModel = new MessageViewModel(currentMessage, CurrentUserId);
            if (i < messages.Count - theirUnreadCount)
            {
                newMessageViewModel.IsRead = true;
            }
            Messages.Add(newMessageViewModel);
        }
    }

    public void HandleIncomingMessage(MessageDataTransferObject message)
    {
        double oneSecondTolerance = 1;

        if (message.conversationId != ConversationId)
        {
            return;
        }

        bool messageExists = Messages.Any(messageItem =>
            messageItem.Content == message.content &&
            Math.Abs((messageItem.SentAt - message.sentAt).TotalSeconds) < oneSecondTolerance);

        if (messageExists)
        {
            return;
        }
        Messages.Add(new MessageViewModel(message, CurrentUserId));
    }

    private string inputText = string.Empty;
    public string InputText
    {
        get => inputText;
        set
        {
            inputText = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSend));
        }
    }

    public bool CanSend => !string.IsNullOrWhiteSpace(InputText);

    public void SendMessage()
    {
        if (!CanSend)
        {
            return;
        }

        int unassignedIdentifier = -1;

        var messageDataTransferObject = new MessageDataTransferObject(
            unassignedIdentifier,
            ConversationId,
            CurrentUserId,
            unassignedIdentifier,
            DateTime.Now,
            InputText,
            MessageType.MessageText,
            null,
            false,
            false,
            false,
            false,
            unassignedIdentifier,
            unassignedIdentifier);

        var newViewModel = new MessageViewModel(messageDataTransferObject, CurrentUserId);

        Messages.Add(newViewModel);
        InputText = string.Empty;
        MessageSent.Invoke(messageDataTransferObject);
    }

    public void ResolveBookingRequest(int messageId, bool accepted)
    {
        var targetMessage = Messages.FirstOrDefault(messageItem => messageItem.Id == messageId);
        if (targetMessage == null)
        {
            return;
        }
        BookingRequestUpdate?.Invoke(messageId, targetMessage.ConversationId, accepted, !accepted);
    }

    public void UpdateCashAgreement(int messageId)
    {
        var targetMessage = Messages.FirstOrDefault(messageItem => messageItem.Id == messageId);
        if (targetMessage == null)
        {
            return;
        }
        CashAgreementAccept?.Invoke(messageId, targetMessage.ConversationId);
    }

    public void ProceedToPayment(int messageId)
    {
        var targetMessage = Messages.FirstOrDefault(messageItem => messageItem.Id == messageId);
    }

    public void SendImage(string fileName)
    {
        int unassignedIdentifier = -1;

        var messageDataTransferObject = new MessageDataTransferObject(
            unassignedIdentifier,
            ConversationId,
            CurrentUserId,
            unassignedIdentifier,
            DateTime.Now,
            string.Empty,
            MessageType.MessageImage,
            fileName,
            false,
            false,
            false,
            false,
            unassignedIdentifier,
            unassignedIdentifier);

        var newViewModel = new MessageViewModel(messageDataTransferObject, CurrentUserId);
        InputText = string.Empty;
        MessageSent.Invoke(messageDataTransferObject);
    }

    public void RaiseBookingRequestUpdate(int messageId, int conversationId, bool accepted, bool resolved)
    {
        BookingRequestUpdate?.Invoke(messageId, conversationId, accepted, resolved);
    }

    public void RaiseCashAgreementAccept(int messageId, int conversationId)
    {
        CashAgreementAccept?.Invoke(messageId, conversationId);
    }

    public void RaiseMessageSent(MessageDataTransferObject message)
    {
        MessageSent?.Invoke(message);
    }
}