using System;
using BookingBoardgamesILoveBan.Src.Mocks.RequestMock;
using Xunit;

namespace BookingBoardgamesLoveBan.Tests.Mocks.RequestMock
{
    public class RequestDataTransferObjectUnitTests
    {
        [Fact]
        public void RequestDataTransferObject_ValidParameters_SetsPropertiesCorrectly()
        {
            var start = new DateTime(2023, 1, 1);
            var end = new DateTime(2023, 1, 5);
            var requestDTO = new RequestDataTransferObject(1, "Game", 2, 3, "Owner", "Client", start, end, 50.0m);

            var expected = new { Id = 1, GameName = "Game", ClientId = 2, OwnerId = 3, OwnerName = "Owner", ClientName = "Client", StartDate = start, EndDate = end, Price = 50.0m };
            var actual = new { requestDTO.Id, requestDTO.GameName, requestDTO.ClientId, requestDTO.OwnerId, requestDTO.OwnerName, requestDTO.ClientName, requestDTO.StartDate, requestDTO.EndDate, requestDTO.Price };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RequestDataTransferObject_PropertyUpdates_SetsPropertiesCorrectly()
        {
            var start = new DateTime(2023, 1, 1);
            var end = new DateTime(2023, 1, 5);
            var requestDTO = new RequestDataTransferObject(0, "", 0, 0, "", "", DateTime.MinValue, DateTime.MaxValue, 0m);

            requestDTO.Id = 1;
            requestDTO.GameName = "Game";
            requestDTO.ClientId = 2;
            requestDTO.OwnerId = 3;
            requestDTO.OwnerName = "Owner";
            requestDTO.ClientName = "Client";
            requestDTO.StartDate = start;
            requestDTO.EndDate = end;
            requestDTO.Price = 50.0m;

            var expected = new { Id = 1, GameName = "Game", ClientId = 2, OwnerId = 3, OwnerName = "Owner", ClientName = "Client", StartDate = start, EndDate = end, Price = 50.0m };
            var actual = new { requestDTO.Id, requestDTO.GameName, requestDTO.ClientId, requestDTO.OwnerId, requestDTO.OwnerName, requestDTO.ClientName, requestDTO.StartDate, requestDTO.EndDate, requestDTO.Price };

            Assert.Equal(expected, actual);
        }
    }
}
