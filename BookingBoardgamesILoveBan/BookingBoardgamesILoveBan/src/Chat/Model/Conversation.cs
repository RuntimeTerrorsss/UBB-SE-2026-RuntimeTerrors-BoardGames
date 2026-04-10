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
        public int Id { get; set; }
        public List<Message> MessageList { get; set; }
        public int[] ParticipantIds { get; set; }
        public Dictionary<int, DateTime> LastRead { get; set; }
        public Dictionary<int, int> UnreadCount { get; set; } // <id, count>
        public Conversation(int convId, int[] participants, List<Message> messages, Dictionary<int, DateTime> lastRead)
        {
            Id = convId;
            ParticipantIds = participants;
            MessageList = messages;
            LastRead = lastRead;
        }
    }
}
