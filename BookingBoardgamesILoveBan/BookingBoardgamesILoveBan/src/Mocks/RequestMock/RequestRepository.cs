using System;
using Microsoft.Data.SqlClient;
using BookingBoardgamesILoveBan.Src.Mocks.GameMock;

namespace BookingBoardgamesILoveBan.Src.Mocks.RequestMock
{
    public class RequestRepository : IRequestRepository
    {
        private readonly string connectionString = DatabaseBootstrap.GetAppConnection();

        // Constructorul trebuie să primească dependințele
        public RequestRepository()
        {
        }

        public Request GetById(int id)
        {
            const string Query = "SELECT rid, GameId, ClientId, OwnerId, StartDate, EndDate FROM Request WHERE rid = @id";
            Request foundRequest = null;

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(Query, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
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
            }

            return foundRequest;
        }
    }
}