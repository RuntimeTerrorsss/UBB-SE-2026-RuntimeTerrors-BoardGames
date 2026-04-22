using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Model;
using BookingBoardgamesILoveBan.Src.PaymentHistory.Constants;

namespace BookingBoardgamesILoveBan.Src.PaymentHistory.Repository
{
    public class RepositoryPayment : IRepositoryPayment
    {
        private readonly string connectionString;

        public RepositoryPayment()
        {
            connectionString = DatabaseBootstrap.GetAppConnection();
        }

        private int GetIntOrDefaultValue(SqlDataReader reader, int index, int defaultValue)
        {
            return reader.IsDBNull(index) ? defaultValue : reader.GetInt32(index);
        }

        private string GetStringOrDefaultValue(SqlDataReader reader, int index, string defaultValue)
        {
            return reader.IsDBNull(index) ? defaultValue : reader.GetString(index);
        }

        private decimal GetDecimalOrDefaultValue(SqlDataReader reader, int index, decimal defaultValue)
        {
            return reader.IsDBNull(index) ? defaultValue : reader.GetDecimal(index);
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
                requestId: GetIntOrDefaultValue(reader, requestIdIndex, PaymentHistoryConstants.NullIdDefaultValue),
                renterId: GetIntOrDefaultValue(reader, renterIdIndex, PaymentHistoryConstants.NullIdDefaultValue),
                ownerId: GetIntOrDefaultValue(reader, ownerIdIndex, PaymentHistoryConstants.NullIdDefaultValue),
                method: GetStringOrDefaultValue(reader, methodIndex, PaymentHistoryConstants.NullMethodDefaultValue),
                amount: GetDecimalOrDefaultValue(reader, amountIndex, PaymentHistoryConstants.NullAmountDefaultValue));

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

            returnedPayment.GameName = GetStringOrDefaultValue(reader, gameNameIndex, PaymentHistoryConstants.NullGameNameDefaultValue);
            returnedPayment.OwnerName = GetStringOrDefaultValue(reader, ownerNameIndex, PaymentHistoryConstants.NullOwnerNameDefaultValue);

            return returnedPayment;
        }

        public IReadOnlyList<HistoryPayment> GetAllPayments()
    {
            List<HistoryPayment> allPayments = new List<HistoryPayment>();

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
                        allPayments.Add(returnedPayment);
                    }
                }
            }

            return allPayments;
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
