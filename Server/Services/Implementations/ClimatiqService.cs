using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Server.Services.Interfaces;

namespace Server.Services.Implementations
{
    public class ClimatiqService : IClimatiqService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _dataVersion = "24.24"; // required

        private readonly Dictionary<string, (string activityId, string unitType, string unit, Func<double, double> convert)> _emissionMappings =
    new()
    {
        ["energy"] = ("electricity-energy_source_grid_mix", "energy", "kWh", v => v),
        ["water"] = ("water-supply_treatment-distribution", "volume", "l", v => v),         // 🔁 no conversion
        ["waste"] = ("waste-disposal_landfill-mixed_municipal_solid", "weight", "kg", v => v),
        ["transportation"] = ("passenger_vehicle-vehicle_type_car-fuel_source_petrol-engine_size_medium-vehicle_age_na", "distance", "km", v => v),
        ["food"] = ("food-supply_beef-farming_method_na-region_europe", "weight", "kg", v => v)
    };


        public ClimatiqService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["Climatiq:ApiKey"];
        }

        public async Task<double> CalculateCo2Async(string category, double value)
        {
            if (!_emissionMappings.TryGetValue(category.ToLower(), out var mapping))
                return 0;

            var (activityId, unitType, unit, convert) = mapping;
            var convertedValue = convert(value);

            var parameters = new Dictionary<string, object>
            {
                [unitType] = convertedValue,
                [$"{unitType}_unit"] = unit
            };

            var payload = new
            {
                emission_factor = new
                {
                    activity_id = activityId,
                    region = "GB",
                    data_version = _dataVersion
                },
                parameters = parameters
            };

            Console.WriteLine($"🚀 Sending to Climatiq: Category={category}, RawValue={value}, ConvertedValue={convertedValue}, UnitType={unitType}, Unit={unit}");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.climatiq.io/estimate");
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Climatiq API error: {response.StatusCode} - {error}");
                return 0;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("co2e", out var co2Element))
            {
                var co2 = co2Element.GetDouble();
                Console.WriteLine($"✅ Climatiq response CO2e: {co2}");
                return co2;
            }

            Console.WriteLine("⚠️ No CO2e in response.");
            return 0;
        }
    }
}
