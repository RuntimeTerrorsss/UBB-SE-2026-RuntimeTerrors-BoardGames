using System;

namespace BookingBoardgamesILoveBan.Src.Mocks.RequestMock
{
    public class Request
    {
        public Request(int id, int gameId, int clientId, int ownerId, DateTime startDate, DateTime endDate)
        {
            Id = id;
            GameId = gameId;
            ClientId = clientId;
            OwnerId = ownerId;
            StartDate = startDate;
            EndDate = endDate;
        }

        public int Id { get; set; }

        public int GameId { get; set; }

        public int ClientId { get; set; }

        public int OwnerId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}