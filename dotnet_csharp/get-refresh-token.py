#!/usr/bin/env python3
"""
Helper script to obtain a refresh token using the device flow

Usage:
python get_refresh_token.py --client-id YOUR_CLIENT_ID --client-secret YOUR_CLIENT_SECRET --scope Site24x7.Admin.Read --server us
"""

import argparse
import requests
import time
import sys

# Account server URLs by region
ACCOUNT_SERVERS = {
    "eu": "https://accounts.zoho.eu",
    "ae": "https://accounts.zoho.ae",
    "au": "https://accounts.zoho.com.au",
    "in": "https://accounts.zoho.in",
    "jp": "https://accounts.zoho.jp",
    "uk": "https://accounts.zoho.uk",
    "us": "https://accounts.zoho.com",
    "ca": "https://accounts.zohocloud.ca",
    "sa": "https://accounts.zoho.sa"
}

sys.argv = [
    "get-refresh-token.py",
    "--client-id", "<>",
    "--client-secret", "<>",
    "--scope", "Site24x7.Admin.All,Site24x7.Reports.All,Site24x7.Account.All,Site24x7.Operations.All,Site24x7.Internal.All",
    "--server", "us"
]

def parse_arguments():
    parser = argparse.ArgumentParser(description="Obtain a refresh token using the device flow.")
    parser.add_argument("--client-id", required=True, help="Your client ID")
    parser.add_argument("--client-secret", required=True, help="Your client secret")
    parser.add_argument("--scope", required=True, help="Scope for the token (e.g., Site24x7.Admin.Read)")
    parser.add_argument("--server", default="us", help="Server region (default: us)")
    return parser.parse_args()

def start_device_flow(client_id, client_secret, scope, account_server_url):
    try:
        print(f"\nStarting device flow authentication with {account_server_url}...\n")

        # Step 1: Request device code
        device_code_response = requests.post(
            f"{account_server_url}/oauth/v3/device/code",
            params={
                "grant_type": "device_request",
                "client_id": client_id,
                "scope": scope,
                "access_type": "offline",
                "prompt": "consent"
            }
        )
        device_code_response.raise_for_status()
        data = device_code_response.json()

        user_code = data["user_code"]
        device_code = data["device_code"]
        verification_url = data["verification_url"]
        interval = 10 #data.get("interval", 30)
        expires_in = data["expires_in"]

        print(f"Please visit: {verification_url}")
        print(f"And enter the code: {user_code}")
        print(f"Device code is: {device_code}")
        print(f"\nThis code will expire in {expires_in / 60} minutes.")
        print("\nWaiting for authentication...")

        # Step 2: Poll for token
        expiry_time = time.time() + expires_in
        authenticated = False

        while time.time() < expiry_time and not authenticated:
            time.sleep(interval)
            try:
                token_response = requests.post(
                    f"{account_server_url}/oauth/v3/device/token",
                    params={
                        "client_id": client_id,
                        "client_secret": client_secret,
                        "grant_type": "device_token",
                        "code": device_code
                    }
                )
                if token_response.status_code == 200:
                    token_data = token_response.json()
                    if "access_token" in token_data and "refresh_token" in token_data:
                        authenticated = True
                        print("\nAuthentication successful!")
                        print("\n=== TOKEN INFORMATION ===")
                        print(f"Access Token: {token_data['access_token']}")
                        print(f"Refresh Token: {token_data['refresh_token']}")
                        print(f"Expires In: {token_data['expires_in']} seconds")
                        print(f"API Domain: {token_data['api_domain']}")

                        print("\n=== ENVIRONMENT VARIABLES FOR MCP SERVER ===")
                        print(f"CLIENT_ID={client_id}")
                        print(f"CLIENT_SECRET={client_secret}")
                        print(f"REFRESH_TOKEN={token_data['refresh_token']}")
                        print(f"ACCOUNT_SERVER_URL={account_server_url}")
                    else:
                        print("\nResponse does not contain required tokens. Continuing to poll...")
                elif token_response.status_code == 400:
                    # User hasn't authenticated yet, continue polling
                    sys.stdout.write(".")
                    sys.stdout.flush()
                else:
                    print("\nError polling for token:", token_response.text)
                    sys.exit(1)
            except requests.RequestException as e:
                print("\nError polling for token:", e)
                sys.exit(1)

        if not authenticated:
            print("\nAuthentication timed out. Please try again.")
            sys.exit(1)

    except requests.RequestException as e:
        print("Error:", e)
        sys.exit(1)

def main():
    args = parse_arguments()
    account_server_url = ACCOUNT_SERVERS.get(args.server)

    if not account_server_url:
        print(f"Error: Invalid server region '{args.server}'. Valid regions are: {', '.join(ACCOUNT_SERVERS.keys())}")
        sys.exit(1)

    start_device_flow(args.client_id, args.client_secret, args.scope, account_server_url)

if __name__ == "__main__":
    main()