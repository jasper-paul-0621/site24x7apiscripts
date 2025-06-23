using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;

namespace Site24x7Integration
{
    public interface ISite24x7ApiClient
    {
        Task<List<JsonElement>> GetMonitorListAsync();
        Task<List<JsonElement>> GetMonitorsAsync();
        Task<List<JsonElement>> GetMonitorTypesAsync();
    }


    /// <summary>
    /// Client for interacting with the Site24x7 API, including monitor retrieval and type queries.
    /// </summary>
    public class Site24x7ApiClient : ISite24x7ApiClient
    {
        private readonly AuthZoho authZoho;
        private const string BaseUrl = "https://www.site24x7.com/api/";

        /// <summary>
        /// Initializes a new instance of the <see cref="Site24x7ApiClient"/> class.
        /// </summary>
        /// <param name="authZoho">The Zoho authentication handler.</param>
        public Site24x7ApiClient(AuthZoho authZoho)
        {
            this.authZoho = authZoho;
        }

        /// <summary>
        /// Makes an authenticated API request to the specified endpoint.
        /// </summary>
        /// <param name="endpoint">The API endpoint (relative to base URL).</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <returns>The root <see cref="JsonElement"/> of the API response.</returns>
        private async Task<JsonElement> MakeApiRequestAsync(string endpoint, IDictionary<string, string>? parameters = null)
        {
            parameters ??= new Dictionary<string, string>();
            var token = await authZoho.GetAccessTokenAsync();

            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("Access token is null or empty. Please authenticate first.");
            }

            authZoho.AuthHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Zoho-oauthtoken", token);
            var url = BaseUrl + endpoint;
            if (parameters.Any())
            {
                var query = string.Join("&", parameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
                url += "?" + query;
            }

            try
            {
                var response = await authZoho.AuthHttpClient.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    // Token might be expired, try to refresh and retry once
                    token = await authZoho.GetAccessTokenAsync(true);
                    if (string.IsNullOrEmpty(token))
                        throw new InvalidOperationException("Failed to refresh access token.");
                    authZoho.AuthHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Zoho-oauthtoken", token);
                    response = await authZoho.AuthHttpClient.GetAsync(url);
                }
                response.EnsureSuccessStatusCode();
                using var stream = await response.Content.ReadAsStreamAsync();
                var doc = await JsonDocument.ParseAsync(stream);
                return doc.RootElement.Clone();
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Error making API request to {url}: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Error parsing JSON response from {url}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the list of monitor types from the Site24x7 API.
        /// </summary>
        /// <returns>A list of monitor type <see cref="JsonElement"/> objects.</returns>
        public async Task<List<JsonElement>> GetMonitorTypesAsync()
        {
            var root = await MakeApiRequestAsync("monitor_type_constants");
            return root.TryGetProperty("data", out var data) ? data.EnumerateArray().ToList() : new List<JsonElement>();
        }

        /// <summary>
        /// Gets the list of monitors from the Site24x7 API.
        /// </summary>
        /// <returns>A list of monitor <see cref="JsonElement"/> objects.</returns>
        public async Task<List<JsonElement>> GetMonitorsAsync()
        {
            var root = await MakeApiRequestAsync("monitors");
            return root.TryGetProperty("data", out var data) ? data.EnumerateArray().ToList() : new List<JsonElement>();
        }

        /// <summary>
        /// Gets the monitor list (with details) from the Site24x7 API, iterating until offset is empty or null to get all monitors.
        /// Includes safeguards against infinite loops, handles exceptions, and out-of-memory errors.
        /// Prints the offset if found.
        /// </summary>
        /// <returns>A list of monitor <see cref="JsonElement"/> objects with details.</returns>
        public async Task<List<JsonElement>> GetMonitorListAsync()
        {
            var allMonitors = new List<JsonElement>();
            string? offset = null;
            int maxPages = 1000; // Safeguard: maximum number of pages to fetch
            int pageCount = 0;
            try
            {
                do
                {
                    var parameters = offset != null ? new Dictionary<string, string> { ["offset"] = offset } : null;
                    var root = await MakeApiRequestAsync("list_monitors", parameters);
                    var data = root.GetProperty("data");
                    var monitors = data.GetProperty("monitors");
                    if (monitors.ValueKind == JsonValueKind.Array)
                    {
                        allMonitors.AddRange(monitors.EnumerateArray());
                    }
                    offset = data.TryGetProperty("offset", out var offsetProp) &&
                             offsetProp.ValueKind == JsonValueKind.String &&
                             !string.IsNullOrEmpty(offsetProp.GetString())
                        ? offsetProp.GetString()
                        : null;
                    if (!string.IsNullOrEmpty(offset))
                        Console.WriteLine($"Next offset found: {offset}");
                    else
                        Console.WriteLine("No more offsets found, ending pagination.");

                    // Safeguard against infinite pagination


                    pageCount++;
                    if (pageCount >= maxPages)
                    {
                        throw new InvalidOperationException($"Aborting: exceeded maximum page limit ({maxPages}). Possible infinite pagination.");
                    }
                } while (!string.IsNullOrEmpty(offset));
            }
            catch (OutOfMemoryException)
            {
                throw new InvalidOperationException("Out of memory while fetching monitor list. Partial results returned.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error fetching monitor list: {ex.Message}", ex);
            }
            return allMonitors;
        }
    }
}