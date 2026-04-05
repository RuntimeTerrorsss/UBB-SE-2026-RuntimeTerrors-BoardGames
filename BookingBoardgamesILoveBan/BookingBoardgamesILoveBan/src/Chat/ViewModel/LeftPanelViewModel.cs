using BookingBoardgamesILoveBan.src.Chat.DTO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.UI.Xaml;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;

namespace BookingBoardgamesILoveBan.src.Chat.ViewModel
{
    public class LeftPanelViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public bool IsEmptyStateVisible => _allConversations.Count == 0;
        public bool IsNoMatchesVisible => _allConversations.Count > 0 && Conversations.Count == 0;
        public bool IsListVisible => Conversations.Count > 0;

        private void RefreshUIStates()
        {
            OnPropertyChanged(nameof(IsEmptyStateVisible));
            OnPropertyChanged(nameof(IsNoMatchesVisible));
            OnPropertyChanged(nameof(IsListVisible));
        }

        private List<ConversationPreviewModel> _allConversations = new();

        private ObservableCollection<ConversationPreviewModel> _conversations;
        // this one below is what is sent to left panel and is updated according to the search string
        public ObservableCollection<ConversationPreviewModel> Conversations
        {
            get => _conversations;
            set
            {
                _conversations = value;
                OnPropertyChanged();
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    ApplyFilter(); // whenever filter is changed, search is triggered
                }
            }
        }

        private int? _selectedConversationId;
        public ConversationPreviewModel SelectedConversation
        {
            get => Conversations.FirstOrDefault(c => c.ConversationId == _selectedConversationId);
            set
            {
                if (_selectedConversationId != value?.ConversationId)
                {
                    _selectedConversationId = value?.ConversationId;

                    if (_selectedConversationId.HasValue)
                    {
                        MarkAsRead(_selectedConversationId.Value);
                    }

                    OnPropertyChanged();
                }
            }
        }

        public LeftPanelViewModel()
        {
            Conversations = new ObservableCollection<ConversationPreviewModel>();
        }

        /// <summary>
        /// This method applies the search filter to the conversations list. 
        /// </summary>
        private void ApplyFilter()
        {
            var filtered = _allConversations
                .Where(c => string.IsNullOrEmpty(SearchText) ||
                            c.DisplayName.Contains(SearchText, StringComparison.Ordinal))
                .ToList();

            // Remove items that no longer match
            for (int i = Conversations.Count - 1; i >= 0; i--)
            {
                if (!filtered.Contains(Conversations[i]))
                    Conversations.RemoveAt(i);
            }

            // Add missing items and fix ordering
            for (int i = 0; i < filtered.Count; i++)
            {
                var item = filtered[i];
                int currentIndex = Conversations.IndexOf(item);

                if (currentIndex == -1)
                    Conversations.Insert(i, item);       // not present, insert at correct position
                else if (currentIndex != i)
                    Conversations.Move(currentIndex, i); // present but wrong position, move it
                                                         // else: already in the right place, do nothing
            }
            OnPropertyChanged(nameof(SelectedConversation));
            RefreshUIStates();
        }

        /// <summary>
        /// Marks a conversation as read by setting its UnreadCount to 0. 
        /// </summary>
        /// <param name="conversationId"></param>
        private void MarkAsRead(int conversationId)
        {
            var existing = _allConversations.FirstOrDefault(c => c.ConversationId == conversationId);
            if (existing == null || existing.UnreadCount == 0) return;
            existing.UnreadCount = 0;
        }

        /// <summary>
        /// Handles an incoming message by either updating an existing conversation preview or creating a new one if it doesn't exist.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="senderName"></param>
        public void HandleIncomingMessage(MessageDTO message, string senderName)
        {
            var existing = _allConversations.FirstOrDefault(c => c.ConversationId == message.ConversationId);

            if (existing != null)
            {
                existing.LastMessageText = message.Content;
                existing.Timestamp = DateTime.Now;
                existing.UnreadCount = (message.ConversationId == _selectedConversationId) ? 0 : existing.UnreadCount + 1;

                // move to top in both collections
                _allConversations.Remove(existing);
                _allConversations.Insert(0, existing);
            }
            else
            {
                var newConvo = new ConversationPreviewModel(
                    message.ConversationId,
                    senderName,
                    senderName.Substring(0, 1).ToUpper(),
                    message.Content,
                    DateTime.Now,
                    unreadCount: (message.ConversationId == _selectedConversationId) ? 0 : 1,
                    App.UserService.GetById(message.ReceiverId).AvatarUrl //TODO - CHANGE
                );
                _allConversations.Insert(0, newConvo);
            }

            ApplyFilter();
        }

        /// <summary>
        /// Handles an incoming conversation by creating a new conversation preview and adding it to the list. 
        /// This is typically called when a new conversation is initiated, either by the user or by someone else.
        /// </summary>
        /// <param name="conversation"></param>
        /// <param name="displayName"></param>
        /// <param name="userId"></param>
        public void HandleIncomingConversation(ConversationDTO conversation, string displayName, int userId)
        {
            var existing = _allConversations.FirstOrDefault(c => c.ConversationId == conversation.Id);
            if (existing != null) return; // should not happen but just in case

            var otherUser = conversation.Participants[0] == userId ? conversation.Participants[1] : conversation.Participants[0];

            var newConvo = new ConversationPreviewModel(
                conversation.Id,
                displayName,
                displayName.Substring(0, 1).ToUpper(),
                conversation.MessageList.LastOrDefault()?.GetPreview() ?? "",
                conversation.MessageList.LastOrDefault()?.SentAt ?? DateTime.MinValue,
                unreadCount: conversation.UnreadCount[userId],
                App.UserService.GetById(otherUser).AvatarUrl 
            );
            _allConversations.Insert(0, newConvo);
            sortConversationsByTimestamp();
            ApplyFilter();
        }

        // Helper method to trigger the UI update
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Sorts the conversations by their timestamp in descending order (most recent first).
        /// </summary>
        public void sortConversationsByTimestamp()
        {
            _allConversations = _allConversations.OrderByDescending(c => c.Timestamp).ToList();
            Debug.WriteLine("sorted conversations:");
            ApplyFilter();
        }
    }
}

