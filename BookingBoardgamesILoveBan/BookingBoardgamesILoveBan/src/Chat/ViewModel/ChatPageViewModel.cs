using BookingBoardgamesILoveBan.src.Chat.DTO;
using BookingBoardgamesILoveBan.src.Chat.Repository;
using BookingBoardgamesILoveBan.src.Chat.Service;
using BookingBoardgamesILoveBan.src.Enum;
using BookingBoardgamesILoveBan.src.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace BookingBoardgamesILoveBan.src.Chat.ViewModel;

public class ChatPageViewModel
{
    public LeftPanelViewModel LeftPanel { get; }
    public ChatViewModel Chat { get; }

    private readonly int _currentUserId;
    private readonly ConversationService _conversationService;
    public ConversationService ConversationService
    {
        get => _conversationService;
    }
    private List<ConversationDTO> _conversations = new();

    public ChatPageViewModel(int currentUser)
    {
        LeftPanel = new LeftPanelViewModel();
        Chat = new ChatViewModel(currentUser);
        _currentUserId = currentUser;

        LeftPanel.PropertyChanged += OnLeftPanelPropertyChanged;
        Chat.MessageSent += OnMessageSent;
        Chat.BookingRequestUpdate += UpdateBookingRequest;
        Chat.CashAgreementAccept += UpdateCashAgreement;

        _conversationService = new ConversationService(App.ConversationRepository, currentUser);

        _conversations = _conversationService.FetchConversations();

        foreach (var convo in _conversations)
        {
            LeftPanel.HandleIncomingConversation(convo, _conversationService.GetOtherUserNameByConversationDTO(convo), _currentUserId);
        }

        _conversationService.MessageProcessed += OnMessageReceived;
        _conversationService.ConversationProcessed += OnConversationReceived;
        _conversationService.ReadReceiptProcessed += OnReadReceiptReceived;
        _conversationService.MessageUpdateProcessed += OnMessageUpdateReceived;
    }

    /// <summary>
    /// Whenever the selected conversation in the left panel changes, this method is triggered. 
    /// It finds the corresponding conversation DTO and loads the messages into the chat view model. 
    /// It also sends a read receipt for that conversation.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnLeftPanelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(LeftPanelViewModel.SelectedConversation)) return;
        if (LeftPanel.SelectedConversation == null) return;

        var convo = _conversations.FirstOrDefault(c => c.Id == LeftPanel.SelectedConversation.ConversationId);
        if (convo == null) return;
        int selectedConversationOtherUserUnreadCount = convo.UnreadCount.FirstOrDefault(x => x.Key != _currentUserId).Value;
        Chat.LoadConversation(LeftPanel.SelectedConversation, convo.MessageList, selectedConversationOtherUserUnreadCount);

        SendReadReceipt(convo);
    }

    /// <summary>
    /// This method is called when a message is sent from the chat view model. 
    /// It finds the corresponding conversation DTO to determine the receiver of the message, 
    /// then calls the conversation service to send the message.
    /// </summary>
    /// <param name="message"></param>
    private void OnMessageSent(MessageDTO message)
    {
        var convo = _conversations.FirstOrDefault(c => c.Id == message.ConversationId);
        message = message with { ReceiverId = convo.Participants[0] == message.SenderId ? convo.Participants[1] : convo.Participants[0] };
        _conversationService.SendMessage(message);
    }

    /// <summary>
    /// This method sends a read receipt for a given conversation. It is called when a conversation is selected in the left panel
    /// or when receiving a new message for the currently open conversation. 
    /// </summary>
    /// <param name="conversation"></param>
    private void SendReadReceipt(ConversationDTO conversation)
    {
        _conversationService.SendReadReceipt(conversation);
    }

    /// <summary>
    /// This method is called when a message is updated (e.g. booking request accepted/declined, cash agreement accepted).
    /// </summary>
    /// <param name="message"></param>
    private void OnSendMessageUpdate(MessageDTO message)
    {
        _conversationService.UpdateMessage(message);
    }

    /// <summary>
    /// This method is called when a new message is received from the conversation service. 
    /// It finds the corresponding conversation DTO and adds the new message to it.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="senderName"></param>
    private void OnMessageReceived(MessageDTO message, string senderName)
    {
        var convo = _conversations.FirstOrDefault(c => c.Id == message.ConversationId);


        convo?.AddMessageDTO(message);

        LeftPanel.HandleIncomingMessage(message, senderName);
        Chat.HandleIncomingMessage(message);
        if(Chat.ConversationId == message.ConversationId)
        {
            SendReadReceipt(convo);
        }
    }

    /// <summary>
    /// This method is called when a booking request is accepted, declined or cancelled.
    /// It sends an updated version of the corresponding message to the conversation service.
    /// </summary>
    /// <param name="messageId"></param>
    /// <param name="conversationId"></param>
    /// <param name="accepted"></param>
    /// <param name="resolved"></param>
    private void UpdateBookingRequest(int messageId, int conversationId, bool accepted, bool resolved)
    {
        var convo = _conversations.FirstOrDefault(c => c.Id == conversationId);
        var message = convo?.MessageList.FirstOrDefault(m => m.Id == messageId);
        if (message == null) return;
        message = message with { IsResolved = resolved, IsAccepted = accepted };
        OnSendMessageUpdate(message);
    }

    /// <summary>
    /// This method is called when a cash agreement is accepted by either the buyer or the seller.
    /// </summary>
    /// <param name="messageId"></param>
    /// <param name="conversationId"></param>
    private void UpdateCashAgreement(int messageId, int conversationId)
    {
        var convo = _conversations.FirstOrDefault(c => c.Id == conversationId);
        var message = convo?.MessageList.FirstOrDefault(m => m.Id == messageId);
        if (message == null) return;
            if (_currentUserId == message.SenderId)
        {
            message = message with { IsAcceptedBySeller = true };
        }
        if (_currentUserId == message.ReceiverId)
        {
            message = message with { IsAcceptedByBuyer= true };
        }
        OnSendMessageUpdate(message);
    }

    /// <summary>
    /// This method is called when a new conversation is received from the conversation service.
    /// This is to handle the case when someone wants to start a conversation with a user thats currently logged in.
    /// </summary>
    /// <param name="convo"></param>
    /// <param name="otherUsername"></param>
    private void OnConversationReceived(ConversationDTO convo, string otherUsername)
    {
        _conversations.Add(convo);
        LeftPanel.HandleIncomingConversation(convo, otherUsername, _currentUserId);
    }

    /// <summary>
    /// This method is called when a read receipt is received from the conversation service.
    /// </summary>
    /// <param name="readReceipt"></param>
    private void OnReadReceiptReceived(ReadReceiptDTO readReceipt)
    {
        var convo = _conversations.FirstOrDefault(c => c.Id == readReceipt.ConversationId);
        convo.LastRead[readReceipt.ReaderId] = readReceipt.TimeStamp;
        convo.UpdateUnreadCounts();
        if (Chat.ConversationId == readReceipt.ConversationId && readReceipt.ReaderId != _currentUserId)
        {
            Chat.LoadConversation(LeftPanel.SelectedConversation, convo.MessageList, convo.UnreadCount[readReceipt.ReaderId]);
        }
    }

    /// <summary>
    /// This method is called when a message update is received from the conversation service. 
    /// </summary>
    /// <param name="updatedMessage"></param>
    /// <param name="senderName"></param>
    private void OnMessageUpdateReceived(MessageDTO updatedMessage, string senderName)
    {
        var convo = _conversations.FirstOrDefault(c => c.Id == updatedMessage.ConversationId);
        if (convo == null) return;
        for(int i = 0; i < convo.MessageList.Count; i++)
        {
            if (convo.MessageList[i].Id == updatedMessage.Id)
            {
                convo.MessageList[i] = updatedMessage;
                if (Chat.ConversationId == updatedMessage.ConversationId)
                {
                    Chat.LoadConversation(LeftPanel.SelectedConversation, convo.MessageList, 0); // unreadcount is 0 cus like think about it how could they have possibly sent an update if they handnt read the convo!
                }
                break;
            }
        }
    }
}