using BookingBoardgamesILoveBan.src.Chat.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesILoveBan.src.Chat.DTO
{
    public record ReadReceiptDTO(
        int ConversationId,
        int ReaderId,
        int ReceiverId,
        DateTime TimeStamp
    );
}
