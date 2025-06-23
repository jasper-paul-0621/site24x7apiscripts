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
        private string clientId;
        private string clientSecret;
        private string refreshToken;

        private string accessToken;
        private string accountServerUrl;
        private readonly HttpClient httpClient;

        private const string AuthFilePath = "site24x7_auth.txt";

        public AuthZoho()
        {
            // Default constructor for cases where parameters are not provided
            clientId = string.Empty;
            clientSecret = string.Empty;
            refreshToken = string.Empty;
            accessToken = string.Empty;
            accountServerUrl = "https://accounts.zoho.com";
            httpClient = new HttpClient();
        }

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
        /// Gets the internal HttpClient instance.
        /// </summary>
        public HttpClient AuthHttpClient
        {
            get { return this.httpClient; }
        }
        
        /// <summary>
        /// Asynchronously retrieves an access token using the refresh token.
        /// </summary>
        /// <returns>The access token string, or null if retrieval fails.</returns>
        public async Task<string?> GetAccessTokenAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && !string.IsNullOrEmpty(accessToken))
            {   
                return accessToken; // Return cached token if available and not forcing refresh
            }
            
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
                this.accessToken = tokenElement.GetString() ?? string.Empty;
                return this.accessToken;
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
        private bool ReadAuthFile(string authFilePath)
        {
            Console.WriteLine($"Reading Zoho OAuth credentials from {authFilePath}");
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
            this.clientId = cid!;
            this.clientSecret = csecret!;
            this.refreshToken = rtoken!;
            return true;
        }

        internal bool Authorize()
        {
            bool authTrue = ReadAuthFile(AuthZoho.AuthFilePath);
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(refreshToken))
            {
                throw new InvalidOperationException("Zoho OAuth credentials are not set. Please provide CLIENT_ID, CLIENT_SECRET, and REFRESH_TOKEN in the site24x7_auth.txt file.");
            }

            return authTrue;
        }
    }
}