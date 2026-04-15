using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesILoveBan.Src.Mocks.RequestMock
{
    public interface IRequestService
    {
        public Request GetById(int id);

        public decimal GetRequestPrice(int requestId);

        public string GetGameName(int requestId);
    }
}
