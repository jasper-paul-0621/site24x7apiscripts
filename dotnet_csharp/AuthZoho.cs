using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;

namespace Site24x7Integration
{
    /// <summary>
    /// Handles Zoho OAuth authentication and access token retrieval for Site24x7 API.
    /// </summary>
    public class AuthZoho
    {
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string refreshToken;
        private readonly string accountServerUrl;
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthZoho"/> class.
        /// </summary>
        /// <param name="clientId">The Zoho OAuth client ID.</param>
        /// <param name="clientSecret">The Zoho OAuth client secret.</param>
        /// <param name="refreshToken">The Zoho OAuth refresh token.</param>
        /// <param name="accountServerUrl">The Zoho accounts server URL.</param>
        public AuthZoho(string clientId, string clientSecret, string refreshToken, string accountServerUrl)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.refreshToken = refreshToken;
            this.accountServerUrl = accountServerUrl.TrimEnd('/');
            this.httpClient = new HttpClient();
        }

        /// <summary>
        /// Asynchronously retrieves an access token using the refresh token.
        /// </summary>
        /// <returns>The access token string, or null if retrieval fails.</returns>
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

        /// <summary>
        /// Attempts to read Zoho OAuth credentials from a file.
        /// </summary>
        /// <param name="authFilePath">Path to the credentials file.</param>
        /// <param name="clientId">Output: The client ID.</param>
        /// <param name="clientSecret">Output: The client secret.</param>
        /// <param name="refreshToken">Output: The refresh token.</param>
        /// <returns>True if all credentials are found and valid; otherwise, false.</returns>
        public static bool TryReadAuthFile(string authFilePath, out string clientId, out string clientSecret, out string refreshToken)
        {
            clientId = string.Empty;
            clientSecret = string.Empty;
            refreshToken = string.Empty;
            var authValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!System.IO.File.Exists(authFilePath))
                return false;
            foreach (var line in System.IO.File.ReadAllLines(authFilePath))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;
                var parts = trimmed.Split('=', 2);
                if (parts.Length == 2)
                    authValues[parts[0].Trim()] = parts[1].Trim();
            }
            string? cid, csecret, rtoken;
            if (!authValues.TryGetValue("CLIENT_ID", out cid) ||
                !authValues.TryGetValue("CLIENT_SECRET", out csecret) ||
                !authValues.TryGetValue("REFRESH_TOKEN", out rtoken) ||
                string.IsNullOrWhiteSpace(cid) ||
                string.IsNullOrWhiteSpace(csecret) ||
                string.IsNullOrWhiteSpace(rtoken))
            {
                return false;
            }
            clientId = cid!;
            clientSecret = csecret!;
            refreshToken = rtoken!;
            return true;
        }
    }
}