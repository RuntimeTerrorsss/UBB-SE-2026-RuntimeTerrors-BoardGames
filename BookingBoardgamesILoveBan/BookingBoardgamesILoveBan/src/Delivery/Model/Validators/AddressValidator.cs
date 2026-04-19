using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesILoveBan.Src.Delivery.Model.Validators
{
    public class AddressValidator : IValidator<Dictionary<string, string>, Address>
    {
        public Dictionary<string, string> Validate(Address address)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(address.Country))
            {
                errors["Country"] = "Country is required";
            }

            if (string.IsNullOrWhiteSpace(address.City))
            {
                errors["City"] = "City is required";
            }

            if (string.IsNullOrWhiteSpace(address.Street))
            {
                errors["Street"] = "Street is required";
            }

            if (string.IsNullOrWhiteSpace(address.StreetNumber))
            {
                errors["StreetNumber"] = "StreetNumber is required";
            }

            return errors;
        }
    }
}