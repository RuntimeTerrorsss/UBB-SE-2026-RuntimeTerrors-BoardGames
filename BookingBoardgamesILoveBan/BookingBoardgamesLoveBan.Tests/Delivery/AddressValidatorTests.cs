using BookingBoardgamesILoveBan.Src.Delivery.Model;
using BookingBoardgamesILoveBan.Src.Delivery.Model.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesLoveBan.Tests.Delivery
{
    public class AddressValidatorTests // unit tests
    {
        private readonly AddressValidator _addressValidator = new AddressValidator();

        [Fact]
        public void EmptyAddressTest()
        {
            Address emptyAddress = new Address("", "", "", "");
            Dictionary<string, string> errors = this._addressValidator.Validate(emptyAddress);
            Assert.NotEmpty(errors);
            Assert.Equal("Country is required", errors["Country"]);
            Assert.Equal("City is required", errors["City"]);
            Assert.Equal("Street is required", errors["Street"]);
            Assert.Equal("StreetNumber is required", errors["StreetNumber"]);
        }

        [Fact]
        public void ValidAddressTest()
        {
            Address validAddress = new Address("Romania", "Cluj-Napoca", "Teodor Mihali", "58");
            Dictionary<string, string> errors = this._addressValidator.Validate(validAddress);
            Assert.Empty(errors);
        }

        [Fact]
        public void MostlyValidAddressTest()
        {
            Address invalidCountryAddress = new Address("", "Cluj-Napoca", "Teodor Mihali", "58");
            Dictionary<string, string> errors = this._addressValidator.Validate(invalidCountryAddress);
            Assert.Single(errors);
            Assert.NotNull(errors["Country"]);
            Assert.Equal("Country is required", errors["Country"]);

            Address invalidCityAddress = new Address("Romania", "", "Teodor Mihali", "58");
            errors = this._addressValidator.Validate(invalidCityAddress);
            Assert.Single(errors);
            Assert.NotNull(errors["City"]);
            Assert.Equal("City is required", errors["City"]);

            Address invalidStreetAddress = new Address("Romania", "Cluj-Napoca", "", "58");
            errors = this._addressValidator.Validate(invalidStreetAddress);
            Assert.Single(errors);
            Assert.NotNull(errors["Street"]);
            Assert.Equal("Street is required", errors["Street"]);

            Address invalidStreetNoAddress = new Address("Romania", "Cluj-Napoca", "Teodor Mihali", "");
            errors = this._addressValidator.Validate(invalidStreetNoAddress);
            Assert.Single(errors);
            Assert.NotNull(errors["StreetNumber"]);
            Assert.Equal("StreetNumber is required", errors["StreetNumber"]);
        }

        [Fact]
        public void HalfValidAddressTest()
        {
            Address invalidCountryCityAddress = new Address("", "", "Teodor Mihali", "58");
            Dictionary<string, string> errors = this._addressValidator.Validate(invalidCountryCityAddress);
            Assert.Equal(2, errors.Count);
            Assert.NotNull(errors["Country"]);
            Assert.NotNull(errors["City"]);
            Assert.Equal("Country is required", errors["Country"]); 
            Assert.Equal("City is required", errors["City"]);

            Address invalidStreetNoAddress = new Address("Romania", "Cluj-Napoca", "", "");
            errors = this._addressValidator.Validate(invalidStreetNoAddress);
            Assert.Equal(2, errors.Count);
            Assert.NotNull(errors["Street"]);
            Assert.NotNull(errors["StreetNumber"]);
            Assert.Equal("Street is required", errors["Street"]);
            Assert.Equal("StreetNumber is required", errors["StreetNumber"]);

            Address invalidCityStreetAddress = new Address("Romania", "", "", "58");
            errors = this._addressValidator.Validate(invalidCityStreetAddress);
            Assert.Equal(2, errors.Count);
            Assert.NotNull(errors["City"]);
            Assert.NotNull(errors["Street"]);
            Assert.Equal("City is required", errors["City"]);
            Assert.Equal("Street is required", errors["Street"]);
        }

        [Fact]
        public void MostlyEmptyAddressTest()
        {
            Address validCountryAddress = new Address("Romania", "", "", "");
            Dictionary<string, string> errors = this._addressValidator.Validate(validCountryAddress);
            Assert.Equal(3, errors.Count);
            Assert.NotNull(errors["City"]);
            Assert.NotNull(errors["Street"]);
            Assert.NotNull(errors["StreetNumber"]);
            Assert.Equal("City is required", errors["City"]);
            Assert.Equal("Street is required", errors["Street"]);
            Assert.Equal("StreetNumber is required", errors["StreetNumber"]);


            Address validCityAddress = new Address("", "Cluj-Napoca", "", "");
            errors = this._addressValidator.Validate(validCityAddress);
            Assert.Equal(3, errors.Count);
            Assert.NotNull(errors["Country"]);
            Assert.NotNull(errors["Street"]);
            Assert.NotNull(errors["StreetNumber"]);
            Assert.Equal("Country is required", errors["Country"]);
            Assert.Equal("Street is required", errors["Street"]);
            Assert.Equal("StreetNumber is required", errors["StreetNumber"]);

            Address validStreetAddress = new Address("", "", "Teodor Mihali", "");
            errors = this._addressValidator.Validate(validStreetAddress);
            Assert.Equal(3, errors.Count);
            Assert.NotNull(errors["Country"]);
            Assert.NotNull(errors["City"]);
            Assert.NotNull(errors["StreetNumber"]);
            Assert.Equal("Country is required", errors["Country"]);
            Assert.Equal("City is required", errors["City"]);
            Assert.Equal("StreetNumber is required", errors["StreetNumber"]);

            Address validStreetNoAddress = new Address("", "", "", "58");
            errors = this._addressValidator.Validate(validStreetNoAddress);
            Assert.Equal(3, errors.Count);
            Assert.NotNull(errors["Country"]);
            Assert.NotNull(errors["City"]);
            Assert.NotNull(errors["Street"]);
            Assert.Equal("Country is required", errors["Country"]);
            Assert.Equal("City is required", errors["City"]);
            Assert.Equal("Street is required", errors["Street"]);
        }
    }
}