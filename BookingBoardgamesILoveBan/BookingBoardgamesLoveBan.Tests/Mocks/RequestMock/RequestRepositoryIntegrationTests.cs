using System;
using BookingBoardgamesILoveBan.Src.Mocks.GameMock;
using BookingBoardgamesILoveBan.Src.Mocks.RequestMock;
using Microsoft.Data.SqlClient;
using Xunit;

namespace BookingBoardgamesLoveBan.Tests.Mocks.RequestMock
{
    public class RequestRepositoryIntegrationTests
    {
        private readonly string connectionString;

        public RequestRepositoryIntegrationTests()
        {
            DatabaseBootstrap.Initialize();
            connectionString = DatabaseBootstrap.GetAppConnection();
        }

        [Fact]
        public void GetById_RequestExists_ReturnsRequest()
        {
            int gid = 1334;
            int rid = 1335;
            var start = new DateTime(2023, 10, 01);
            var end = new DateTime(2023, 10, 06);

            try
            {
                // Setup Game
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    new SqlCommand($"DELETE FROM Request WHERE GameId = {gid}", connection).ExecuteNonQuery();
                    new SqlCommand($"DELETE FROM Game WHERE gid = {gid}", connection).ExecuteNonQuery();
                    new SqlCommand("SET IDENTITY_INSERT Game ON", connection).ExecuteNonQuery();

                    var sqlCommand = new SqlCommand("INSERT INTO Game (gid, Name, PricePerDay) VALUES (@id, 'Test Game', @price)", connection);
                    sqlCommand.Parameters.AddWithValue("@id", gid);
                    sqlCommand.Parameters.AddWithValue("@price", 25.50m);
                    sqlCommand.ExecuteNonQuery();

                    new SqlCommand("SET IDENTITY_INSERT Game OFF", connection).ExecuteNonQuery();
                }

                // Setup Request
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    new SqlCommand($"DELETE FROM Request WHERE rid = {rid}", connection).ExecuteNonQuery();
                    new SqlCommand("SET IDENTITY_INSERT Request ON", connection).ExecuteNonQuery();

                    var sqlCommand = new SqlCommand(
                        @"INSERT INTO Request (rid, GameId, ClientId, OwnerId, StartDate, EndDate) 
                        VALUES (@rid, @gid, 0, 0, @start, @end)",
                        connection);
                    sqlCommand.Parameters.AddWithValue("@rid", rid);
                    sqlCommand.Parameters.AddWithValue("@gid", gid);
                    sqlCommand.Parameters.AddWithValue("@start", start);
                    sqlCommand.Parameters.AddWithValue("@end", end);
                    sqlCommand.ExecuteNonQuery();

                    new SqlCommand("SET IDENTITY_INSERT Request OFF", connection).ExecuteNonQuery();
                }

                var requestRepository = new RequestRepository().GetById(rid);

                Assert.NotNull(requestRepository);
                Assert.Equal(
                    new { Id = rid, GameId = gid, StartDate = start, EndDate = end },
                    new { requestRepository.Id, requestRepository.GameId, requestRepository.StartDate, requestRepository.EndDate });
            }
            finally
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    new SqlCommand($"DELETE FROM Request WHERE rid = {rid}", connection).ExecuteNonQuery();
                    new SqlCommand($"DELETE FROM Game WHERE gid = {gid}", connection).ExecuteNonQuery();
                }
            }
        }

        [Fact]
        public void GetById_RequestDoesNotExist_ReturnsNull()
        {
            var requestRepository = new RequestRepository();
            var request = requestRepository.GetById(-999);

            Assert.Null(request);
        }
    }
}