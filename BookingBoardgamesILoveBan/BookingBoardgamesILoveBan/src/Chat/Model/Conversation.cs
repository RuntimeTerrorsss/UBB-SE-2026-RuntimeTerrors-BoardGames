using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.Model;

namespace BookingBoardgamesILoveBan.Src.Chat.Model
{
    public class Conversation
    {
        public int ConversationId { get; set; }
        public List<Message> ConversationMessageList { get; set; }
        public int[] ConversationParticipantIds { get; set; }
        public Dictionary<int, DateTime> LastMessageReadTime { get; set; }
        public Dictionary<int, int> UnreadMessagesCount { get; set; } // <id, count>
        public Conversation(int convId, int[] participants, List<Message> messages, Dictionary<int, DateTime> lastRead)
        {
            ConversationId = convId;
            ConversationParticipantIds = participants;
            ConversationMessageList = messages;
            LastMessageReadTime = lastRead;
        }
    }
}
