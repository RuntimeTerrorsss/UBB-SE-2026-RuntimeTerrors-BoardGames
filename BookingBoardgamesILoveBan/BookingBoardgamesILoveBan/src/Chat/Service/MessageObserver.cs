using BookingBoardgamesILoveBan.src.Chat.Model;
using BookingBoardgamesILoveBan.src.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesILoveBan.src.Chat.Service
{
    public interface MessageObserver
    {
        public void OnMessageReceived(Message message);
        public void OnMessageUpdateReceived(Message message);
        public void OnConversationReceived(Conversation conversation);
        public void OnReadReceiptReceived(ReadReceipt readReceipt);
    }
}
