using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Model;

namespace BookingBoardgamesILoveBan.Src.PaymentHistory.Repository
{
    public class RepositoryPayment : IRepositoryPayment
    {
        private readonly string connectionString;

        public RepositoryPayment()
        {
            connectionString = DatabaseBootstrap.GetAppConnection();
        }

        private HistoryPayment GetPaymentFromReader(SqlDataReader reader)
        {
            int tidIndex = reader.GetOrdinal("tid");
            int requestIdIndex = reader.GetOrdinal("RequestId");
            int renterIdIndex = reader.GetOrdinal("ClientId");
            int ownerIdIndex = reader.GetOrdinal("OwnerId");
            int methodIndex = reader.GetOrdinal("PaymentMethod");
            int amountIndex = reader.GetOrdinal("Amount");
            int dateOfTransactionIndex = reader.GetOrdinal("DateOfTransaction");
            int dateConfirmedBuyerIndex = reader.GetOrdinal("DateConfirmedBuyer");
            int dateConfirmedSellerIndex = reader.GetOrdinal("DateConfirmedSeller");
            int filePathIndex = reader.GetOrdinal("FilePath");
            int gameNameIndex = reader.GetOrdinal("GameName");
            int ownerNameIndex = reader.GetOrdinal("OwnerName");

            var returnedPayment = new HistoryPayment(
                id: reader.GetInt32(tidIndex),
                requestId: reader.IsDBNull(requestIdIndex) ? 0 : reader.GetInt32(requestIdIndex),
                renterId: reader.IsDBNull(renterIdIndex) ? 0 : reader.GetInt32(renterIdIndex),
                ownerId: reader.IsDBNull(ownerIdIndex) ? 0 : reader.GetInt32(ownerIdIndex),
                method: reader.IsDBNull(methodIndex) ? "UNKNOWN" : reader.GetString(methodIndex),
                amount: reader.IsDBNull(amountIndex) ? 0m : reader.GetDecimal(amountIndex));

            if (!reader.IsDBNull(dateOfTransactionIndex))
            {
                returnedPayment.DateOfTransaction = reader.GetDateTime(dateOfTransactionIndex);
            }

            if (!reader.IsDBNull(dateConfirmedBuyerIndex))
            {
                returnedPayment.DateConfirmedBuyer = reader.GetDateTime(dateConfirmedBuyerIndex);
            }

            if (!reader.IsDBNull(dateConfirmedSellerIndex))
            {
                returnedPayment.DateConfirmedSeller = reader.GetDateTime(dateConfirmedSellerIndex);
            }

            if (!reader.IsDBNull(filePathIndex))
            {
                returnedPayment.FilePath = reader.GetString(filePathIndex);
            }

            returnedPayment.GameName = reader.IsDBNull(gameNameIndex) ? "Unknown Game" : reader.GetString(gameNameIndex);
            returnedPayment.OwnerName = reader.IsDBNull(ownerNameIndex) ? "Unknown Owner" : reader.GetString(ownerNameIndex);

            return returnedPayment;
        }

        public IReadOnlyList<HistoryPayment> GetAllPayments()
    {
            List<HistoryPayment> payments = new List<HistoryPayment>();

            using (var connection = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        t.tid, t.RequestId, t.ClientId, t.OwnerId, t.PaymentMethod, t.Amount, 
                        t.DateOfTransaction, t.DateConfirmedBuyer, t.DateConfirmedSeller, t.FilePath,
                        g.Name AS GameName,
                        u.DisplayName AS OwnerName
                    FROM [Payment] t
                    LEFT JOIN [Request] r ON t.RequestId = r.rid
                    LEFT JOIN [Game] g ON r.GameId = g.gid
                    LEFT JOIN [User] u ON t.OwnerId = u.[uid]";

                var command = new SqlCommand(query, connection);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var returnedPayment = GetPaymentFromReader(reader);
                        payments.Add(returnedPayment);
                    }
                }
            }

            return payments;
        }

        public HistoryPayment GetPaymentById(int searchedPaymentId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        t.tid, t.RequestId, t.ClientId, t.OwnerId, t.PaymentMethod, t.Amount, 
                        t.DateOfTransaction, t.DateConfirmedBuyer, t.DateConfirmedSeller, t.FilePath,
                        g.Name AS GameName,
                        u.DisplayName AS OwnerName
                    FROM [Payment] t
                    LEFT JOIN [Request] r ON t.RequestId = r.rid
                    LEFT JOIN [Game] g ON r.GameId = g.gid
                    LEFT JOIN [User] u ON t.OwnerId = u.[uid]
                    WHERE t.tid = @id";

                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", searchedPaymentId);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var returnedPayment = GetPaymentFromReader(reader);
                        return returnedPayment;
                    }
                }
            }

            return null;
        }
    }
}
