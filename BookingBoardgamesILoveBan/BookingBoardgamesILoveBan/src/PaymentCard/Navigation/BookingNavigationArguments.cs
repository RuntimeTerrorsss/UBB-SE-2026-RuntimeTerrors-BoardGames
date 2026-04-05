using BookingBoardgamesILoveBan.src.Chat.Service;
using Microsoft.UI.Composition.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;



namespace BookingBoardgamesILoveBan.src.PaymentCard.Navigation
{
    public class BookingNavigationArguments
    {
        public int RequestId { get; set; }
        
        public string DeliveryAddress { get; set; }

        public int BookingMessageId{ get; set; }

        public ConversationService ConversationService { get; set; }

        public Window CurrentWindow { get; set; }
    }
}
