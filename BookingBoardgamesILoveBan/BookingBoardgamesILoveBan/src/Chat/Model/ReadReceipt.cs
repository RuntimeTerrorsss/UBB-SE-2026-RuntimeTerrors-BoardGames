using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesILoveBan.src.Chat.Model
{
    public record ReadReceipt(
        int ConversationId,
        int ReaderId,
        int ReceiverId,
        DateTime TimeStamp
    );
}
