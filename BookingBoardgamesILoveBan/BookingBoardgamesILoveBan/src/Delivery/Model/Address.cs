using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBoardgamesILoveBan.src.Delivery.Model
{
    public class Address
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string StreetNumber { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        public Address(string country, string city, string street , string streetNumber) { 
            this.Street = street;
            this.City = city;
            this.StreetNumber = streetNumber;
            this.Country = country;
        }
        public Address() { }
        public override string ToString()
        {
            return $"{Street} {StreetNumber}, {City}, {Country}";
        }
    }
}
