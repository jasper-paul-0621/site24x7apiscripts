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

### 2. Configure Authentication
Create a file named `site24x7_auth.txt` in the project directory with the following content:
```
CLIENT_ID=your_client_id
CLIENT_SECRET=your_client_secret
REFRESH_TOKEN=your_refresh_token
```

Alternatively, you can set these values in `TokenConstants.cs` for testing, but using the auth file is recommended for security.

### 3. Build the Project
```sh
dotnet build
```

### 4. Run the Project
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

## Security Note
**Do not commit your `site24x7_auth.txt` or any real credentials to version control.**

## License
MIT License
