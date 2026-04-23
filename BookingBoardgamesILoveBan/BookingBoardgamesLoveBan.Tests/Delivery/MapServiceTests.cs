using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.Delivery.Service.MapServices;
using Xunit;

namespace BookingBoardgamesLoveBan.Tests.Delivery
{
    public class MapServiceTests
    {
        private readonly MapService mapService = new MapService();

        [Fact]
        public async Task GetAddressFromMapAsync_ZeroCoordinates_ReturnsNull()
        {
            var resultedMapAddress = await mapService.GetAddressFromMapAsync(0, 0);

            Assert.Null(resultedMapAddress);
        }

        [Fact]
        public async Task GetAddressFromMapAsync_ValidCoordinates_ReturnsNonNull()
        {
            await Task.Delay(1500);
            var resultedMapAddress = await mapService.GetAddressFromMapAsync(46.77, 23.59);

            Assert.NotNull(resultedMapAddress);
        }

        [Fact]
        public async Task GetAddressFromMapAsync_ValidCoordinates_ReturnsCorrectCountry()
        {
            await Task.Delay(1500);
            var resultedMapAddress = await mapService.GetAddressFromMapAsync(46.77, 23.59);

            var expectedMapAddress = new { Country = "România", HasCity = true, HasStreet = true };
            var actualMapAddress = new { resultedMapAddress.Country, HasCity = !string.IsNullOrEmpty(resultedMapAddress.City), HasStreet = !string.IsNullOrEmpty(resultedMapAddress.Street) };

            Assert.Equal(expectedMapAddress, actualMapAddress);
        }

        [Fact]
        public async Task GetAddressFromMapAsync_InvalidCoordinates_ReturnsNull()
        {
            var resultedMapAddress = await mapService.GetAddressFromMapAsync(0.1, 0.1);

            Assert.Null(resultedMapAddress);
        }

        [Fact]
        public async Task GetAddressFromMapAsync_TownProvided_FillsCityField()
        {
            await Task.Delay(1500);
            var resultedMapAddress = await mapService.GetAddressFromMapAsync(46.80, 23.70);

            Assert.NotNull(resultedMapAddress);
            Assert.NotEmpty(resultedMapAddress.City);
        }
    }
}