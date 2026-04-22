using System;
using BookingBoardgamesILoveBan.Src.Mocks.RequestMock;
using Xunit;

namespace BookingBoardgamesLoveBan.Tests.Mocks.RequestMock
{
    public class RequestUnitTests
    {
        [Fact]
        public void Request_ValidParameters_SetsPropertiesCorrectly()
        {
            var start = new DateTime(2023, 1, 1);
            var end = new DateTime(2023, 1, 5);
            var request = new Request(1, 2, 3, 4, start, end);

            var expected = new { Id = 1, GameId = 2, ClientId = 3, OwnerId = 4, StartDate = start, EndDate = end };
            var actual = new { request.Id, request.GameId, request.ClientId, request.OwnerId, request.StartDate, request.EndDate };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Request_PropertyUpdates_SetsPropertiesCorrectly()
        {
            var start = new DateTime(2023, 1, 1);
            var end = new DateTime(2023, 1, 5);
            var request = new Request(0, 0, 0, 0, DateTime.MinValue, DateTime.MaxValue);
            
            request.Id = 1;
            request.GameId = 2;
            request.ClientId = 3;
            request.OwnerId = 4;
            request.StartDate = start;
            request.EndDate = end;

            var expected = new { Id = 1, GameId = 2, ClientId = 3, OwnerId = 4, StartDate = start, EndDate = end };
            var actual = new { request.Id, request.GameId, request.ClientId, request.OwnerId, request.StartDate, request.EndDate };

            Assert.Equal(expected, actual);
        }
    }
}
