using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BookingBoardgamesILoveBan.Src.Delivery.Model;

namespace BookingBoardgamesILoveBan.Src.Delivery.Service.MapServices
{
    public class MapService : IMapService
    {
        private readonly HttpClient httpClient = new HttpClient();

        public MapService()
        {
            httpClient.DefaultRequestHeaders.Add("User-Agent", "BookingBoardgamesILoveBan/1.0");
        }

        public async Task<Address> GetAddressFromMapAsync(double latitude, double longitude)
        {
            if (latitude == 0 && longitude == 0)
            {
                return null;
            }

            try
            {
                string url = FormattableString.Invariant($"https://nominatim.openstreetmap.org/reverse?lat={latitude}&lon={longitude}&format=json");

                HttpResponseMessage response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();

                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    JsonElement addr = doc.RootElement.GetProperty("address");

                    return new Address
                    {
                        Country = addr.TryGetProperty("country", out JsonElement country) ? country.GetString() ?? string.Empty : string.Empty,
                        City = addr.TryGetProperty("city", out JsonElement city) ? city.GetString() ?? string.Empty
                                     : addr.TryGetProperty("town", out JsonElement town) ? town.GetString() ?? string.Empty
                                     : addr.TryGetProperty("village", out JsonElement village) ? village.GetString() ?? string.Empty : string.Empty,
                        Street = addr.TryGetProperty("road", out JsonElement road) ? road.GetString() ?? string.Empty : string.Empty,
                        StreetNumber = addr.TryGetProperty("house_number", out JsonElement number) ? number.GetString() ?? string.Empty : string.Empty,
                    };
                }
            }
            catch
            {
                Debug.Write("Error when getting address from map in service");
                return null;
            }
        }
    }
}