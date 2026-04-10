using System;

namespace BookingBoardgamesILoveBan.Src.Mocks.RequestMock
{
	public class Request
	{
		public int Id { get; set; }
		public int GameId { get; set; }
		public int ClientId { get; set; }
		public int OwnerId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		public Request(int id, int gameId, int clientId, int ownerId, DateTime startDate, DateTime endDate)
		{
			this.Id = id;
			this.GameId = gameId;
			this.ClientId = clientId;
			this.OwnerId = ownerId;
			this.StartDate = startDate;
			this.EndDate = endDate;
		}
	}
}
