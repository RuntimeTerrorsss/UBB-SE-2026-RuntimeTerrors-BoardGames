using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Constants;

namespace BookingBoardgamesILoveBan.Src.PaymentCard.Constants
{
    public class CardPaymentConstants : PaymentConstrants
    {
        public const int NullBalance = 0;
        public const int NullPrice = 0;
        public const double TimerBeforeClosingPayment = 30_000;
        public const double TimerForRefreshingBalance = 4000;
        public const int LoadingTime = 50;
    }
}
