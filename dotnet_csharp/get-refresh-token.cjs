#!/usr/bin/env node
/**
 * Helper script to obtain a refresh token using the device flow
 * 
 * Usage:
 * node get-refresh-token.js --client-id=YOUR_CLIENT_ID --client-secret=YOUR_CLIENT_SECRET --scope=Site24x7.Admin.Read --server=us
 */

const axios = require('axios');
const readline = require('readline');

// Account server URLs by region
const ACCOUNT_SERVERS = {
  "eu": "https://accounts.zoho.eu",
  "ae": "https://accounts.zoho.ae",
  "au": "https://accounts.zoho.com.au",
  "in": "https://accounts.zoho.in",
  "jp": "https://accounts.zoho.jp",
  "uk": "https://accounts.zoho.uk",
  "us": "https://accounts.zoho.com",
  "ca": "https://accounts.zohocloud.ca",
  "sa": "https://accounts.zoho.sa"
};

// Parse command line arguments
const args = process.argv.slice(2).reduce((acc, arg) => {
  if (arg.startsWith('--')) {
    const [key, value] = arg.substring(2).split('=');
    acc[key] = value;
  }
  return acc;
}, {});

// Validate required arguments
if (!args['client-id']) {
  console.error('Error: --client-id is required');
  process.exit(1);
}

if (!args['client-secret']) {
  console.error('Error: --client-secret is required');
  process.exit(1);
}

if (!args.scope) {
  console.error('Error: --scope is required (e.g., Site24x7.Admin.Read)');
  process.exit(1);
}

// Get account server URL
const serverRegion = args.server || 'us';
const accountServerUrl = ACCOUNT_SERVERS[serverRegion];

if (!accountServerUrl) {
  console.error(`Error: Invalid server region "${serverRegion}". Valid regions are: ${Object.keys(ACCOUNT_SERVERS).join(', ')}`);
  process.exit(1);
}

// Create readline interface for user input
const rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout
});

async function startDeviceFlow() {
  try {
    console.log(`\nStarting device flow authentication with ${accountServerUrl}...\n`);
    
    // Step 1: Request device code
    const deviceCodeResponse = await axios.post(
      `${accountServerUrl}/oauth/v3/device/code`,
      null,
      {
        params: {
          grant_type: 'device_request',
          client_id: args['client-id'],
          scope: args.scope,
          access_type: 'offline',
          prompt: 'consent'
        }
      }
    );
    
    const { user_code, device_code, verification_url, interval, expires_in } = deviceCodeResponse.data;
    
    console.log(`Please visit: ${verification_url}`);
    console.log(`And enter the code: ${user_code}`);
    console.log(`Device code is: ${device_code}`);
    console.log(`\nThis code will expire in ${expires_in / 1000 / 60} minutes.`);
    console.log(`\nWaiting for authentication...`);
    
    // Step 2: Poll for token
    const intervalMs = interval || 30000; // Default to 30 seconds if not provided
    const startTime = Date.now();
    const expiryTime = startTime + expires_in * 1000;
    
    let authenticated = false;
    
    while (Date.now() < expiryTime && !authenticated) {
      try {
        await new Promise(resolve => setTimeout(resolve, intervalMs));
        
        const tokenResponse = await axios.post(
          `${accountServerUrl}/oauth/v3/device/token`,
          null,
          {
            params: {
              client_id: args['client-id'],
              client_secret: args['client-secret'],
              grant_type: 'device_token',
              code: device_code
            }
          }
        );
        
        console.log('\nReceived response from token endpoint:');
        console.log(JSON.stringify(tokenResponse.data, null, 2));

        // Check if tokenResponse contains the required data
        if (tokenResponse.data && tokenResponse.data.access_token && tokenResponse.data.refresh_token) {
          authenticated = true;

          console.log('\nAuthentication successful!');
          console.log('\n=== TOKEN INFORMATION ===');
          console.log(`Access Token: ${tokenResponse.data.access_token}`);
          console.log(`Refresh Token: ${tokenResponse.data.refresh_token}`);
          console.log(`Expires In: ${tokenResponse.data.expires_in} seconds`);
          console.log(`API Domain: ${tokenResponse.data.api_domain}`);
          
          console.log('\n=== ENVIRONMENT VARIABLES FOR MCP SERVER ===');
          console.log(`CLIENT_ID=${args['client-id']}`);
          console.log(`CLIENT_SECRET=${args['client-secret']}`);
          console.log(`REFRESH_TOKEN=${tokenResponse.data.refresh_token}`);
          console.log(`ACCOUNT_SERVER_URL=${accountServerUrl}`);
        } else {
          console.log('\nResponse does not contain required tokens. Continuing to poll...');
        }        
      } catch (error) {
        if (error.response && error.response.status === 400) {
          // User hasn't authenticated yet, continue polling
          process.stdout.write('.');
        } else {
          // Other error
          console.error('\nError polling for token:', error);
          if (error.response && error.response.data) {
            console.error('Response:', error.response.data);
          }
          process.exit(1);
        }
      }
    }
    
    if (!authenticated) {
      console.error('\nAuthentication timed out. Please try again.');
      process.exit(1);
    }
    
  } catch (error) {
    console.error('Error:', error.message);
    if (error.response && error.response.data) {
      console.error('Response:', error.response.data);
    }
    process.exit(1);
  } finally {
    rl.close();
  }
}

startDeviceFlow();
