using BookingBoardgamesILoveBan.Src.Mocks.RequestMock;
using BookingBoardgamesILoveBan.Src.Mocks.GameMock;
using Microsoft.Data.SqlClient;
using Xunit;

namespace BookingBoardgamesLoveBan.Tests.Mocks.RequestMock
{
    public class RequestServiceIntegrationTests
    {
        private readonly string connectionString;

        public RequestServiceIntegrationTests()
        {
            DatabaseBootstrap.Initialize();
            connectionString = DatabaseBootstrap.GetAppConnection();
        }

        #region Helper Functions
        private void SetupTestGame(int gid, decimal price)
        {
            using var conn = new SqlConnection(connectionString);
            conn.Open();
            new SqlCommand($"DELETE FROM Request WHERE GameId = {gid}", conn).ExecuteNonQuery();
            new SqlCommand($"DELETE FROM Game WHERE gid = {gid}", conn).ExecuteNonQuery();

            new SqlCommand("SET IDENTITY_INSERT Game ON", conn).ExecuteNonQuery();
            var cmd = new SqlCommand("INSERT INTO Game (gid, Name, PricePerDay) VALUES (@id, 'Test Game', @price)", conn);
            cmd.Parameters.AddWithValue("@id", gid);
            cmd.Parameters.AddWithValue("@price", price);
            cmd.ExecuteNonQuery();
            new SqlCommand("SET IDENTITY_INSERT Game OFF", conn).ExecuteNonQuery();
        }

        private void SetupTestRequest(int rid, int gid, DateTime start, DateTime end)
        {
            using var conn = new SqlConnection(connectionString);
            conn.Open();
            new SqlCommand($"DELETE FROM Request WHERE rid = {rid}", conn).ExecuteNonQuery();

            new SqlCommand("SET IDENTITY_INSERT Request ON", conn).ExecuteNonQuery();
            var cmd = new SqlCommand(@"
                INSERT INTO Request (rid, GameId, ClientId, OwnerId, StartDate, EndDate) 
                VALUES (@rid, @gid, 0, 0, @start, @end)", conn);
            cmd.Parameters.AddWithValue("@rid", rid);
            cmd.Parameters.AddWithValue("@gid", gid);
            cmd.Parameters.AddWithValue("@start", start);
            cmd.Parameters.AddWithValue("@end", end);
            cmd.ExecuteNonQuery();
            new SqlCommand("SET IDENTITY_INSERT Request OFF", conn).ExecuteNonQuery();
        }

        private void Cleanup(int rid, int gid)
        {
            using var conn = new SqlConnection(connectionString);
            conn.Open();
            new SqlCommand($"DELETE FROM Request WHERE rid = {rid}", conn).ExecuteNonQuery();
            new SqlCommand($"DELETE FROM Game WHERE gid = {gid}", conn).ExecuteNonQuery();
        }
        #endregion

        [Fact]
        public void GetById_ReturnsRequest_WhenExists()
        {
            int gid = 1334;
            int rid = 1335;
            SetupTestGame(gid, 25.50m);
            SetupTestRequest(rid, gid, new DateTime(2023, 10, 01), new DateTime(2023, 10, 06));

            var reqService = new RequestService(new GameService());

            try
            {
                var request = reqService.GetById(rid);
                Assert.NotNull(request);
                Assert.Equal(rid, request.Id); 
            }
            finally
            {
                Cleanup(rid, gid);
            }
        }

        [Fact]
        public void GetRequestPrice_CalculatesCorrectTotal()
        {
            int gid = 2000;
            int rid = 2001;
            decimal pricePerDay = 20.0m;

            DateTime start = new DateTime(2026, 01, 01);
            DateTime end = new DateTime(2026, 01, 06);
            decimal expectedTotal = 100.0m; 

            SetupTestGame(gid, pricePerDay);
            SetupTestRequest(rid, gid, start, end);

            var reqService = new RequestService(new GameService());

            try
            {
                decimal actualPrice = reqService.GetRequestPrice(rid);

                Assert.Equal(expectedTotal, actualPrice);
            }
            finally
            {
                Cleanup(rid, gid);
            }
        }

        [Fact]
        public void GetGameName_ReturnsCorrectNameFromGameService()
        {
            int gid = 3000;
            int rid = 3001;
            SetupTestGame(gid, 15.0m);
            SetupTestRequest(rid, gid, DateTime.Now, DateTime.Now.AddDays(1));

            var reqService = new RequestService(new GameService());

            try
            {
                string gameName = reqService.GetGameName(rid);

                Assert.Equal("Test Game", gameName);
            }
            finally
            {
                Cleanup(rid, gid);
            }
        }
    }
}