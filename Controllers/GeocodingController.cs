using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodDeliveryApp.Controllers
{
    [Authorize]
    [Route("api/geocoding")]
    public class GeocodingController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<GeocodingController> _logger;

        public GeocodingController(
            IHttpClientFactory httpClientFactory,
            ILogger<GeocodingController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet("reverse")]
        public async Task<IActionResult> Reverse(double latitude, double longitude)
        {
            if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid latitude or longitude."
                });
            }

            var lat = latitude.ToString(CultureInfo.InvariantCulture);
            var lon = longitude.ToString(CultureInfo.InvariantCulture);

            var url =
                $"https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat={lat}&lon={lon}&zoom=18&addressdetails=1";

            try
            {
                var client = _httpClientFactory.CreateClient();

                using var request = new HttpRequestMessage(HttpMethod.Get, url);

                request.Headers.UserAgent.ParseAdd("SnackDashFoodDeliveryApp/1.0");
                request.Headers.Referrer = new Uri("https://localhost");

                using var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Reverse geocoding failed with status code {StatusCode}.",
                        response.StatusCode);

                    return StatusCode((int)response.StatusCode, new
                    {
                        success = false,
                        message = "Could not find an address for this location."
                    });
                }

                var json = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<NominatimReverseResponse>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (result == null || string.IsNullOrWhiteSpace(result.DisplayName))
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "No address was found for this location."
                    });
                }

                return Ok(new
                {
                    success = true,
                    address = result.DisplayName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reverse geocoding failed.");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Something went wrong while getting the address."
                });
            }
        }

        private class NominatimReverseResponse
        {
            [JsonPropertyName("display_name")]
            public string? DisplayName { get; set; }
        }
    }
}