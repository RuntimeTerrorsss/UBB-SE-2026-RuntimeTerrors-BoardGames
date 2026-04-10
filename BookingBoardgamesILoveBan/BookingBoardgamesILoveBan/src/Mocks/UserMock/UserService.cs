using System;
using BookingBoardgamesILoveBan.Src.Delivery.Model;
using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml;
namespace BookingBoardgamesILoveBan.Src.Mocks.UserMock
{
	public class UserService
	{
		private readonly string connectionString = DatabaseBootstrap.GetAppConnection();

		public User GetById(int id)
		{
			const string query = @"SELECT uid, UserName, Country, City, Street, StreetNumber, DisplayName, AvatarUrl, Balance FROM [User] WHERE uid = @id";
			User foundUser = null;

			using (var connection = new SqlConnection(this.connectionString))
			{
				using (var command = new SqlCommand(query, connection))
				{
					command.Parameters.AddWithValue("@id", id);

					connection.Open();

					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							foundUser = new User(reader.GetInt32(
								reader.GetOrdinal("uid")),
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

					connection.Close();
				}
			}

			return foundUser;
		}

        public void SaveAddress(int id, Address address)
        {
			string country = address.Country;
			string city = address.City;
			string street = address.Street;
			string streetNumber = address.StreetNumber;
			const string query = @"update [User] set Country = @country, City = @city, Street = @street, StreetNumber = @streetNumber  where uid = @id";

			using (var newConnection = new SqlConnection(this.connectionString))
			{
				using (var command = new SqlCommand(query, newConnection))
				{
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@country", country);
                    command.Parameters.AddWithValue("@city", city);
                    command.Parameters.AddWithValue("@street", street);
                    command.Parameters.AddWithValue("@streetNumber", streetNumber);
                    newConnection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning: No user found with ID {id}");
                    }
                }
			}
        }

        public decimal GetUserBalance(int userId)
        {
            User user = this.GetById(userId);
            return user.Balance;
        }

        public void UpdateBalance(int userId, decimal newBalance)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new SqlCommand(
                    "UPDATE [User] SET Balance = @Balance WHERE Uid = @Uid", connection);
                cmd.Parameters.AddWithValue("@Balance", newBalance);
                cmd.Parameters.AddWithValue("@Uid", userId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
