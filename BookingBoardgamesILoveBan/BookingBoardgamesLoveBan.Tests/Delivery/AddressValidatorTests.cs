using BookingBoardgamesILoveBan.Src.Delivery.Model;
using BookingBoardgamesILoveBan.Src.Delivery.Model.Validators;

namespace BookingBoardgamesLoveBan.Tests.Delivery
{
    public class AddressValidatorTests
    {
        private readonly AddressValidator addressValidator = new AddressValidator();

        [Fact]
        public void Validate_EmptyAddress_ReturnsAllErrors()
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
        public void Validate_ValidAddress_ReturnsNoErrors()
        {
            Address validAddress = new Address("Romania", "Cluj-Napoca", "Teodor Mihali", "58");
            Dictionary<string, string> errors = addressValidator.Validate(validAddress);

            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_OneMissingField_ReturnsSpecificError()
        {
            var expected = new[]
            {
                new Dictionary<string, string> { { "Country", "Country is required" } },
                new Dictionary<string, string> { { "City", "City is required" } },
                new Dictionary<string, string> { { "Street", "Street is required" } },
                new Dictionary<string, string> { { "StreetNumber", "StreetNumber is required" } }
            };

            var actual = new[]
            {
                addressValidator.Validate(new Address(string.Empty, "Cluj-Napoca", "Teodor Mihali", "58")),
                addressValidator.Validate(new Address("Romania", string.Empty, "Teodor Mihali", "58")),
                addressValidator.Validate(new Address("Romania", "Cluj-Napoca", string.Empty, "58")),
                addressValidator.Validate(new Address("Romania", "Cluj-Napoca", "Teodor Mihali", string.Empty))
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Validate_TwoMissingFields_ReturnsSpecificErrors()
        {
            var expected = new[]
            {
                new Dictionary<string, string>
                {
                    { "Country", "Country is required" },
                    { "City", "City is required" }
                },
                new Dictionary<string, string>
                {
                    { "Street", "Street is required" },
                    { "StreetNumber", "StreetNumber is required" }
                },
                new Dictionary<string, string>
                {
                    { "City", "City is required" },
                    { "Street", "Street is required" }
                }
            };

            var actual = new[]
            {
                addressValidator.Validate(new Address(string.Empty, string.Empty, "Teodor Mihali", "58")),
                addressValidator.Validate(new Address("Romania", "Cluj-Napoca", string.Empty, string.Empty)),
                addressValidator.Validate(new Address("Romania", string.Empty, string.Empty, "58"))
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Validate_ThreeMissingFields_ReturnsSpecificErrors()
        {
            var expected = new[]
            {
                new Dictionary<string, string>
                {
                    { "City", "City is required" },
                    { "Street", "Street is required" },
                    { "StreetNumber", "StreetNumber is required" }
                },
                new Dictionary<string, string>
                {
                    { "Country", "Country is required" },
                    { "Street", "Street is required" },
                    { "StreetNumber", "StreetNumber is required" }
                },
                new Dictionary<string, string>
                {
                    { "Country", "Country is required" },
                    { "City", "City is required" },
                    { "StreetNumber", "StreetNumber is required" }
                },
                new Dictionary<string, string>
                {
                    { "Country", "Country is required" },
                    { "City", "City is required" },
                    { "Street", "Street is required" }
                }
            };

            var actual = new[]
            {
                addressValidator.Validate(new Address("Romania", string.Empty, string.Empty, string.Empty)),
                addressValidator.Validate(new Address(string.Empty, "Cluj-Napoca", string.Empty, string.Empty)),
                addressValidator.Validate(new Address(string.Empty, string.Empty, "Teodor Mihali", string.Empty)),
                addressValidator.Validate(new Address(string.Empty, string.Empty, string.Empty, "58"))
            };

            Assert.Equal(expected, actual);
        }
    }
}