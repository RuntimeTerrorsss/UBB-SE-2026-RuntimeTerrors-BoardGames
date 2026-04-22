using System;
using BookingBoardgamesILoveBan.Src.Mocks.GameMock;
using Microsoft.Data.SqlClient;
using Xunit;

public class GameRepositoryIntegrationTests
{
    private readonly string connectionString;

    public GameRepositoryIntegrationTests()
    {
        DatabaseBootstrap.Initialize();
        connectionString = DatabaseBootstrap.GetAppConnection();

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new Exception("Connection string is null!");
        }
    }

    [Fact]
    public void GetById_GameExists_ReturnsGame()
    {
        int testId = 12344;

        try
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                var deleteOld = new SqlCommand("DELETE FROM Game WHERE gid = @id", conn);
                deleteOld.Parameters.AddWithValue("@id", testId);
                deleteOld.ExecuteNonQuery();

                new SqlCommand("SET IDENTITY_INSERT Game ON", conn).ExecuteNonQuery();
                var insert = new SqlCommand(
                    "INSERT INTO Game (gid, Name, PricePerDay) VALUES (@id, 'TestGame', 15)", conn);
                insert.Parameters.AddWithValue("@id", testId);
                insert.ExecuteNonQuery();
                new SqlCommand("SET IDENTITY_INSERT Game OFF", conn).ExecuteNonQuery();
            }

            var service = new GameRepository();
            var game = service.GetById(testId);

            Assert.NotNull(game);
            Assert.Equal(
                new { Id = testId, Name = "TestGame", PricePerDay = (decimal)15 },
                new { game.Id, game.Name, game.PricePerDay });
        }
        finally
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var delete = new SqlCommand("DELETE FROM Game WHERE gid = @id", conn);
                delete.Parameters.AddWithValue("@id", testId);
                delete.ExecuteNonQuery();
            }
        }
    }

    [Fact]
    public void GetById_GameDoesNotExist_ReturnsNull()
    {
        var service = new GameRepository();
        var game = service.GetById(-999);

        Assert.Null(game);
    }

    [Fact]
    public void GetPriceGameById_GameExists_ReturnsCorrectPrice()
    {
        int testId = 12345;
        decimal expectedPrice = 25.50m;

        try
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                var deleteOld = new SqlCommand("DELETE FROM Game WHERE gid = @id", conn);
                deleteOld.Parameters.AddWithValue("@id", testId);
                deleteOld.ExecuteNonQuery();

                new SqlCommand("SET IDENTITY_INSERT Game ON", conn).ExecuteNonQuery();
                var insert = new SqlCommand(
                    "INSERT INTO Game (gid, Name, PricePerDay) VALUES (@id, 'PriceTestGame', @price)", conn);
                insert.Parameters.AddWithValue("@id", testId);
                insert.Parameters.AddWithValue("@price", expectedPrice);
                insert.ExecuteNonQuery();
                new SqlCommand("SET IDENTITY_INSERT Game OFF", conn).ExecuteNonQuery();
            }

            var service = new GameRepository();

            decimal actualPrice = service.GetPriceGameById(testId);

            Assert.Equal(expectedPrice, actualPrice);
        }
        finally
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var delete = new SqlCommand("DELETE FROM Game WHERE gid = @id", conn);
                delete.Parameters.AddWithValue("@id", testId);
                delete.ExecuteNonQuery();
            }
        }
    }

    [Fact]
    public void GetPriceGameById_GameDoesNotExist_ReturnsZero()
    {
        var service = new GameRepository();
        var price = service.GetPriceGameById(-999);

        Assert.Equal(0m, price);
    }
}