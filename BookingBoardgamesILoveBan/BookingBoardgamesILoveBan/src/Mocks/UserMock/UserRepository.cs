using Microsoft.Data.SqlClient;
using BookingBoardgamesILoveBan.Src.Delivery.Model;

namespace BookingBoardgamesILoveBan.Src.Mocks.UserMock
{
    public class UserRepository : IUserRepository
    {
        private readonly string connectionString = DatabaseBootstrap.GetAppConnection();

        public User GetById(int id)
        {
            const string Query = @"SELECT uid, UserName, Country, City, Street, StreetNumber, DisplayName, AvatarUrl, Balance 
                                   FROM [User] WHERE uid = @id";
            User foundUser = null;

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(Query, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        foundUser = new User(
                            reader.GetInt32(reader.GetOrdinal("uid")),
                            reader.GetString(reader.GetOrdinal("UserName")),
                            reader.GetString(reader.GetOrdinal("DisplayName")),
                            reader.GetString(reader.GetOrdinal("Country")),
                            reader.GetString(reader.GetOrdinal("City")),
                            reader.GetString(reader.GetOrdinal("Street")),
                            reader.GetString(reader.GetOrdinal("StreetNumber")),
                            reader.GetString(reader.GetOrdinal("AvatarUrl")),
                            reader.GetDecimal(reader.GetOrdinal("Balance")));
                    }
                }
            }
            return foundUser;
        }

        public void SaveAddress(int id, Address address)
        {
            const string Query = @"UPDATE [User] SET Country = @country, City = @city, 
                                   Street = @street, StreetNumber = @streetNumber WHERE uid = @id";

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(Query, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@country", address.Country);
                command.Parameters.AddWithValue("@city", address.City);
                command.Parameters.AddWithValue("@street", address.Street);
                command.Parameters.AddWithValue("@streetNumber", address.StreetNumber);

                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Warning: No user found with ID {id}");
                }
            }
        }

        public virtual decimal GetUserBalance(int userId)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand("SELECT Balance FROM [User] WHERE uid = @userId", connection))
            {
                command.Parameters.AddWithValue("@userId", userId);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return 0m;
                    }

                    return (decimal)reader["Balance"];
                }
            }
        }

        public virtual void UpdateBalance(int userId, decimal newBalance)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand("UPDATE [User] SET Balance = @balance WHERE uid = @userId", connection))
            {
                command.Parameters.AddWithValue("@balance", newBalance);
                command.Parameters.AddWithValue("@userId", userId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}