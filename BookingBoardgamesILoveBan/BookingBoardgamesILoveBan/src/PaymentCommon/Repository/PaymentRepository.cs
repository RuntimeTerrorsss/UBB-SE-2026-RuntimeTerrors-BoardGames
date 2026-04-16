using System;
using System.Collections.Generic;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using Microsoft.Data.SqlClient;

namespace BookingBoardgamesILoveBan.Src.PaymentCommon.Repository
{
	public class PaymentRepository : IPaymentRepository
	{
        private static string connectionString = DatabaseBootstrap.GetAppConnection();

        public IReadOnlyList<Model.Payment> GetAll()
        {
            var transactions = new List<Model.Payment>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new SqlCommand("SELECT * FROM [Payment]", connection);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    transactions.Add(new Payment
                    {
                        Tid = (int)reader["tid"],
                        RequestId = (int)reader["RequestId"],
                        ClientId = (int)reader["ClientId"],
                        OwnerId = (int)reader["OwnerId"],
                        Amount = (decimal)reader["Amount"],
                        PaymentMethod = (string)reader["PaymentMethod"],
                    });
                }
            }

            return transactions;
        }

        public virtual Model.Payment GetById(int tid)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var getCommand = connection.CreateCommand();
                getCommand.CommandText = "SELECT * FROM [Payment] WHERE tid = @Tid";
                getCommand.Parameters.AddWithValue("@Tid", tid);
                using var reader = getCommand.ExecuteReader();
                if (!reader.Read())
                {
                    return null;
                }
                return new Model.Payment
                {
                    Tid = (int)reader["tid"],
                    RequestId = (int)reader["RequestId"],
                    ClientId = (int)reader["ClientId"],
                    OwnerId = (int)reader["OwnerId"],
                    Amount = (decimal)reader["Amount"],
                    DateOfTransaction = reader["DateOfTransaction"] == DBNull.Value ? null : (global::System.DateTime)reader["DateOfTransaction"],
                    PaymentMethod = reader["PaymentMethod"].ToString(),

                    DateConfirmedBuyer = reader["DateConfirmedBuyer"] == DBNull.Value ? null : (global::System.DateTime)reader["DateConfirmedBuyer"],
                    DateConfirmedSeller = reader["DateConfirmedSeller"] == DBNull.Value ? null : (global::System.DateTime)reader["DateConfirmedSeller"],

                    State = (int)reader["state"],
                    FilePath = reader["FilePath"] == DBNull.Value ? null : reader["FilePath"].ToString()
                };
            }
        }

        public virtual int AddPayment(Model.Payment transaction)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var createCommand = connection.CreateCommand();
                createCommand.CommandText = @"
                INSERT INTO [Payment] (RequestId, ClientId, OwnerId, Amount,
                    DateOfTransaction, DateConfirmedBuyer, DateConfirmedSeller, PaymentMethod, State, FilePath)
                OUTPUT INSERTED.Tid
                VALUES
                    (@RequestId, @ClientId, @OwnerId, @Amount,
                     @DateOfTransaction, @DateConfirmedBuyer, @DateConfirmedSeller, @PaymentMethod, @State, @FilePath)";
                createCommand.Parameters.AddWithValue("@RequestId", transaction.RequestId);
                createCommand.Parameters.AddWithValue("@ClientId", transaction.ClientId);
                createCommand.Parameters.AddWithValue("@OwnerId", transaction.OwnerId);
                createCommand.Parameters.AddWithValue("@Amount", transaction.Amount);
                createCommand.Parameters.AddWithValue("@PaymentMethod", transaction.PaymentMethod);
                createCommand.Parameters.AddWithValue("@State", transaction.State);
                createCommand.Parameters.AddWithValue("@DateOfTransaction",
                    (object?)transaction.DateOfTransaction ?? DateTime.Now);
				createCommand.Parameters.AddWithValue("@DateConfirmedBuyer",
                    (object?)transaction.DateConfirmedBuyer ?? DBNull.Value);
				createCommand.Parameters.AddWithValue("@DateConfirmedSeller",
                    (object?)transaction.DateConfirmedSeller ?? DBNull.Value);
				createCommand.Parameters.AddWithValue("@FilePath",
                    (object?)transaction.FilePath ?? string.Empty);
                var result = createCommand.ExecuteScalar();
                return (int)result;
            }
        }
        public bool DeletePayment(Model.Payment transaction)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new SqlCommand("DELETE FROM [Payment] WHERE tid = @Tid", connection);
                cmd.Parameters.AddWithValue("@Tid", transaction.Tid);
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
        public virtual Model.Payment UpdatePayment(Model.Payment transaction)
        {
            Model.Payment oldTransaction = GetById(transaction.Tid);

            using (var connection = new SqlConnection(DatabaseBootstrap.GetAppConnection()))
            {
                connection.Open();
                var cmd = new SqlCommand(@"
                        UPDATE [Payment]
                        SET FilePath = @FilePath,  DateOfTransaction = @DateOfTransaction, DateConfirmedBuyer=@DateConfirmedBuyer, DateConfirmedSeller=@DateConfirmedSeller
                        WHERE tid = @Tid", connection);

                cmd.Parameters.AddWithValue("@FilePath",
                    (object?)transaction.FilePath ?? string.Empty);
                cmd.Parameters.AddWithValue("@DateOfTransaction",
                    (object?)transaction.DateOfTransaction ?? DateTime.Now);
                cmd.Parameters.AddWithValue("@DateConfirmedBuyer",
                    (object?)transaction.DateConfirmedBuyer ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DateConfirmedSeller",
                    (object?)transaction.DateConfirmedSeller ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Tid", transaction.Tid);
                cmd.ExecuteNonQuery();
            }

            return oldTransaction;
        }
    }
}
