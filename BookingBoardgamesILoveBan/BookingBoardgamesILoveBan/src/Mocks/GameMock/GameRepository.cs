using Microsoft.Data.SqlClient;

namespace BookingBoardgamesILoveBan.Src.Mocks.GameMock
{
    public class GameRepository : IGameRepository
    {
        private readonly string connectionString = DatabaseBootstrap.GetAppConnection();

        private const string GetGameByIdQuery = "SELECT gid, Name, PricePerDay FROM Game WHERE gid = @id";
        private const string GetPriceByIdQuery = "SELECT PricePerDay FROM Game WHERE gid = @id";

        public Game GetById(int id)
        {
            Game foundGame = null;

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(GetGameByIdQuery, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        foundGame = new Game(
                            reader.GetInt32(reader.GetOrdinal("gid")),
                            reader.GetString(reader.GetOrdinal("Name")),
                            reader.GetDecimal(reader.GetOrdinal("PricePerDay")));
                    }
                }
            }

            return foundGame;
        }

        public decimal GetPriceGameById(int gameId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand("SELECT PricePerDay FROM [Game] WHERE gid = @gameId", connection);
                command.Parameters.AddWithValue("@gameId", gameId);

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return 0m;
                    }

                    return (decimal)reader["PricePerDay"];
                }
            }
        }
    }
}