using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System;
using Site24x7Integration;

namespace Site24x7CustomReports.Services
{
    public class ApiService : IApiService
    {
        private readonly Site24x7ApiClient _apiClient;

        public ApiService()
        {
            IAuthService authService = new AuthService();
            authService.Run(); // Ensure authentication is performed
            _apiClient = new Site24x7ApiClient(authService.ZohoAuth);
        }

        public async Task<string> GetMonitorsJsonAsync()
        {
            // Use the injected AuthZoho with the provided accessToken
            // (Assume AuthZoho is already set up with the correct token)
            try
            {
                var monitors = await _apiClient.GetMonitorListAsync();
                return JsonSerializer.Serialize(monitors);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch monitors: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
