using System.Globalization;
using System.Net.Http;
using System.Text.Json;

namespace Noxy;

internal sealed class WeatherService
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(12) };

    public async Task<string?> GetDailyCommentAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var locationResponse = await Http.GetAsync(
                "https://ipwho.is/?fields=success,city,latitude,longitude", cancellationToken);
            locationResponse.EnsureSuccessStatusCode();
            await using var locationStream = await locationResponse.Content.ReadAsStreamAsync(cancellationToken);
            using var location = await JsonDocument.ParseAsync(locationStream, cancellationToken: cancellationToken);
            var root = location.RootElement;
            if (!root.TryGetProperty("success", out var success) || !success.GetBoolean() ||
                !root.TryGetProperty("latitude", out var latitude) ||
                !root.TryGetProperty("longitude", out var longitude))
                return null;

            var city = root.TryGetProperty("city", out var cityValue) ? cityValue.GetString() : null;
            var lat = latitude.GetDouble().ToString(CultureInfo.InvariantCulture);
            var lon = longitude.GetDouble().ToString(CultureInfo.InvariantCulture);
            var url = "https://api.open-meteo.com/v1/forecast" +
                $"?latitude={Uri.EscapeDataString(lat)}&longitude={Uri.EscapeDataString(lon)}" +
                "&current=temperature_2m,weather_code&daily=weather_code,temperature_2m_max" +
                "&forecast_days=1&timezone=auto";
            using var weatherResponse = await Http.GetAsync(url, cancellationToken);
            weatherResponse.EnsureSuccessStatusCode();
            await using var weatherStream = await weatherResponse.Content.ReadAsStreamAsync(cancellationToken);
            using var weather = await JsonDocument.ParseAsync(weatherStream, cancellationToken: cancellationToken);
            var weatherRoot = weather.RootElement;
            var current = weatherRoot.GetProperty("current");
            var daily = weatherRoot.GetProperty("daily");
            var temperature = current.GetProperty("temperature_2m").GetDouble();
            var currentCode = current.GetProperty("weather_code").GetInt32();
            var high = daily.GetProperty("temperature_2m_max")[0].GetDouble();
            var dayCode = daily.GetProperty("weather_code")[0].GetInt32();
            var place = string.IsNullOrWhiteSpace(city) ? "bei dir" : $"bei {city}";
            return $"Wetter {place}: gerade {temperature:0} °C und {Describe(currentCode)}. Heute bis zu {high:0} °C, {Describe(dayCode)}. Open-Meteo.";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or InvalidOperationException or KeyNotFoundException or FormatException)
        {
            return null;
        }
    }

    private static string Describe(int code) => code switch
    {
        0 => "klar",
        1 => "überwiegend klar",
        2 => "teilweise bewölkt",
        3 => "bedeckt",
        45 or 48 => "neblig",
        51 or 53 or 55 or 56 or 57 => "Nieselregen",
        61 or 63 or 65 or 66 or 67 => "Regen",
        71 or 73 or 75 or 77 => "Schnee",
        80 or 81 or 82 => "Regenschauer",
        85 or 86 => "Schneeschauer",
        95 or 96 or 99 => "Gewitter",
        _ => "wechselhaft"
    };
}

