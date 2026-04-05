using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesILoveBan.src.Enum
{
    public enum MessageType
    {
        System,
        Text,
        Image,
        RentalRequest,
        CashAgreement
    }

    public static class MessageTypeExtensions
    {
        public static string ToString(this MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.Text:
                    return "TEXT";
                case MessageType.Image:
                    return "IMAGE";
                case MessageType.RentalRequest:
                    return "RENTAL_REQUEST";
                case MessageType.CashAgreement:
                    return "CASH_AGREEMENT";
                case MessageType.System:
                    return "SYSTEM";
                default:
                    return "Unknown";
            }
        }
    }
}
