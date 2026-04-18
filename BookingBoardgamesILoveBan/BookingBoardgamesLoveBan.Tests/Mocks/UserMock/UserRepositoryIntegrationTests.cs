using BookingBoardgamesILoveBan.Src.Delivery.Model;
using BookingBoardgamesILoveBan.Src.Mocks.UserMock;
using Microsoft.Data.SqlClient;
using Xunit;

namespace BookingBoardgamesLoveBan.Tests.Mocks.UserMock
{
    public class UserRepositoryIntegrationTests
    {
        private readonly string connectionString;

        public UserRepositoryIntegrationTests()
        {
            DatabaseBootstrap.Initialize();
            connectionString = DatabaseBootstrap.GetAppConnection();
        }

        [Fact]
        public void GetById_ReturnsUser_WhenExists()
        {
            int testUid = 8881;
            SetupTestUser(testUid, "TestUser1", 100.0m);
            var service = new UserRepository();

            try
            {
                var user = service.GetById(testUid);

                Assert.NotNull(user);
                Assert.Equal(
                    new { Id = testUid, Username = "TestUser1" },
                    new { user.Id, user.Username });
            }
            finally
            {
                Cleanup(testUid);
            }
        }

        [Fact]
        public void SaveAddress_UpdatesUserAddressCorrectly()
        {
            int testUid = 8882;
            SetupTestUser(testUid, "TestUser2", 50.0m);
            var service = new UserRepository();
            var newAddress = new Address("Moldova", "Chisinau", "Stefan cel Mare", "10");

            try
            {
                service.SaveAddress(testUid, newAddress);
                var updatedUser = service.GetById(testUid);

                Assert.Equal(
                    new { newAddress.Country, newAddress.City, newAddress.Street, newAddress.StreetNumber },
                    new { updatedUser.Country, updatedUser.City, updatedUser.Street, updatedUser.StreetNumber });
            }
            finally
            {
                Cleanup(testUid);
            }
        }

        [Fact]
        public void GetUserBalance_ReturnsCorrectBalance()
        {
            int testUid = 8883;
            decimal initialBalance = 150.75m;
            SetupTestUser(testUid, "TestUser3", initialBalance);
            var service = new UserRepository();

            try
            {
                decimal balance = service.GetUserBalance(testUid);
                Assert.Equal(initialBalance, balance);
            }
            finally
            {
                Cleanup(testUid);
            }
        }

        [Fact]
        public void UpdateBalance_ChangesBalanceInDatabase()
        {
            int testUid = 8884;
            SetupTestUser(testUid, "TestUser4", 0.0m);
            var service = new UserRepository();
            decimal updatedBalance = 99.99m;

            try
            {
                service.UpdateBalance(testUid, updatedBalance);
                decimal actualBalance = service.GetUserBalance(testUid);
                Assert.Equal(updatedBalance, actualBalance);
            }
            finally
            {
                Cleanup(testUid);
            }
        }

        private void SetupTestUser(int uid, string userName, decimal balance)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                new SqlCommand($"DELETE FROM [User] WHERE uid = {uid}", conn).ExecuteNonQuery();
                new SqlCommand("SET IDENTITY_INSERT [User] ON", conn).ExecuteNonQuery();

                var cmd = new SqlCommand(
                    @"INSERT INTO [User] (uid, UserName, DisplayName, Country, City, Street, StreetNumber, AvatarUrl, Balance) 
                    VALUES (@uid, @un, @dn, 'Romania', 'Iasi', 'Street', '1', 'url', @balance)",
                    conn);

                cmd.Parameters.AddWithValue("@uid", uid);
                cmd.Parameters.AddWithValue("@un", userName);
                cmd.Parameters.AddWithValue("@dn", userName + " Display");
                cmd.Parameters.AddWithValue("@balance", balance);

                cmd.ExecuteNonQuery();
                new SqlCommand("SET IDENTITY_INSERT [User] OFF", conn).ExecuteNonQuery();
            }
        }

        private void Cleanup(int uid)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                new SqlCommand($"DELETE FROM [User] WHERE uid = {uid}", conn).ExecuteNonQuery();
            }
        }
    }
}