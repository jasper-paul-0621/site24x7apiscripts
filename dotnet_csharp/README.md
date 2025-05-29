# Site24x7 C# FNF Integration

This project provides a C# implementation for integrating with the Site24x7 API, supporting monitor listing and export functionality. It demonstrates how to authenticate with Zoho OAuth, retrieve monitor data, and export it in various formats (CSV, JSON, PDF).

## Features
- Authenticate with Zoho OAuth using device flow or refresh token
- Read credentials from a `site24x7_auth.txt` file
- List all monitors from your Site24x7 account
- Export monitor data to CSV, JSON, or PDF

## Prerequisites
- .NET 8.0 SDK or later
- A Site24x7 account with API access
- Zoho OAuth client credentials (Client ID, Client Secret, Refresh Token)

## Getting Started

### 1. Clone the Repository
```sh
git clone https://github.com/your-username/site24x7apiscripts.git
cd site24x7apiscripts/dotnet_csharp
```

### 2. Generate a Zoho Refresh Token (Recommended)
To use the API, you need a Zoho OAuth refresh token. This project provides a helper script to obtain one using the device flow.

#### Steps to Generate a Refresh Token
1. **Register your client in the Zoho API Console:**
   - Go to the [Zoho API Console](https://api-console.zoho.com/) and create a new client (choose 'Server-based Applications').
   - For more details, see the [Site24x7 API Authentication Guide](https://www.site24x7.com/help/api/#authentication).
2. **Run the helper script:**
   ```sh
   python get-refresh-token.py --client-id YOUR_CLIENT_ID --client-secret YOUR_CLIENT_SECRET --scope "Site24x7.Admin.All,Site24x7.Reports.All,Site24x7.Account.All,Site24x7.Operations.All,Site24x7.Internal.All" --server us
   ```
   - Replace `YOUR_CLIENT_ID` and `YOUR_CLIENT_SECRET` with your values from the Zoho API Console.
   - Adjust the `--scope` and `--server` as needed (see script for region options).
3. **Follow the instructions in the script output:**
   - Visit the provided verification URL and enter the user code.
   - After authentication, the script will display your refresh token and environment variables.

### 3. Configure Authentication
Create a file named `site24x7_auth.txt` in the project directory with the following content:
```
CLIENT_ID=your_client_id
CLIENT_SECRET=your_client_secret
REFRESH_TOKEN=your_refresh_token
```

Alternatively, you can set these values in `TokenConstants.cs` for testing, but using the auth file is recommended for security.

### 4. Build the Project
```sh
dotnet build
```

### 5. Run the Project
```sh
dotnet run
```

The application will authenticate and export the list of monitors to `monitors.csv` by default.

## Export Formats
- **CSV**: `monitors.csv`
- **JSON**: `monitors.json`
- **PDF**: `monitors.pdf`

You can change the export format in `ListMonitors.cs` by modifying the `exportFormat` variable.

## Project Structure
- `AuthZoho.cs`: Handles Zoho OAuth authentication and reading credentials from file
- `ListMonitors.cs`: Main logic for retrieving and exporting monitor data
- `ExportUtils.cs`: Utilities for exporting data in different formats
- `TokenConstants.cs`: (Optional) Hardcoded credentials for quick testing
- `site24x7_auth.txt`: Credentials file (not committed to version control)
- `get-refresh-token.py`: Helper script to generate a refresh token using device flow

## Security Note
**Do not commit your `site24x7_auth.txt` or any real credentials to version control.**

## References
- [Zoho API Console](https://api-console.zoho.com/)
- [Site24x7 API Authentication Guide](https://www.site24x7.com/help/api/#authentication)

## License
MIT License
