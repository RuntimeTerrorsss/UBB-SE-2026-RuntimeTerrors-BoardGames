using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookingBoardgamesILoveBan.src.PaymentHistory.Model;

namespace BookingBoardgamesILoveBan.src.PaymentHistory.Repository
{
    public class RepositoryPayment : IRepositoryPayment
    {
        private readonly string _connectionString;

        public RepositoryPayment()
        {
            
            _connectionString = DatabaseBootstrap.GetAppConnection();
        }

        public IReadOnlyList<HistoryPayment> GetAllPayments()
    {
            List<HistoryPayment> payments = new List<HistoryPayment>();

            using (var connection = new SqlConnection(_connectionString))
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
                        var t = new HistoryPayment(
                            id: reader.GetInt32(0),
                            requestId: reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                            renterId: reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                            ownerId: reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                            method: reader.IsDBNull(4) ? "UNKNOWN" : reader.GetString(4),
                            amount: reader.IsDBNull(5) ? 0m : reader.GetDecimal(5)
                        );

                        if (!reader.IsDBNull(6)) t.DateOfTransaction = reader.GetDateTime(6);
                        if (!reader.IsDBNull(7)) t.DateConfirmedBuyer = reader.GetDateTime(7);
                        if (!reader.IsDBNull(8)) t.DateConfirmedSeller = reader.GetDateTime(8);
                        if (!reader.IsDBNull(9)) t.FilePath = reader.GetString(9);

                        t.GameName = reader.IsDBNull(10) ? "Unknown Game" : reader.GetString(10);
                        t.OwnerName = reader.IsDBNull(11) ? "Unknown Owner" : reader.GetString(11);

                        payments.Add(t);
                    }
                }
            }

            return payments;
        }

        public HistoryPayment GetPaymentById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
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
                command.Parameters.AddWithValue("@id", id);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var t = new HistoryPayment(
                            id: reader.GetInt32(0),
                            requestId: reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                            renterId: reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                            ownerId: reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                            method: reader.IsDBNull(4) ? "UNKNOWN" : reader.GetString(4),
                            amount: reader.IsDBNull(5) ? 0m : reader.GetDecimal(5)
                        );

                        if (!reader.IsDBNull(6)) t.DateOfTransaction = reader.GetDateTime(6);
                        if (!reader.IsDBNull(7)) t.DateConfirmedBuyer = reader.GetDateTime(7);
                        if (!reader.IsDBNull(8)) t.DateConfirmedSeller = reader.GetDateTime(8);
                        if (!reader.IsDBNull(9)) t.FilePath = reader.GetString(9);

                        t.GameName = reader.IsDBNull(10) ? "Unknown Game" : reader.GetString(10);
                        t.OwnerName = reader.IsDBNull(11) ? "Unknown Owner" : reader.GetString(11);

                        return t;
                    }
                }
            }

            return null;
        }
    }
}
