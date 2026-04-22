using System;
using BookingBoardgamesILoveBan.Src.Delivery.Model;
using Xunit;

namespace BookingBoardgamesLoveBan.Tests.Delivery
{
    public class AddressTests
    {
        [Fact]
        public void Address_ValidParameters_SetsPropertiesCorrectly()
        {
            var address = new Address("Country", "City", "Street", "Number");

            var expected = new { Country = "Country", City = "City", Street = "Street", StreetNumber = "Number" };
            var actual = new { address.Country, address.City, address.Street, address.StreetNumber };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Address_DefaultInitialization_SetsEmptyStrings()
        {
            var address = new Address();

            var expected = new { Country = string.Empty, City = string.Empty, Street = string.Empty, StreetNumber = string.Empty };
            var actual = new { address.Country, address.City, address.Street, address.StreetNumber };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToString_ValidAddress_ReturnsCorrectFormat()
        {
            var address = new Address("Country", "City", "Street", "Number");

            var result = address.ToString();

            Assert.Equal("Street Number, City, Country", result);
        }
    }
}
