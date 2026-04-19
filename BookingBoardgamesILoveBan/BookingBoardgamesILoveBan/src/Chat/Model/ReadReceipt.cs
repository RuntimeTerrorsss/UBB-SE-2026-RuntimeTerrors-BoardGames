using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesILoveBan.Src.Chat.Model
{
    public record ReadReceipt(
        int conversationId,
        int messageReaderId,
        int messageReceiverId,
        DateTime timeStamp);
}
