using BookingBoardgamesILoveBan.Src.Delivery.Model;
using BookingBoardgamesILoveBan.Src.Delivery.Model.Validators;

namespace BookingBoardgamesLoveBan.Tests.Delivery
{
    public class AddressValidatorTests
    {
        private readonly AddressValidator addressValidator = new AddressValidator();

        [Fact]
        public void EmptyAddressTest()
        {
            Address emptyAddress = new Address(string.Empty, string.Empty, string.Empty, string.Empty);
            Dictionary<string, string> errors = addressValidator.Validate(emptyAddress);

            Assert.NotEmpty(errors);
            Assert.Equal(
                new Dictionary<string, string>
                {
                    { "Country", "Country is required" },
                    { "City", "City is required" },
                    { "Street", "Street is required" },
                    { "StreetNumber", "StreetNumber is required" }
                },
                errors);
        }

        [Fact]
        public void ValidAddressTest()
        {
            Address validAddress = new Address("Romania", "Cluj-Napoca", "Teodor Mihali", "58");
            Dictionary<string, string> errors = addressValidator.Validate(validAddress);

            Assert.Empty(errors);
        }

        [Fact]
        public void MostlyValidAddressTest()
        {
            Assert.Equal(
                new Dictionary<string, string> { { "Country", "Country is required" } },
                addressValidator.Validate(new Address(string.Empty, "Cluj-Napoca", "Teodor Mihali", "58")));

            Assert.Equal(
                new Dictionary<string, string> { { "City", "City is required" } },
                addressValidator.Validate(new Address("Romania", string.Empty, "Teodor Mihali", "58")));

            Assert.Equal(
                new Dictionary<string, string> { { "Street", "Street is required" } },
                addressValidator.Validate(new Address("Romania", "Cluj-Napoca", string.Empty, "58")));

            Assert.Equal(
                new Dictionary<string, string> { { "StreetNumber", "StreetNumber is required" } },
                addressValidator.Validate(new Address("Romania", "Cluj-Napoca", "Teodor Mihali", string.Empty)));
        }

        [Fact]
        public void HalfValidAddressTest()
        {
            Assert.Equal(
                new Dictionary<string, string>
                {
                    { "Country", "Country is required" },
                    { "City", "City is required" }
                },
                addressValidator.Validate(new Address(string.Empty, string.Empty, "Teodor Mihali", "58")));

            Assert.Equal(
                new Dictionary<string, string>
                {
                    { "Street", "Street is required" },
                    { "StreetNumber", "StreetNumber is required" }
                },
                addressValidator.Validate(new Address("Romania", "Cluj-Napoca", string.Empty, string.Empty)));

            Assert.Equal(
                new Dictionary<string, string>
                {
                    { "City", "City is required" },
                    { "Street", "Street is required" }
                },
                addressValidator.Validate(new Address("Romania", string.Empty, string.Empty, "58")));
        }

        [Fact]
        public void MostlyEmptyAddressTest()
        {
            Assert.Equal(
                new Dictionary<string, string>
                {
                    { "City", "City is required" },
                    { "Street", "Street is required" },
                    { "StreetNumber", "StreetNumber is required" }
                },
                addressValidator.Validate(new Address("Romania", string.Empty, string.Empty, string.Empty)));

            Assert.Equal(
                new Dictionary<string, string>
                {
                    { "Country", "Country is required" },
                    { "Street", "Street is required" },
                    { "StreetNumber", "StreetNumber is required" }
                },
                addressValidator.Validate(new Address(string.Empty, "Cluj-Napoca", string.Empty, string.Empty)));

            Assert.Equal(
                new Dictionary<string, string>
                {
                    { "Country", "Country is required" },
                    { "City", "City is required" },
                    { "StreetNumber", "StreetNumber is required" }
                },
                addressValidator.Validate(new Address(string.Empty, string.Empty, "Teodor Mihali", string.Empty)));

            Assert.Equal(
                new Dictionary<string, string>
                {
                    { "Country", "Country is required" },
                    { "City", "City is required" },
                    { "Street", "Street is required" }
                },
                addressValidator.Validate(new Address(string.Empty, string.Empty, string.Empty, "58")));
        }
    }
}