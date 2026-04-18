using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesILoveBan.Src.Delivery.Model
{
    public class Address
    {
        public Address(string country, string city, string street, string streetNumber)
        {
            Street = street;
            City = city;
            StreetNumber = streetNumber;
            Country = country;
        }

        public Address()
        {
        }

        public string Street { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string StreetNumber { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{Street} {StreetNumber}, {City}, {Country}";
        }
    }
}