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
        public void GetById_UserExists_ReturnsUser()
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
        public void GetById_UserDoesNotExist_ReturnsNull()
        {
            var userRepository = new UserRepository();
            var user = userRepository.GetById(-999);

            Assert.Null(user);
        }

        [Fact]
        public void SaveAddress_ValidAddress_UpdatesUserAddress()
        {
            int testUid = 8882;
            SetupTestUser(testUid, "TestUser2", 50.0m);
            var userRepository = new UserRepository();
            var newAddress = new Address("Moldova", "Chisinau", "Stefan cel Mare", "10");

            try
            {
                userRepository.SaveAddress(testUid, newAddress);
                var updatedUser = userRepository.GetById(testUid);

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
        public void SaveAddress_UserDoesNotExist_DoesNotThrow()
        {
            var userRepository = new UserRepository();
            var newAddress = new Address("Moldova", "Chisinau", "Stefan cel Mare", "10");

            var exception = Record.Exception(() => userRepository.SaveAddress(-999, newAddress));

            Assert.Null(exception);
        }

        [Fact]
        public void GetUserBalance_UserExists_ReturnsCorrectBalance()
        {
            int testUid = 8883;
            decimal initialBalance = 150.75m;
            SetupTestUser(testUid, "TestUser3", initialBalance);
            var userRepository = new UserRepository();

            try
            {
                decimal balance = userRepository.GetUserBalance(testUid);
                Assert.Equal(initialBalance, balance);
            }
            finally
            {
                Cleanup(testUid);
            }
        }

        [Fact]
        public void GetUserBalance_UserDoesNotExist_ReturnsZero()
        {
            var userRepository = new UserRepository();
            decimal balance = userRepository.GetUserBalance(-999);

            Assert.Equal(0m, balance);
        }

        [Fact]
        public void UpdateBalance_ValidUser_ChangesBalanceInDatabase()
        {
            int testUid = 8884;
            SetupTestUser(testUid, "TestUser4", 0.0m);
            var userRepository = new UserRepository();
            decimal updatedBalance = 99.99m;

            try
            {
                userRepository.UpdateBalance(testUid, updatedBalance);
                decimal actualBalance = userRepository.GetUserBalance(testUid);
                Assert.Equal(updatedBalance, actualBalance);
            }
            finally
            {
                Cleanup(testUid);
            }
        }

        private void SetupTestUser(int uid, string userName, decimal balance)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                new SqlCommand($"DELETE FROM [User] WHERE uid = {uid}", connection).ExecuteNonQuery();
                new SqlCommand("SET IDENTITY_INSERT [User] ON", connection).ExecuteNonQuery();

                var sqlCommand = new SqlCommand(
                    @"INSERT INTO [User] (uid, UserName, DisplayName, Country, City, Street, StreetNumber, AvatarUrl, Balance) 
                    VALUES (@uid, @un, @dn, 'Romania', 'Iasi', 'Street', '1', 'url', @balance)",
                    connection);

                sqlCommand.Parameters.AddWithValue("@uid", uid);
                sqlCommand.Parameters.AddWithValue("@un", userName);
                sqlCommand.Parameters.AddWithValue("@dn", userName + " Display");
                sqlCommand.Parameters.AddWithValue("@balance", balance);

                sqlCommand.ExecuteNonQuery();
                new SqlCommand("SET IDENTITY_INSERT [User] OFF", connection).ExecuteNonQuery();
            }
        }

        private void Cleanup(int uid)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                new SqlCommand($"DELETE FROM [User] WHERE uid = {uid}", connection).ExecuteNonQuery();
            }
        }
    }
}