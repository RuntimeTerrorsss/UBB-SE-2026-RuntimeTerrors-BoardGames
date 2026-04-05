using BookingBoardgamesILoveBan.src.Chat.Model;
using BookingBoardgamesILoveBan.src.Model;
using BookingBoardgamesILoveBan.src.Chat.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesILoveBan.src.Chat.DTO
{
    public class ConversationDTO
    {
        public int Id { get; set; }
        public List<MessageDTO> MessageList { get; set; }
        public int[] Participants { get; set; }
        public Dictionary<int, DateTime> LastRead { get; set; }
        public Dictionary<int, int> UnreadCount { get; set; } // <id, count>
        public ConversationDTO(int convId, int[] participants, List<MessageDTO> messages, Dictionary<int, DateTime> lastRead)
        {
            Id = convId;
            Participants = participants;
            MessageList = messages;
            LastRead = lastRead;
            UnreadCount = participants.ToDictionary(p => p, p => 0);
            UpdateUnreadCounts();
        }

        public void AddMessageDTO(MessageDTO newMessage)
        {
            MessageList.Add(newMessage);
            UpdateUnreadCounts();
        }

        public void UpdateUnreadCounts()
        {
            UnreadCount[Participants[0]] = 0;
            UnreadCount[Participants[1]] = 0;

            foreach (var message in MessageList)
            {
                // ignore system message
                if (message.ReceiverId == 0) continue;
                if (message.SentAt >= LastRead[message.ReceiverId])
                {
                    UnreadCount[message.ReceiverId] ++;
                }
            }
        }
    }
}
