using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using BookingBoardgamesILoveBan.Src.Chat.DTO;
using BookingBoardgamesILoveBan.Src.Chat.Service;
using BookingBoardgamesILoveBan.Src.Chat.ViewModel;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;

namespace BookingBoardgamesILoveBan.src.Chat.ViewModel;

public class ChatPageViewModel
{
    public LeftPanelViewModel LeftPanel { get; }
    public ChatViewModel Chat { get; }

    private readonly int currentUserId;
    private readonly ConversationService conversationService;
    public ConversationService ConversationService
    {
        get => conversationService;
    }
    private List<ConversationDTO> conversations = new ();
    public ChatPageViewModel(int currentUser)
    : this(currentUser, new ConversationService(App.ConversationRepository, currentUser))
    {
    }

    public ChatPageViewModel(int currentUser, ConversationService service) : this(currentUser, service, App.UserService)
    {
    }

    public ChatPageViewModel(int currentUser, ConversationService service, IUserService userService)
    {
        LeftPanel = new LeftPanelViewModel();
        Chat = new ChatViewModel(currentUser);
        currentUserId = currentUser;

        LeftPanel.PropertyChanged += OnLeftPanelPropertyChanged;
        Chat.MessageSent += OnMessageSent;
        Chat.BookingRequestUpdate += UpdateBookingRequest;
        Chat.CashAgreementAccept += UpdateCashAgreement;

        conversationService = service;

        conversations = conversationService.FetchConversations();

        foreach (var conversation in conversations)
        {
            LeftPanel.HandleIncomingConversation(
                conversation,
                conversationService.GetOtherUserNameByConversationDTO(conversation),
                currentUserId,
                userService);
        }

        conversationService.MessageProcessed += OnMessageReceived;
        conversationService.ConversationProcessed += OnConversationReceived;
        conversationService.ReadReceiptProcessed += OnReadReceiptReceived;
        conversationService.MessageUpdateProcessed += OnMessageUpdateReceived;
    }

    /// <summary>
    /// Whenever the selected conversation in the left panel changes, this method is triggered.
    /// It finds the corresponding conversation DTO and loads the messages into the chat view model.
    /// It also sends a read receipt for that conversation.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="eventArgs"></param>
    private void OnLeftPanelPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.PropertyName != nameof(LeftPanelViewModel.SelectedConversation))
        {
            return;
        }
        if (LeftPanel.SelectedConversation == null)
        {
            return;
        }

        var conversation = conversations.FirstOrDefault(firstConversation => firstConversation.Id == LeftPanel.SelectedConversation.ConversationId);
        if (conversation == null)
        {
            return;
        }
        int selectedConversationOtherUserUnreadCount = conversation.UnreadCount.FirstOrDefault(x => x.Key != currentUserId).Value;
        Chat.LoadConversation(LeftPanel.SelectedConversation, conversation.MessageList, selectedConversationOtherUserUnreadCount);

        SendReadReceipt(conversation);
    }

    /// <summary>
    /// This method is called when a message is sent from the chat view model.
    /// It finds the corresponding conversation DTO to determine the receiver of the message,
    /// then calls the conversation service to send the message.
    /// </summary>
    /// <param name="message"></param>
    private void OnMessageSent(MessageDTO message)
    {
        var conversation = conversations.FirstOrDefault(firstConversation => firstConversation.Id == message.conversationId);
        message = message with { receiverId = conversation.Participants[0] == message.senderId ? conversation.Participants[1] : conversation.Participants[0] };
        conversationService.SendMessage(message);
    }

    /// <summary>
    /// This method sends a read receipt for a given conversation. It is called when a conversation is selected in the left panel
    /// or when receiving a new message for the currently open conversation.
    /// </summary>
    /// <param name="conversation"></param>
    private void SendReadReceipt(ConversationDTO conversation)
    {
        conversationService.SendReadReceipt(conversation);
    }

    /// <summary>
    /// This method is called when a message is updated (e.g. booking request accepted/declined, cash agreement accepted).
    /// </summary>
    /// <param name="message"></param>
    private void OnSendMessageUpdate(MessageDTO message)
    {
        conversationService.UpdateMessage(message);
    }

    /// <summary>
    /// This method is called when a new message is received from the conversation service.
    /// It finds the corresponding conversation DTO and adds the new message to it.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="senderName"></param>
    private void OnMessageReceived(MessageDTO message, string senderName)
    {
        var conversation = conversations.FirstOrDefault(firstConversation => firstConversation.Id == message.conversationId);

        conversation?.AddMessageDTO(message);

        LeftPanel.HandleIncomingMessage(message, senderName);
        Chat.HandleIncomingMessage(message);
        if (Chat.ConversationId == message.conversationId)
        {
            SendReadReceipt(conversation);
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
        var conversation = conversations.FirstOrDefault(firstConversation => firstConversation.Id == conversationId);
        var message = conversation?.MessageList.FirstOrDefault(firstMessage => firstMessage.id == messageId);
        if (message == null)
        {
            return;
        }
        message = message with { isResolved = resolved, isAccepted = accepted };
        OnSendMessageUpdate(message);
    }

    /// <summary>
    /// This method is called when a cash agreement is accepted by either the buyer or the seller.
    /// </summary>
    /// <param name="messageId"></param>
    /// <param name="conversationId"></param>
    private void UpdateCashAgreement(int messageId, int conversationId)
    {
        var conversation = conversations.FirstOrDefault(firstConversation => firstConversation.Id == conversationId);
        var message = conversation?.MessageList.FirstOrDefault(firstMessage => firstMessage.id == messageId);
        if (message == null)
        {
            return;
        }
        if (currentUserId == message.senderId)
        {
            message = message with { isAcceptedBySeller = true };
        }
        if (currentUserId == message.receiverId)
        {
            message = message with { isAcceptedByBuyer = true };
        }
        OnSendMessageUpdate(message);
    }

    /// <summary>
    /// This method is called when a new conversation is received from the conversation service.
    /// This is to handle the case when someone wants to start a conversation with a user thats currently logged in.
    /// </summary>
    /// <param name="conversation"></param>
    /// <param name="otherUsername"></param>
    private void OnConversationReceived(ConversationDTO conversation, string otherUsername)
    {
        conversations.Add(conversation);
        LeftPanel.HandleIncomingConversation(conversation, otherUsername, currentUserId);
    }

    /// <summary>
    /// This method is called when a read receipt is received from the conversation service.
    /// </summary>
    /// <param name="readReceipt"></param>
    private void OnReadReceiptReceived(ReadReceiptDTO readReceipt)
    {
        var conversation = conversations.FirstOrDefault(firstConversation => firstConversation.Id == readReceipt.conversationId);
        conversation.LastRead[readReceipt.readerId] = readReceipt.timeStamp;
        conversation.UpdateUnreadCounts();
        if (Chat.ConversationId == readReceipt.conversationId && readReceipt.readerId != currentUserId)
        {
            Chat.LoadConversation(LeftPanel.SelectedConversation, conversation.MessageList, conversation.UnreadCount[readReceipt.readerId]);
        }
    }

    /// <summary>
    /// This method is called when a message update is received from the conversation service.
    /// </summary>
    /// <param name="updatedMessage"></param>
    /// <param name="senderName"></param>
    private void OnMessageUpdateReceived(MessageDTO updatedMessage, string senderName)
    {
        var conversation = conversations.FirstOrDefault(firstConversation => firstConversation.Id == updatedMessage.conversationId);
        if (conversation == null)
        {
            return;
        }
        for (int i = 0; i < conversation.MessageList.Count; i++)
        {
            if (conversation.MessageList[i].id == updatedMessage.id)
            {
                conversation.MessageList[i] = updatedMessage;
                if (Chat.ConversationId == updatedMessage.conversationId)
                {
                    Chat.LoadConversation(LeftPanel.SelectedConversation, conversation.MessageList, 0); // unreadcount is 0 cus like think about it how could they have possibly sent an update if they handnt read the convo!
                }
                break;
            }
        }
    }
}