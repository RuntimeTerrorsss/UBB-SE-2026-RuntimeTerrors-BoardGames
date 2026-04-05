using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.src.Delivery.Model;

namespace BookingBoardgamesILoveBan.src.Delivery.Service.MapServices
{
    public interface IMapService
    {
        /// <summary>
        /// This method does the reverse geocode from coordinates to an address
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns>The address from those specific coordinates</returns>
        public Task<Address> GetAddressFromMapAsync(double latitude, double longitude);
    }
}
