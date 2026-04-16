using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using BookingBoardgamesILoveBan.Src.Delivery.Service.MapServices;

namespace BookingBoardgamesLoveBan.Tests.Delivery
{
    public class MapServiceTests // integration tests
    {
        private readonly MapService service = new MapService();


        // ================================ GetAddressFromMapAsync ======================================

        [Fact]
        public async Task GetAddressFromMapAsync_ZeroCoordinates_ReturnsNull()
        {
            var result = await service.GetAddressFromMapAsync(0, 0);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAddressFromMapAsync_ValidCoordinates_ReturnsNonNull()
        {
            var result = await service.GetAddressFromMapAsync(46.77, 23.59);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetAddressFromMapAsync_ValidCoordinates_ReturnsCorrectCountry()
        {
            var result = await service.GetAddressFromMapAsync(46.77, 23.59);

            Assert.Equal("România", result.Country);
        }

        [Fact]
        public async Task GetAddressFromMapAsync_InvalidCoordinates_ReturnsNull()
        {
            // coordinate in ocean
            var result = await service.GetAddressFromMapAsync(0.1, 0.1);

            Assert.Null(result);
        }
    }
}
