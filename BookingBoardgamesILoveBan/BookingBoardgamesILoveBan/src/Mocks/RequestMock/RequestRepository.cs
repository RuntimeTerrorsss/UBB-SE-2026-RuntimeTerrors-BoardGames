using System;
using BookingBoardgamesILoveBan.Src.Mocks.GameMock;
using Microsoft.Data.SqlClient;

namespace BookingBoardgamesILoveBan.Src.Mocks.RequestMock
{
    public class RequestRepository : IRequestRepository
    {
        private readonly string connectionString = DatabaseBootstrap.GetAppConnection();
        private readonly IGameRepository gameRepository;

        public RequestRepository()
        {
        }

        public Request GetById(int id)
        {
            const string Query = @"SELECT rid, GameId, ClientId, OwnerId, StartDate, EndDate FROM Request WHERE rid = @id";
            Request foundRequest = null;

            using (var connection = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand(Query, connection))
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
    }
}