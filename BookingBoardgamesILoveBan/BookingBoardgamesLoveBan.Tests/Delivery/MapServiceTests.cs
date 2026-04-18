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
        private readonly MapService service = new MapService();

        [Fact]
        public async Task GetAddressFromMapAsync_ZeroCoordinates_ReturnsNull()
        {
            var result = await service.GetAddressFromMapAsync(0, 0);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAddressFromMapAsync_ValidCoordinates_ReturnsNonNull()
        {
            await Task.Delay(1500);
            var result = await service.GetAddressFromMapAsync(46.77, 23.59);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetAddressFromMapAsync_ValidCoordinates_ReturnsCorrectCountry()
        {
            await Task.Delay(1500);
            var result = await service.GetAddressFromMapAsync(46.77, 23.59);

            Assert.Equal("România", result.Country);
            Assert.NotEmpty(result.City);
            Assert.NotEmpty(result.Street);
        }

        [Fact]
        public async Task GetAddressFromMapAsync_InvalidCoordinates_ReturnsNull()
        {
            var result = await service.GetAddressFromMapAsync(0.1, 0.1);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAddressFromMapAsync_Town_FillsCityField()
        {
            await Task.Delay(1500);
            var result = await service.GetAddressFromMapAsync(46.80, 23.70);

            Assert.NotNull(result);
            Assert.NotEmpty(result.City);
        }
    }
}