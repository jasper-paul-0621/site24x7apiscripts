param(
    [Parameter(Mandatory=$true)][string]$clientid,
    [Parameter(Mandatory=$true)][string]$clientsecret,
    [string]$scope = "Site24x7.Admin.All,Site24x7.Reports.All,Site24x7.Account.All,Site24x7.Operations.All,Site24x7.Internal.All",
    [string]$server = "us"
)

$ACCOUNT_SERVERS = @{
    "eu" = "https://accounts.zoho.eu"
    "ae" = "https://accounts.zoho.ae"
    "au" = "https://accounts.zoho.com.au"
    "in" = "https://accounts.zoho.in"
    "jp" = "https://accounts.zoho.jp"
    "uk" = "https://accounts.zoho.uk"
    "us" = "https://accounts.zoho.com"
    "ca" = "https://accounts.zohocloud.ca"
    "sa" = "https://accounts.zoho.sa"
}

if (-not $ACCOUNT_SERVERS.ContainsKey($server)) {
    Write-Host "Error: Invalid server region '$server'. Valid regions are: $($ACCOUNT_SERVERS.Keys -join ', ')"
    exit 1
}

$account_server_url = $ACCOUNT_SERVERS[$server]
Write-Host "`nStarting device flow authentication with $account_server_url...`n"

# Step 1: Request device code
$device_code_response = Invoke-RestMethod -Method Post -Uri "$account_server_url/oauth/v3/device/code" -Body @{
    grant_type = "device_request"
    client_id = $clientid
    scope = $scope
    access_type = "offline"
    prompt = "consent"
}

$user_code = $device_code_response.user_code
$device_code = $device_code_response.device_code
$verification_url = $device_code_response.verification_url
$interval = 10
$expires_in = $device_code_response.expires_in

Write-Host "Please visit: $verification_url"
Write-Host "And enter the code: $user_code"
Write-Host "Device code is: $device_code"
Write-Host "`nThis code will expire in $($expires_in / 60) minutes."
Write-Host "`nWaiting for authentication..."

$expiry_time = (Get-Date).AddSeconds($expires_in)
$authenticated = $false

while ((Get-Date) -lt $expiry_time -and -not $authenticated) {
    Start-Sleep -Seconds $interval
    try {
        $token_response = Invoke-RestMethod -Method Post -Uri "$account_server_url/oauth/v3/device/token" -Body @{
            client_id = $clientid
            client_secret = $clientsecret
            grant_type = "device_token"
            code = $device_code
        }
        if ($token_response.access_token -and $token_response.refresh_token) {
            $authenticated = $true
            Write-Host "`nAuthentication successful!"
            Write-Host "`n=== TOKEN INFORMATION ==="
            Write-Host "Access Token: $($token_response.access_token)"
            Write-Host "Refresh Token: $($token_response.refresh_token)"
            Write-Host "Expires In: $($token_response.expires_in) seconds"
            Write-Host "API Domain: $($token_response.api_domain)"
            Write-Host "`n=== ENVIRONMENT VARIABLES FOR MCP SERVER ==="
            Write-Host "CLIENT_ID=$clientid"
            Write-Host "CLIENT_SECRET=$clientsecret"
            Write-Host "REFRESH_TOKEN=$($token_response.refresh_token)"
            Write-Host "ACCOUNT_SERVER_URL=$account_server_url"
        } else {
            Write-Host -NoNewline "."
        }
    } catch {
        if ($_.Exception.Response.StatusCode.Value__ -eq 400) {
            Write-Host -NoNewline "."
        } else {
            Write-Host "`nError polling for token: $($_.Exception.Message)"
            exit 1
        }
    }
}

if (-not $authenticated) {
    Write-Host "`nAuthentication timed out. Please try again."
    exit 1
}
