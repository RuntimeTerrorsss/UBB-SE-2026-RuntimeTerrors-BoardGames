using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using BookingBoardgamesILoveBan.Src.Chat.DTO;
using BookingBoardgamesILoveBan.Src.Chat.Service;
using BookingBoardgamesILoveBan.Src.Chat.ViewModel;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;

namespace BookingBoardgamesILoveBan.Src.Chat.ViewModel;

public class ChatPageViewModel
{
    public LeftPanelViewModel LeftPanelModelView { get; }
    public ChatViewModel ChatModelView { get; }

    private readonly int currentUserId;
    private readonly ConversationService conversationService;

    public ConversationService ConversationService
    {
        get => conversationService;
    }

    private List<ConversationDataTransferObject> conversations = new ();

    public ChatPageViewModel(int currentUser)
    : this(currentUser, new ConversationService(App.ConversationRepository, currentUser))
    {
    }

    public ChatPageViewModel(int currentUser, ConversationService service) : this(currentUser, service, App.UserRepository)
    {
    }

    public ChatPageViewModel(int currentUser, ConversationService service, IUserRepository userRepository)
    {
        LeftPanelModelView = new LeftPanelViewModel();
        ChatModelView = new ChatViewModel(currentUser);
        currentUserId = currentUser;

        LeftPanelModelView.PropertyChanged += OnLeftPanelPropertyChanged;
        ChatModelView.MessageSent += OnMessageSent;
        ChatModelView.BookingRequestUpdate += UpdateBookingRequest;
        ChatModelView.CashAgreementAccept += UpdateCashAgreement;

        conversationService = service;
        conversations = conversationService.FetchConversations();

        foreach (var conversationItem in conversations)
        {
            LeftPanelModelView.HandleIncomingConversation(
                conversationItem,
                conversationService.GetOtherUserNameByConversationDTO(conversationItem),
                currentUserId,
                userRepository);
        }

        conversationService.ActionMessageProcessed += OnMessageReceived;
        conversationService.ActionConversationProcessed += OnConversationReceived;
        conversationService.ActionReadReceiptProcessed += OnReadReceiptReceived;
        conversationService.ActionMessageUpdateProcessed += OnMessageUpdateReceived;
    }

    private void OnLeftPanelPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
        if (propertyChangedEventArgs.PropertyName != nameof(LeftPanelViewModel.SelectedConversation))
        {
            return;
        }
        if (LeftPanelModelView.SelectedConversation == null)
        {
            return;
        }

        var matchedConversation = conversations.FirstOrDefault(conversationItem => conversationItem.Id == LeftPanelModelView.SelectedConversation.ConversationId);
        if (matchedConversation == null)
        {
            return;
        }

        int selectedConversationOtherUserUnreadCount = matchedConversation.UnreadCount.FirstOrDefault(unreadItem => unreadItem.Key != currentUserId).Value;
        ChatModelView.LoadConversation(LeftPanelModelView.SelectedConversation, matchedConversation.MessageList, selectedConversationOtherUserUnreadCount);

        SendReadReceipt(matchedConversation);
    }

    private void OnMessageSent(MessageDataTransferObject message)
    {
        int firstParticipantIndex = 0;
        int secondParticipantIndex = 1;

        var matchedConversation = conversations.FirstOrDefault(conversationItem => conversationItem.Id == message.conversationId);
        message = message with { receiverId = matchedConversation.Participants[firstParticipantIndex] == message.senderId ? matchedConversation.Participants[secondParticipantIndex] : matchedConversation.Participants[firstParticipantIndex] };
        conversationService.SendMessage(message);
    }

    private void SendReadReceipt(ConversationDataTransferObject conversation)
    {
        conversationService.SendReadReceipt(conversation);
    }

    private void OnSendMessageUpdate(MessageDataTransferObject message)
    {
        conversationService.UpdateMessage(message);
    }

    private void OnMessageReceived(MessageDataTransferObject message, string senderName)
    {
        var matchedConversation = conversations.FirstOrDefault(conversationItem => conversationItem.Id == message.conversationId);

        matchedConversation?.AddMessageToListDTO(message);

        LeftPanelModelView.HandleIncomingMessage(message, senderName);
        ChatModelView.HandleIncomingMessage(message);
        if (ChatModelView.ConversationId == message.conversationId)
        {
            SendReadReceipt(matchedConversation);
        }
    }

    private void UpdateBookingRequest(int messageId, int conversationId, bool accepted, bool resolved)
    {
        var matchedConversation = conversations.FirstOrDefault(conversationItem => conversationItem.Id == conversationId);
        var targetMessage = matchedConversation?.MessageList.FirstOrDefault(messageItem => messageItem.id == messageId);
        if (targetMessage == null)
        {
            return;
        }
        targetMessage = targetMessage with { isResolved = resolved, isAccepted = accepted };
        OnSendMessageUpdate(targetMessage);
    }

    private void UpdateCashAgreement(int messageId, int conversationId)
    {
        var matchedConversation = conversations.FirstOrDefault(conversationItem => conversationItem.Id == conversationId);
        var targetMessage = matchedConversation?.MessageList.FirstOrDefault(messageItem => messageItem.id == messageId);
        if (targetMessage == null)
        {
            return;
        }
        if (currentUserId == targetMessage.senderId)
        {
            targetMessage = targetMessage with { isAcceptedBySeller = true };
        }
        if (currentUserId == targetMessage.receiverId)
        {
            targetMessage = targetMessage with { isAcceptedByBuyer = true };
        }
        OnSendMessageUpdate(targetMessage);
    }

    private void OnConversationReceived(ConversationDataTransferObject conversation, string otherUsername)
    {
        conversations.Add(conversation);
        LeftPanelModelView.HandleIncomingConversation(conversation, otherUsername, currentUserId);
    }

    private void OnReadReceiptReceived(ReadReceiptDataTransferObject readReceipt)
    {
        var matchedConversation = conversations.FirstOrDefault(conversationItem => conversationItem.Id == readReceipt.conversationId);
        matchedConversation.LastRead[readReceipt.readerId] = readReceipt.receiptTimeStamp;
        matchedConversation.UpdateUnreadCounts();
        if (ChatModelView.ConversationId == readReceipt.conversationId && readReceipt.readerId != currentUserId)
        {
            ChatModelView.LoadConversation(LeftPanelModelView.SelectedConversation, matchedConversation.MessageList, matchedConversation.UnreadCount[readReceipt.readerId]);
        }
    }

    private void OnMessageUpdateReceived(MessageDataTransferObject updatedMessage, string senderName)
    {
        int noUnreadMessagesCount = 0;
        var matchedConversation = conversations.FirstOrDefault(conversationItem => conversationItem.Id == updatedMessage.conversationId);
        if (matchedConversation == null)
        {
            return;
        }
        for (int i = 0; i < matchedConversation.MessageList.Count; i++)
        {
            if (matchedConversation.MessageList[i].id == updatedMessage.id)
            {
                matchedConversation.MessageList[i] = updatedMessage;
                if (ChatModelView.ConversationId == updatedMessage.conversationId)
                {
                    ChatModelView.LoadConversation(LeftPanelModelView.SelectedConversation, matchedConversation.MessageList, noUnreadMessagesCount);
                }
                break;
            }
        }
    }
}