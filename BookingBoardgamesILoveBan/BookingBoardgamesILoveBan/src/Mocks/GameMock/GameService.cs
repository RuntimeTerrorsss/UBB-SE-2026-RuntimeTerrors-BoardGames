using Microsoft.Data.SqlClient;

namespace BookingBoardgamesILoveBan.src.Mocks.GameMock
{
	public class GameService
	{
		private readonly string _connectionString = DatabaseBootstrap.GetAppConnection();

		public Game GetById(int id) {
            const string query = @"SELECT gid, Name, PricePerDay FROM Game WHERE gid = @id";
            Game foundGame = null;

			using (var connection = new SqlConnection(this._connectionString)) {
				using (var command = new SqlCommand(query, connection)) {
					command.Parameters.AddWithValue("@id", id);

					connection.Open();

					using (var reader = command.ExecuteReader()) {
						while (reader.Read()) {
							foundGame = new Game(
								reader.GetInt32(reader.GetOrdinal("gid")),
								reader.GetString(reader.GetOrdinal("Name")),
								reader.GetDecimal(reader.GetOrdinal("PricePerDay"))
							);
						}
					}

					connection.Close();
				}
			}

			return foundGame;
		}
        public decimal getPriceGameById(int gameId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SqlCommand(
                    "SELECT PricePerDay FROM [Game] WHERE gid = @gameId", connection);
                cmd.Parameters.AddWithValue("@gameId", gameId);

                using var reader = cmd.ExecuteReader();
                if (!reader.Read()) return 0;

                return (decimal)reader["PricePerDay"];

            }
        }
    }

}
