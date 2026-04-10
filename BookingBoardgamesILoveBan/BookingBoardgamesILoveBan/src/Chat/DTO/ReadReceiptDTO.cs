using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.Chat.Model;

namespace BookingBoardgamesILoveBan.Src.Chat.DTO
{
    public record ReadReceiptDTO(
        int conversationId,
        int readerId,
        int receiverId,
        DateTime timeStamp);
}
