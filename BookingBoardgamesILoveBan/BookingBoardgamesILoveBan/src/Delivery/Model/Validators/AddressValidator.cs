using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesILoveBan.Src.Delivery.Model.Validators
{
    public class AddressValidator : IValidator<Dictionary<string, string>, Address>
    {
        private const string RequiredFieldMessage = "is required";

        public Dictionary<string, string> Validate(Address address)
        {
            var validationErrors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(address.Country))
            {
                validationErrors[nameof(address.Country)] = $"{nameof(address.Country)} {RequiredFieldMessage}";
            }

            if (string.IsNullOrWhiteSpace(address.City))
            {
                validationErrors[nameof(address.City)] = $"{nameof(address.City)} {RequiredFieldMessage}";
            }

            if (string.IsNullOrWhiteSpace(address.Street))
            {
                validationErrors[nameof(address.Street)] = $"{nameof(address.Street)} {RequiredFieldMessage}";
            }

            if (string.IsNullOrWhiteSpace(address.StreetNumber))
            {
                validationErrors[nameof(address.StreetNumber)] = $"{nameof(address.StreetNumber)} {RequiredFieldMessage}";
            }

            return validationErrors;
        }
    }
}