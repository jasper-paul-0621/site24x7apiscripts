using System.Text.Json.Serialization;

namespace Site24x7Integration
{
    /// <summary>
    /// Represents a monitor object from the Site24x7 API.
    /// Add properties as needed to match the monitor JSON structure.
    /// </summary>
    public class Monitor
    {
        [JsonPropertyName("monitor_id")]
        public string? MonitorId { get; set; }

        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        // Add more properties as needed based on your monitor JSON structure
    }
}
