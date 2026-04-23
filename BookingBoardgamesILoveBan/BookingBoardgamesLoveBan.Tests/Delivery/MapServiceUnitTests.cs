using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.Delivery.Service.MapServices;
using Moq;
using Moq.Protected;
using Xunit;

namespace BookingBoardgamesLoveBan.Tests.Delivery
{
    public class MapServiceUnitTests
    {
        [Fact]
        public async Task GetAddressFromMapAsync_InvalidCoordinates_ReturnsNull()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            var client = new HttpClient(mockHandler.Object);
            var mapService = new MapService(client);

            var resultedMapAddress = await mapService.GetAddressFromMapAsync(0.1, 0.1);

            Assert.Null(resultedMapAddress);
        }

        [Fact]
        public async Task GetAddressFromMapAsync_ValidCoordinates_ReturnsAddress()
        {
            var jsonResponse = @"{
                ""address"": {
                    ""country"": ""Romania"",
                    ""city"": ""Cluj-Napoca"",
                    ""road"": ""Teodor Mihali"",
                    ""house_number"": ""58""
                }
            }";

            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            var client = new HttpClient(mockHandler.Object);
            var mapService = new MapService(client);

            var resultedMapAddress = await mapService.GetAddressFromMapAsync(46.77, 23.59);

            var expectedMapAddress = new { Country = "Romania", City = "Cluj-Napoca", Street = "Teodor Mihali", StreetNumber = "58" };
            var actualMapAddress = new { resultedMapAddress?.Country, resultedMapAddress?.City, resultedMapAddress?.Street, resultedMapAddress?.StreetNumber };

            Assert.NotNull(resultedMapAddress);
            Assert.Equal(expectedMapAddress, actualMapAddress);
        }

        [Fact]
        public async Task GetAddressFromMapAsync_CityMissing_ReturnsTown()
        {
            var jsonResponse = @"{
                ""address"": {
                    ""town"": ""Some Town""
                }
            }";

            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            var client = new HttpClient(mockHandler.Object);
            var mapService = new MapService(client);

            var resultedMapAddress = await mapService.GetAddressFromMapAsync(46.77, 23.59);

            Assert.NotNull(resultedMapAddress);
            Assert.Equal("Some Town", resultedMapAddress.City);
        }

        [Fact]
        public async Task GetAddressFromMapAsync_CityAndTownMissing_ReturnsVillage()
        {
            var jsonResponse = @"{
                ""address"": {
                    ""village"": ""Some Village""
                }
            }";

            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            var client = new HttpClient(mockHandler.Object);
            var mapService = new MapService(client);

            var resultedMapAddress = await mapService.GetAddressFromMapAsync(46.77, 23.59);

            Assert.NotNull(resultedMapAddress);
            Assert.Equal("Some Village", resultedMapAddress.City);
        }

        [Fact]
        public async Task GetAddressFromMapAsync_NoAddressData_ReturnsEmptyFields()
        {
            var jsonResponse = @"{ ""address"": { } }";

            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            var client = new HttpClient(mockHandler.Object);
            var mapService = new MapService(client);

            var resultedMapAddress = await mapService.GetAddressFromMapAsync(46.77, 23.59);

            var expectedMapAddress = new { Country = string.Empty, City = string.Empty, Street = string.Empty, StreetNumber = string.Empty };
            var actualMapAddress = new { resultedMapAddress?.Country, resultedMapAddress?.City, resultedMapAddress?.Street, resultedMapAddress?.StreetNumber };

            Assert.NotNull(resultedMapAddress);
            Assert.Equal(expectedMapAddress, actualMapAddress);
        }

        [Fact]
        public async Task GetAddressFromMapAsync_HttpException_ReturnsNull()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Network Error"));

            var client = new HttpClient(mockHandler.Object);
            var mapService = new MapService(client);

            var resultedMapAddress = await mapService.GetAddressFromMapAsync(46.77, 23.59);

            Assert.Null(resultedMapAddress);
        }
    }
}
