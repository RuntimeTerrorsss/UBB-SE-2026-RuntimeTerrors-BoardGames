using System;
using BookingBoardgamesILoveBan.Src.Chat.Service;
using Microsoft.UI.Xaml;

namespace BookingBoardgamesILoveBan.Src.PaymentCard.Navigation
{
    public class BookingNavigationArguments
    {
        public int RequestIdentifier { get; set; }
        public string DeliveryAddress { get; set; }
        public int BookingMessageIdentifier { get; set; }
        public ConversationService ConversationService { get; set; }
        public Window CurrentWindow { get; set; }
    }
}