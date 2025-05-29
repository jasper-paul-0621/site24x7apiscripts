using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;

namespace Site24x7Integration
{
    public class Site24x7ApiClient
    {
        private readonly AuthZoho authZoho;
        private readonly HttpClient httpClient;
        private const string BaseUrl = "https://www.site24x7.com/api/";

        public Site24x7ApiClient(AuthZoho authZoho)
        {
            this.authZoho = authZoho;
            this.httpClient = new HttpClient();
        }

        private async Task<JsonElement> MakeApiRequestAsync(string endpoint, IDictionary<string, string>? parameters = null)
        {

            parameters ??= new Dictionary<string, string>();
            var token = await authZoho.GetAccessTokenAsync();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Zoho-oauthtoken", token);
            var url = BaseUrl + endpoint;
            if (parameters.Any())
            {
                var query = string.Join("&", parameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
                url += "?" + query;
            }
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            using var stream = await response.Content.ReadAsStreamAsync();
            var doc = await JsonDocument.ParseAsync(stream);
            return doc.RootElement.Clone();
        }

        public async Task<List<JsonElement>> GetMonitorTypesAsync()
        {
            var root = await MakeApiRequestAsync("monitor_type_constants");
            return root.TryGetProperty("data", out var data) ? data.EnumerateArray().ToList() : new List<JsonElement>();
        }

        public async Task<List<JsonElement>> GetMonitorsAsync()
        {
            var root = await MakeApiRequestAsync("monitors");
            return root.TryGetProperty("data", out var data) ? data.EnumerateArray().ToList() : new List<JsonElement>();
        }

        public async Task<List<JsonElement>> GetMonitorListAsync()
        {
            var root = await MakeApiRequestAsync("list_monitors");
            var data = root.GetProperty("data");
            var monitors = data.GetProperty("monitors");
            if (monitors.ValueKind == JsonValueKind.Array)
            {
                return monitors.EnumerateArray().ToList();
            }
            return new List<JsonElement>();
        }
    }
}