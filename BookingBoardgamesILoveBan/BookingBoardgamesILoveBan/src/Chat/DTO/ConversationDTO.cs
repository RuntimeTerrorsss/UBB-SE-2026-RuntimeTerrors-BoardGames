using System;
using System.Collections.Generic;
using System.Linq;

namespace BookingBoardgamesILoveBan.Src.Chat.DTO
{
    public class ConversationDataTransferObject
    {
        public int Id { get; set; }
        public List<MessageDataTransferObject> MessageList { get; set; }
        public int[] Participants { get; set; }
        public Dictionary<int, DateTime> LastRead { get; set; }
        public Dictionary<int, int> UnreadCount { get; set; }

        public ConversationDataTransferObject(int conversationId, int[] participants, List<MessageDataTransferObject> messages, Dictionary<int, DateTime> lastRead)
        {
            Id = conversationId;
            Participants = participants;
            MessageList = messages;
            LastRead = lastRead;
            UnreadCount = participants.ToDictionary(participant => participant, participant => 0);
            UpdateUnreadCounts();
        }

        public void AddMessageToListDTO(MessageDataTransferObject newMessage)
        {
            MessageList.Add(newMessage);
            UpdateUnreadCounts();
        }

        public void UpdateUnreadCounts()
        {
            int firstParticipantIndex = 0;
            int secondParticipantIndex = 1;
            int defaultUnreadCount = 0;
            int systemMessageSenderIdentifier = 0;

            UnreadCount[Participants[firstParticipantIndex]] = defaultUnreadCount;
            UnreadCount[Participants[secondParticipantIndex]] = defaultUnreadCount;

            foreach (var messageItem in MessageList)
            {
                if (messageItem.receiverId == systemMessageSenderIdentifier)
                {
                    continue;
                }

                if (messageItem.sentAt >= LastRead[messageItem.receiverId])
                {
                    UnreadCount[messageItem.receiverId]++;
                }
            }
        }
    }
}