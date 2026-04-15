using System;
using BookingBoardgamesILoveBan.Src.Mocks.GameMock;
using Microsoft.Data.SqlClient;

namespace BookingBoardgamesILoveBan.Src.Mocks.RequestMock
{
	public class RequestService : IRequestService
	{
		private readonly string connectionString = DatabaseBootstrap.GetAppConnection();
		private readonly IGameService gameService;

        public RequestService(IGameService gameservice)
        {
			gameService = gameservice;
        }
        public Request GetById(int id)
		{
			const string query = @"SELECT rid, GameId, ClientId, OwnerId, StartDate, EndDate FROM Request WHERE rid = @id";
			Request foundRequest = null;

			using (var connection = new SqlConnection(this.connectionString))
			{
				using (var command = new SqlCommand(query, connection))
				{
					command.Parameters.AddWithValue("@id", id);

					connection.Open();

					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							foundRequest = new Request(
								reader.GetInt32(reader.GetOrdinal("rid")),
								reader.GetInt32(reader.GetOrdinal("GameId")),
								reader.GetInt32(reader.GetOrdinal("ClientId")),
								reader.GetInt32(reader.GetOrdinal("OwnerId")),
								reader.GetDateTime(reader.GetOrdinal("StartDate")),
								reader.GetDateTime(reader.GetOrdinal("EndDate")));
						}
					}

					connection.Close();
				}
			}

			return foundRequest;
		}

        public virtual decimal GetRequestPrice(int requestId)
        {
            Request request = this.GetById(requestId);
            int daysOfBooking = (request.EndDate - request.StartDate).Days;
            decimal gamePricePerDay = gameService.GetPriceGameById(request.GameId);
            return gamePricePerDay * daysOfBooking;
        }

		public string GetGameName(int requestId)
		{
			Request request = this.GetById(requestId);
			Game game = this.gameService.GetById(request.GameId);
			return game.Name;
        }
    }
}
