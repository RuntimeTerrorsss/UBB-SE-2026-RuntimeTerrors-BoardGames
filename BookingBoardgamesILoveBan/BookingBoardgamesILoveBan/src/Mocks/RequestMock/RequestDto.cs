using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesILoveBan.Src.Mocks.RequestMock
{
    public class RequestDto
    {
        public RequestDto(
            int id,
            string gameName,
            int clientId,
            int ownerId,
            string ownerName,
            string clientName,
            DateTime startDate,
            DateTime endDate,
            decimal price)
        {
            Id = id;
            GameName = gameName;
            ClientId = clientId;
            OwnerId = ownerId;
            OwnerName = ownerName;
            ClientName = clientName;
            StartDate = startDate;
            EndDate = endDate;
            Price = price;
        }

        public int Id { get; set; }

        public string GameName { get; set; }

        public int ClientId { get; set; }

        public int OwnerId { get; set; }

        public string OwnerName { get; set; }

        public string ClientName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal Price { get; set; }
    }
}