using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;

namespace Site24x7Integration
{
    public class AuthZoho
    {
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string refreshToken;
        private readonly string accountServerUrl;
        private readonly HttpClient httpClient;

        public AuthZoho(string clientId, string clientSecret, string refreshToken, string accountServerUrl)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.refreshToken = refreshToken;
            this.accountServerUrl = accountServerUrl.TrimEnd('/');
            this.httpClient = new HttpClient();
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            var tokenUrl = $"{accountServerUrl}/oauth/v2/token";
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["refresh_token"] = refreshToken,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["grant_type"] = "refresh_token"
            });
            var response = await httpClient.PostAsync(tokenUrl, content);
            response.EnsureSuccessStatusCode();
            using var stream = await response.Content.ReadAsStreamAsync();
            var json = await JsonDocument.ParseAsync(stream);
            if (json.RootElement.TryGetProperty("access_token", out var tokenElement))
            {
                return tokenElement.GetString();
            }
            return null;
        }
    }
}