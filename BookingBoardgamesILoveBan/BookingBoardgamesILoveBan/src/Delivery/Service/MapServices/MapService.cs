using BookingBoardgamesILoveBan.src.Delivery.Model;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookingBoardgamesILoveBan.src.Delivery.Service.MapServices
{
    /// <summary>
    /// Reverse-geocodes coordinates using OpenStreetMap Nominatim (free, no API key).
    /// Policy: max 1 request/sec, User-Agent must be set.
    /// </summary>
    public class MapService : IMapService
    {
        private readonly HttpClient HttpClient = new HttpClient();

        public MapService()
        {
            HttpClient.DefaultRequestHeaders.Add("User-Agent", "BookingBoardgamesILoveBan/1.0");
        }

        public async Task<Address> GetAddressFromMapAsync(double latitude, double longitude)
        {
            if (latitude == 0 && longitude == 0) return null;

            try
            {
                string url = $"https://nominatim.openstreetmap.org/reverse?lat={latitude}&lon={longitude}&format=json";

                HttpResponseMessage response = await HttpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement addr = doc.RootElement.GetProperty("address");

                return new Address
                {
                    Country = addr.TryGetProperty("country", out JsonElement country) ? country.GetString() ?? "" : "",
                    City = addr.TryGetProperty("city", out JsonElement city) ? city.GetString() ?? ""
                                 : addr.TryGetProperty("town", out JsonElement town) ? town.GetString() ?? ""
                                 : addr.TryGetProperty("village", out JsonElement village) ? village.GetString() ?? "" : "",
                    Street = addr.TryGetProperty("road", out JsonElement road) ? road.GetString() ?? "" : "",
                    StreetNumber = addr.TryGetProperty("house_number", out JsonElement number) ? number.GetString() ?? "" : "",
                };
            }
            catch
            {
                Debug.Write("Error when getting address from map in service");
                return null;
            }
        }
    }
}