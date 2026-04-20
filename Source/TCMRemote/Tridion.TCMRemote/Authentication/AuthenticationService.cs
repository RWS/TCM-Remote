// Copyright (c) 2024-2026 RWS Holdings plc and its subsidiaries.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// TridionCoreService/Authentication/AuthenticationService.cs
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Tridion.TCMRemote.Models;
using Tridion.TCMRemote.Interfaces;
using Tridion.TCMRemote.HelperClasses;

namespace Tridion.TCMRemote.Authentication
{
    public class AuthenticationService
    {
        private readonly AuthConfig _config;
        private readonly HttpClient _httpClient;
        private string _accessToken;
        private DateTime _tokenExpiry;
        private string _authorizationEndpoint;
        private string _tokenEndpoint;
        private string _userInfoEndpoint;

        public AuthenticationService(AuthConfig config)
        {
            _config = config;
            _httpClient = new HttpClient();

            // Discover endpoints from OpenID configuration
            DiscoverEndpoints().GetAwaiter().GetResult();
        }

        private async Task DiscoverEndpoints()
        {
            try
            {
                _config.Authority = GetAccessManagementUrlIfEnabled(_config.BaseUrl);
                var response = await _httpClient.GetAsync($"{_config.Authority}/.well-known/openid-configuration");
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    JObject config = JObject.Parse(json);
                    _authorizationEndpoint = config["authorization_endpoint"]?.ToString();
                    _tokenEndpoint = config["token_endpoint"]?.ToString();
                    _userInfoEndpoint = config["userinfo_endpoint"]?.ToString();
                }
                else
                {
                    // Fallback to default endpoints if discovery fails
                    _authorizationEndpoint = $"{_config.Authority}/connect/authorize";
                    _tokenEndpoint = $"{_config.Authority}/connect/token";
                    _userInfoEndpoint = $"{_config.Authority}/connect/userinfo";
                }
            }
            catch (Exception ex)
            {
                // Fallback to default endpoints
                _authorizationEndpoint = $"{_config.Authority}/connect/authorize";
                _tokenEndpoint = $"{_config.Authority}/connect/token";
                _userInfoEndpoint = $"{_config.Authority}/connect/userinfo";
                System.Diagnostics.Debug.WriteLine($"OpenID discovery failed, using defaults: {ex.Message}");
            }
        }

        /// <summary>Gets the access management URL if enabled.</summary>
        /// <param name="tcmServerUrl">The TCM server URL.</param>
        /// <returns>Access Management URI</returns>
        /// <exception cref="System.Exception">Access Management Capability not installed</exception>
        public string GetAccessManagementUrlIfEnabled(string tcmServerUrl)
        {
            Capabilities capabilities = GetCapabilities(tcmServerUrl);

            if (string.IsNullOrEmpty(capabilities.Links.AccessManagementUrl))
                throw new Exception("Access Management Capability not installed");

            return capabilities.Links.AccessManagementUrl;
        }

        /// <summary>Gets installed Tridion Capabilities via Core Service REST</summary>
        /// <param name="tcmServerUrl">The TCM server URL.</param>
        /// <returns>
        ///   Capabilities Class
        /// </returns>
        private Capabilities GetCapabilities(string tcmServerUrl)
        {
            var capabilities = new Capabilities();

            if (tcmServerUrl.EndsWith("/"))
                tcmServerUrl.Remove(tcmServerUrl.Length - 1);

            using (HttpClient httpClient = new HttpClient())
            {
                // This is actually talking to the ***REST*** version of the CORE SERVICE
                var response = httpClient.GetAsync($"{tcmServerUrl}/api/v1.0/system/capabilities").GetAwaiter().GetResult();
                var jsonContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;

                capabilities = JsonConvert.DeserializeObject<Capabilities>(jsonContent, settings);
            }

            return capabilities;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
            {
                return _accessToken;
            }

            // Browser-based authentication with PKCE (like your working code)
            return await GetTokenViaBrowserWithPkceAsync();
        }

        public async Task<string> GetAccessTokenAsync(string clientId, string clientSecret)
        {
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
            {
                return _accessToken;
            }

            // Client credentials flow
            return await GetTokenViaClientCredentialsAsync(clientId, clientSecret);
        }

        private async Task<string> GetTokenViaBrowserWithPkceAsync()
        {
            // Generate PKCE code verifier and challenge (like your working code)
            string codeVerifier = GenerateCodeVerifier();
            string codeChallenge = ComputeCodeChallenge(codeVerifier);

            // Use localhost redirect URI as per your TAM configuration
            string redirectUri = "http://127.0.0.1"; // Matches your TAM client configuration

            // Build authorization URL with PKCE (like your working code)
            var authUrl = $"{_authorizationEndpoint}?" +
                $"client_id={Uri.EscapeDataString(_config.ClientId)}" +
                $"&response_type=code" +
                $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                $"&scope={Uri.EscapeDataString(_config.Scope)}" +
                $"&code_challenge={codeChallenge}" +
                $"&code_challenge_method=S256";

            // Start local HTTP listener for callback
            var listener = new HttpListener();
            listener.Prefixes.Add(redirectUri + "/");

            try
            {
                listener.Start();

                // Open browser for authentication
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = authUrl,
                    UseShellExecute = true
                });

                // Wait for callback with timeout
                var timeoutTask = Task.Delay(TimeSpan.FromMinutes(2)); // 2 minute timeout
                var contextTask = listener.GetContextAsync();

                var completedTask = await Task.WhenAny(contextTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException("Authentication timeout - no response received from browser");
                }

                var context = await contextTask;
                var request = context.Request;
                var response = context.Response;

                // Extract authorization code - FIXED LINE
                var query = QueryStringHelper.ParseQueryString(request.Url.Query);
                var code = query["code"];

                if (string.IsNullOrEmpty(code))
                {
                    var error = query["error"];
                    var errorDescription = query["error_description"];
                    throw new Exception($"Authentication failed: {error} - {errorDescription}");
                }

                // Send success response to browser
                var responseString = "<html><body><h2>Authentication Successful!</h2><p>You can close this window and return to PowerShell.</p></body></html>";
                var buffer = Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                response.ContentType = "text/html";
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.Close();

                // Exchange code for token using PKCE
                return await ExchangeCodeForTokenWithPkceAsync(code, codeVerifier, redirectUri);
            }
            finally
            {
                listener.Stop();
                listener.Close();
            }
        }

        private async Task<string> GetTokenViaClientCredentialsAsync(string clientId, string clientSecret)
        {
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, _tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("scope", _config.Scope)
                })
            };

            var response = await _httpClient.SendAsync(tokenRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Client credentials authentication failed: {response.StatusCode} - {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(json);

            _accessToken = tokenResponse.AccessToken;
            _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // 60 second buffer

            return _accessToken;
        }

        private async Task<string> ExchangeCodeForTokenWithPkceAsync(string code, string codeVerifier, string redirectUri)
        {
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, _tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("client_id", _config.ClientId),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("code_verifier", codeVerifier) // PKCE
                })
            };

            var response = await _httpClient.SendAsync(tokenRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Token exchange failed: {response.StatusCode} - {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(json);

            _accessToken = tokenResponse.AccessToken;
            _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);

            return _accessToken;
        }

        // PKCE methods from your working code
        private static string GenerateCodeVerifier()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[32];
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes)
                    .TrimEnd('=')
                    .Replace('+', '-')
                    .Replace('/', '_');
            }
        }

        private static string ComputeCodeChallenge(string codeVerifier)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                return Convert.ToBase64String(bytes)
                    .TrimEnd('=')
                    .Replace('+', '-')
                    .Replace('/', '_');
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                // Option 1: Test against CoreService whoAmI endpoint
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_config.BaseUrl}/whoAmI");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<JObject> GetUserInfoAsync(string accessToken)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                var response = await client.GetStringAsync(_userInfoEndpoint);
                return JObject.Parse(response);
            }
        }
    }
}