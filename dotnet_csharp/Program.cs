﻿using Site24x7CustomReports;
using Site24x7Integration;
using System.Text.Json;

/// <summary>
/// Entry point for the Site24x7CustomReports application.
/// </summary>
internal class Program
{
    /// <summary>
    /// Main entry point. Handles refresh token retrieval and monitor listing logic.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    private static async Task Main(string[] args)
    {
        // With these lines:
        var CLIENT_ID = TokenConstants.CLIENT_ID;
        var CLIENT_SECRET = TokenConstants.CLIENT_SECRET;
        string REFRESH_TOKEN = TokenConstants.REFRESH_TOKEN;
        var ACCOUNT_SERVER_URL = TokenConstants.ACCOUNT_SERVER_URL;

        //if (args.Length > 0 && args[0].Equals("get-refresh-token", StringComparison.OrdinalIgnoreCase))
        if (string.IsNullOrEmpty(REFRESH_TOKEN))
        {
            var clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
            var scope = args.Length > 1 ? args[1] : "Site24x7.Admin.Read";
            var server = args.Length > 2 ? args[2] : ACCOUNT_SERVER_URL;
            var device = new DeviceFlow();
            string? newRefreshToken = await device.StartDeviceFlowAndGetRefreshTokenAsync(CLIENT_ID, CLIENT_SECRET, scope, server);
            if (!string.IsNullOrEmpty(newRefreshToken))
            {
                TokenConstants.REFRESH_TOKEN = newRefreshToken;
                REFRESH_TOKEN = newRefreshToken;
                await ListMonitors.RunAsync();
            }
            else
            {
                Console.WriteLine("Failed to obtain refresh token.");
            }
        }
        else
        {
            await ListMonitors.RunAsync();
        }
    }
}