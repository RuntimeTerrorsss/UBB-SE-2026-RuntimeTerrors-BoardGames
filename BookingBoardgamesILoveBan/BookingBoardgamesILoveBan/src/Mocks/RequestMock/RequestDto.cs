using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesILoveBan.Src.Mocks.RequestMock
{
    public class RequestDto
    {
        public int Id { get; set; }
        public string GameName { get; set; }
        public int ClientId { get; set; }
        public int OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string ClientName { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal Price { get; set; }

        public RequestDto(int id, string gameName, int clientId, int ownerId, string ownerName, string clientName, DateTime startDate, DateTime endDate, decimal price)
        {
            this.Id = id;
            this.GameName = gameName;
            this.ClientId = clientId;
            this.OwnerId = ownerId;
            this.OwnerName = ownerName;
            this.ClientName = clientName;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.Price = price;
        }
    }
}
