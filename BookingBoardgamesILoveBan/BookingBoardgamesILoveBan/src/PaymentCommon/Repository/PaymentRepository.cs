using System;
using System.Collections.Generic;
using BookingBoardgamesILoveBan.Src.PaymentCommon.Model;
using Microsoft.Data.SqlClient;

namespace BookingBoardgamesILoveBan.Src.PaymentCommon.Repository
{
	public class PaymentRepository : IPaymentRepository
	{
        private const string SelectAllPaymentsQuery = "SELECT * FROM [Payment]";
        private const string SelectPaymentByIdentifierQuery = "SELECT * FROM [Payment] WHERE tid = @PaymentId";
        private const string DeletePaymentByIdentifierQuery = "DELETE FROM [Payment] WHERE tid = @PaymentId";
        private const string PaymentIdParameterName = "@PaymentId";
        private const string PaymentIdColumnName = "tid";
        private static readonly string ConnectionString = DatabaseBootstrap.GetAppConnection();

        public IReadOnlyList<Payment> GetAll()
        {
            var payments = new List<Payment>();

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                var command = new SqlCommand(SelectAllPaymentsQuery, connection);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    payments.Add(new Payment
                    {
                        TransactionId = (int)reader[PaymentIdColumnName],
                        RequestId = (int)reader["RequestId"],
                        ClientId = (int)reader["ClientId"],
                        OwnerId = (int)reader["OwnerId"],
                        Amount = (decimal)reader["Amount"],
                        PaymentMethod = (string)reader["PaymentMethod"],
                    });
                }
            }

            return payments;
        }

        public virtual Payment GetById(int paymentId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                var getCommand = connection.CreateCommand();
                getCommand.CommandText = SelectPaymentByIdentifierQuery;
                getCommand.Parameters.AddWithValue(PaymentIdParameterName, paymentId);
                using var reader = getCommand.ExecuteReader();
                if (!reader.Read())
                {
                    return null;
                }
                return new Payment
                {
                    TransactionId = (int)reader[PaymentIdColumnName],
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

        public virtual int AddPayment(Payment payment)
        {
            using (var connection = new SqlConnection(ConnectionString))
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
                createCommand.Parameters.AddWithValue("@RequestId", payment.RequestId);
                createCommand.Parameters.AddWithValue("@ClientId", payment.ClientId);
                createCommand.Parameters.AddWithValue("@OwnerId", payment.OwnerId);
                createCommand.Parameters.AddWithValue("@Amount", payment.Amount);
                createCommand.Parameters.AddWithValue("@PaymentMethod", payment.PaymentMethod);
                createCommand.Parameters.AddWithValue("@State", payment.State);
                createCommand.Parameters.AddWithValue("@DateOfTransaction",
                    (object?)payment.DateOfTransaction ?? DateTime.Now);
				createCommand.Parameters.AddWithValue("@DateConfirmedBuyer",
                    (object?)payment.DateConfirmedBuyer ?? DBNull.Value);
				createCommand.Parameters.AddWithValue("@DateConfirmedSeller",
                    (object?)payment.DateConfirmedSeller ?? DBNull.Value);
				createCommand.Parameters.AddWithValue("@FilePath",
                    (object?)payment.FilePath ?? string.Empty);
                var result = createCommand.ExecuteScalar();
                return (int)result;
            }
        }
        public bool DeletePayment(Payment payment)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                var command = new SqlCommand(DeletePaymentByIdentifierQuery, connection);
                command.Parameters.AddWithValue(PaymentIdParameterName, payment.TransactionId);
                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
        public virtual Payment UpdatePayment(Payment payment)
        {
            Payment previousPayment = GetById(payment.TransactionId);

            using (var connection = new SqlConnection(DatabaseBootstrap.GetAppConnection()))
            {
                connection.Open();
                var command = new SqlCommand(@"
                        UPDATE [Payment]
                        SET FilePath = @FilePath,  DateOfTransaction = @DateOfTransaction, DateConfirmedBuyer=@DateConfirmedBuyer, DateConfirmedSeller=@DateConfirmedSeller
                        WHERE tid = @PaymentId", connection);

                command.Parameters.AddWithValue("@FilePath",
                    (object?)payment.FilePath ?? string.Empty);
                command.Parameters.AddWithValue("@DateOfTransaction",
                    (object?)payment.DateOfTransaction ?? DateTime.Now);
                command.Parameters.AddWithValue("@DateConfirmedBuyer",
                    (object?)payment.DateConfirmedBuyer ?? DBNull.Value);
                command.Parameters.AddWithValue("@DateConfirmedSeller",
                    (object?)payment.DateConfirmedSeller ?? DBNull.Value);
                command.Parameters.AddWithValue(PaymentIdParameterName, payment.TransactionId);
                command.ExecuteNonQuery();
            }

            return previousPayment;
        }
    }
}
