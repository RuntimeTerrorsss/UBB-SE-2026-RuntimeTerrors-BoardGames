using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.Chat.Model;
using BookingBoardgamesILoveBan.Src.Model;

namespace BookingBoardgamesILoveBan.Src.Chat.Service
{
    public interface IMessageObserver
    {
        public void OnMessageReceived(Message message);
        public void OnMessageUpdateReceived(Message message);
        public void OnConversationReceived(Conversation conversation);
        public void OnReadReceiptReceived(ReadReceipt readReceipt);
    }
}
