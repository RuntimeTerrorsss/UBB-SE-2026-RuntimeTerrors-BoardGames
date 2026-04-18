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
        public void GetById_ReturnsRequest_WhenExists()
        {
            int gid = 1334;
            int rid = 1335;
            var start = new DateTime(2023, 10, 01);
            var end = new DateTime(2023, 10, 06);

            try
            {
                // Setup Game
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    new SqlCommand($"DELETE FROM Request WHERE GameId = {gid}", conn).ExecuteNonQuery();
                    new SqlCommand($"DELETE FROM Game WHERE gid = {gid}", conn).ExecuteNonQuery();
                    new SqlCommand("SET IDENTITY_INSERT Game ON", conn).ExecuteNonQuery();

                    var cmd = new SqlCommand("INSERT INTO Game (gid, Name, PricePerDay) VALUES (@id, 'Test Game', @price)", conn);
                    cmd.Parameters.AddWithValue("@id", gid);
                    cmd.Parameters.AddWithValue("@price", 25.50m);
                    cmd.ExecuteNonQuery();

                    new SqlCommand("SET IDENTITY_INSERT Game OFF", conn).ExecuteNonQuery();
                }

                // Setup Request
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    new SqlCommand($"DELETE FROM Request WHERE rid = {rid}", conn).ExecuteNonQuery();
                    new SqlCommand("SET IDENTITY_INSERT Request ON", conn).ExecuteNonQuery();

                    var cmd = new SqlCommand(
                        @"INSERT INTO Request (rid, GameId, ClientId, OwnerId, StartDate, EndDate) 
                        VALUES (@rid, @gid, 0, 0, @start, @end)",
                        conn);
                    cmd.Parameters.AddWithValue("@rid", rid);
                    cmd.Parameters.AddWithValue("@gid", gid);
                    cmd.Parameters.AddWithValue("@start", start);
                    cmd.Parameters.AddWithValue("@end", end);
                    cmd.ExecuteNonQuery();

                    new SqlCommand("SET IDENTITY_INSERT Request OFF", conn).ExecuteNonQuery();
                }

                var request = new RequestRepository().GetById(rid);

                Assert.NotNull(request);
                Assert.Equal(
                    new { Id = rid, GameId = gid, StartDate = start, EndDate = end },
                    new { request.Id, request.GameId, request.StartDate, request.EndDate });
            }
            finally
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    new SqlCommand($"DELETE FROM Request WHERE rid = {rid}", conn).ExecuteNonQuery();
                    new SqlCommand($"DELETE FROM Game WHERE gid = {gid}", conn).ExecuteNonQuery();
                }
            }
        }
    }
}