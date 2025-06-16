using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;

namespace Site24x7Integration
{
    public class DeviceFlow
    {
        private readonly HttpClient httpClient = new HttpClient();

        public async Task StartDeviceFlowAsync(string clientId, string clientSecret, string scope, string serverRegion)
        {
            var authUrl = $"{serverRegion}/oauth/v2/device/auth";
            var tokenUrl = $"{serverRegion}/oauth/v2/token";
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["scope"] = scope,
                ["grant_type"] = "device_token"
            });
            var response = await httpClient.PostAsync(authUrl, content);
            response.EnsureSuccessStatusCode();
            var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var deviceCode = json.RootElement.TryGetProperty("device_id", out var deviceIdElem) ? deviceIdElem.GetString() : null;
            var userCode = json.RootElement.TryGetProperty("user_code", out var userCodeElem) ? userCodeElem.GetString() : null;
            var verificationUrl = json.RootElement.TryGetProperty("verification_url", out var verificationUrlElem) ? verificationUrlElem.GetString() : null;
            var interval = json.RootElement.TryGetProperty("interval", out var intervalElem) ? intervalElem.GetInt32() : 5;

            Console.WriteLine($"Please visit {verificationUrl} and enter code: {userCode}");
            Console.WriteLine("Waiting for user authorization...");

            while (true)
            {
                await Task.Delay(interval * 1000);
                var tokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "device_token",
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["code"] = deviceCode ?? string.Empty
                });
                var tokenResp = await httpClient.PostAsync(tokenUrl, tokenContent);
                var text = await tokenResp.Content.ReadAsStringAsync();
                if (tokenResp.IsSuccessStatusCode)
                {
                    var tokJson = JsonDocument.Parse(text);
                    var refreshToken = tokJson.RootElement.TryGetProperty("refresh_token", out var refreshTokenElem) ? refreshTokenElem.GetString() : null;
                    Console.WriteLine($"Refresh Token: {refreshToken}");
                    break;
                }
                else
                {
                    var errorJson = JsonDocument.Parse(text);
                    var error = errorJson.RootElement.TryGetProperty("error", out var errorElem) ? errorElem.GetString() : null;
                    if (error == "authorization_pending")
                        continue;
                    Console.WriteLine($"Error getting refresh token: {error}");
                    break;
                }
            }
        }

        public async Task<string?> StartDeviceFlowAndGetRefreshTokenAsync(string clientId, string clientSecret, string scope, string serverRegion)
        {
            var authUrl = $"{serverRegion}/oauth/v2/device/auth";
            var tokenUrl = $"{serverRegion}/oauth/v2/token";
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["scope"] = scope,
                ["grant_type"] = "refresh_token"
            });
            var response = await httpClient.PostAsync(authUrl, content);
            response.EnsureSuccessStatusCode();
            var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var deviceCode = json.RootElement.TryGetProperty("device_id", out var deviceIdElem) ? deviceIdElem.GetString() : null;
            var userCode = json.RootElement.TryGetProperty("user_code", out var userCodeElem) ? userCodeElem.GetString() : null;
            var verificationUrl = json.RootElement.TryGetProperty("verification_url", out var verificationUrlElem) ? verificationUrlElem.GetString() : null;
            var interval = json.RootElement.TryGetProperty("interval", out var intervalElem) ? intervalElem.GetInt32() : 5;

            Console.WriteLine($"Please visit {verificationUrl} and enter code: {userCode}");
            Console.WriteLine("Waiting for user authorization...");

            while (true)
            {
                await Task.Delay(interval * 1000);
                var tokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "device_token",
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["code"] = deviceCode ?? string.Empty
                });
                var tokenResp = await httpClient.PostAsync(tokenUrl, tokenContent);
                var text = await tokenResp.Content.ReadAsStringAsync();
                if (tokenResp.IsSuccessStatusCode)
                {
                    var tokJson = JsonDocument.Parse(text);
                    var refreshToken = tokJson.RootElement.TryGetProperty("refresh_token", out var refreshTokenElem) ? refreshTokenElem.GetString() : null;
                    Console.WriteLine($"Refresh Token: {refreshToken}");
                    return refreshToken;
                }
                else
                {
                    var errorJson = JsonDocument.Parse(text);
                    var error = errorJson.RootElement.TryGetProperty("error", out var errorElem) ? errorElem.GetString() : null;
                    if (error == "authorization_pending")
                        continue;
                    Console.WriteLine($"Error getting refresh token: {error}");
                    return null;
                }
            }
        }
    }
}