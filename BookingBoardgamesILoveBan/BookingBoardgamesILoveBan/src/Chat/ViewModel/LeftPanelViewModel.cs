using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.Chat.DTO;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using Microsoft.UI.Xaml;

namespace BookingBoardgamesILoveBan.Src.Chat.ViewModel
{
    public class LeftPanelViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsEmptyStateVisible => allConversations.Count == 0;
        public bool IsNoMatchesVisible => allConversations.Count > 0 && Conversations.Count == 0;
        public bool IsListVisible => Conversations.Count > 0;

        private void RefreshUIStates()
        {
            OnPropertyChanged(nameof(IsEmptyStateVisible));
            OnPropertyChanged(nameof(IsNoMatchesVisible));
            OnPropertyChanged(nameof(IsListVisible));
        }

        private List<ConversationPreviewModel> allConversations = new ();

        private ObservableCollection<ConversationPreviewModel> conversations;

        public ObservableCollection<ConversationPreviewModel> Conversations
        {
            get => conversations;
            set
            {
                conversations = value;
                OnPropertyChanged();
            }
        }

        private string searchText = string.Empty;
        public string SearchText
        {
            get => searchText;
            set
            {
                if (searchText != value)
                {
                    searchText = value;
                    OnPropertyChanged();
                    ApplyFilter();
                }
            }
        }

        private int? selectedConversationId;
        public ConversationPreviewModel SelectedConversation
        {
            get => Conversations.FirstOrDefault(conversationItem => conversationItem.ConversationId == selectedConversationId);
            set
            {
                if (selectedConversationId != value?.ConversationId)
                {
                    selectedConversationId = value?.ConversationId;

                    if (selectedConversationId.HasValue)
                    {
                        MarkAsRead(selectedConversationId.Value);
                    }

                    OnPropertyChanged();
                }
            }
        }

        public LeftPanelViewModel()
        {
            Conversations = new ObservableCollection<ConversationPreviewModel>();
        }

        private void ApplyFilter()
        {
            var filteredConversations = allConversations
                .Where(conversationItem => string.IsNullOrEmpty(SearchText) ||
                            conversationItem.DisplayName.Contains(SearchText, StringComparison.Ordinal))
                .ToList();

            for (int i = Conversations.Count - 1; i >= 0; i--)
            {
                if (!filteredConversations.Contains(Conversations[i]))
                {
                    Conversations.RemoveAt(i);
                }
            }

            int notFoundIndex = -1;

            for (int i = 0; i < filteredConversations.Count; i++)
            {
                var filterItem = filteredConversations[i];
                int currentIndex = Conversations.IndexOf(filterItem);

                if (currentIndex == notFoundIndex)
                {
                    Conversations.Insert(i, filterItem);
                }
                else if (currentIndex != i)
                {
                    Conversations.Move(currentIndex, i);
                }
            }
            OnPropertyChanged(nameof(SelectedConversation));
            RefreshUIStates();
        }

        private void MarkAsRead(int conversationId)
        {
            int noUnreadMessagesCount = 0;
            var matchedConversation = allConversations.FirstOrDefault(conversationItem => conversationItem.ConversationId == conversationId);
            if (matchedConversation == null || matchedConversation.UnreadCount == noUnreadMessagesCount)
            {
                return;
            }
            matchedConversation.UnreadCount = noUnreadMessagesCount;
        }

        public void HandleIncomingMessage(MessageDataTransferObject message, string senderName)
        {
            HandleIncomingMessage(message, senderName, App.UserRepository);
        }

        public void HandleIncomingMessage(MessageDataTransferObject message, string senderName, IUserRepository userService)
        {
            int firstCharacterIndex = 0;
            int singleCharacterLength = 1;
            int noUnreadMessagesCount = 0;
            int singleUnreadMessageCount = 1;

            var matchedConversation = allConversations.FirstOrDefault(conversationItem => conversationItem.ConversationId == message.conversationId);

            if (matchedConversation != null)
            {
                matchedConversation.LastMessageText = message.content;
                matchedConversation.Timestamp = DateTime.Now;
                matchedConversation.UnreadCount = (message.conversationId == selectedConversationId) ? noUnreadMessagesCount : matchedConversation.UnreadCount + singleUnreadMessageCount;

                allConversations.Remove(matchedConversation);
                allConversations.Insert(0, matchedConversation);
            }
            else
            {
                var newConversationPreview = new ConversationPreviewModel(
                    message.conversationId,
                    senderName,
                    senderName.Substring(firstCharacterIndex, singleCharacterLength).ToUpper(),
                    message.content,
                    DateTime.Now,
                    unreadCountInput: (message.conversationId == selectedConversationId) ? noUnreadMessagesCount : singleUnreadMessageCount,
                    userService.GetById(message.receiverId).AvatarUrl);
                allConversations.Insert(0, newConversationPreview);
            }

            ApplyFilter();
        }

        public void HandleIncomingConversation(ConversationDataTransferObject conversation, string displayName, int userId)
        {
            HandleIncomingConversation(conversation, displayName, userId, App.UserRepository);
        }

        public void HandleIncomingConversation(ConversationDataTransferObject conversation, string displayName, int userId, IUserRepository service)
        {
            int firstParticipantIndex = 0;
            int secondParticipantIndex = 1;
            int firstCharacterIndex = 0;
            int singleCharacterLength = 1;

            var matchedConversation = allConversations.FirstOrDefault(conversationItem => conversationItem.ConversationId == conversation.Id);
            if (matchedConversation != null)
            {
                return;
            }

            var otherUserIdentifier = conversation.Participants[firstParticipantIndex] == userId ? conversation.Participants[secondParticipantIndex] : conversation.Participants[firstParticipantIndex];

            var newConversationPreview = new ConversationPreviewModel(
                conversation.Id,
                displayName,
                displayName.Substring(firstCharacterIndex, singleCharacterLength).ToUpper(),
                conversation.MessageList.LastOrDefault()?.GetChatMessagePreview() ?? string.Empty,
                conversation.MessageList.LastOrDefault()?.sentAt ?? DateTime.MinValue,
                unreadCountInput: conversation.UnreadCount[userId],
                service.GetById(otherUserIdentifier).AvatarUrl);

            allConversations.Insert(0, newConversationPreview);
            SortConversationsByTimestamp();
            ApplyFilter();
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void SortConversationsByTimestamp()
        {
            allConversations = allConversations.OrderByDescending(conversationItem => conversationItem.Timestamp).ToList();
            Debug.WriteLine("sorted conversations:");
            ApplyFilter();
        }

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}