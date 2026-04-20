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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Tridion.TCMRemote.Connection.Exceptions;

namespace Tridion.TCMRemote.Authentication
{
    /// <summary>
    /// Queries the Tridion Sites REST capabilities endpoint to discover the
    /// Access Management base URL required for OAuth2 / OIDC authentication.
    /// </summary>
    public static class TcmCapabilitiesHelper
    {
        private const string CapabilitiesPath = "/api/v1.0/system/capabilities";

        /// <summary>
        /// Returns the Access Management base URL configured on the Tridion Sites server.
        /// Handles both camelCase (<c>accessManagementUrl</c>) and display-name
        /// (<c>Access Management Url</c>) key formats returned by different server versions.
        /// </summary>
        /// <param name="tcmServerUrl">Root URL of the Tridion Sites server.</param>
        /// <returns>Access Management base URL.</returns>
        /// <exception cref="TcmClientException">
        /// Thrown when the capabilities endpoint is unreachable or when Access Management is not installed.
        /// </exception>
        public static string GetAccessManagementUrl(string tcmServerUrl)
        {
            var baseUrl = tcmServerUrl.TrimEnd('/');
            string json;

            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                var response = httpClient.GetAsync($"{baseUrl}{CapabilitiesPath}").GetAwaiter().GetResult();
                if (!response.IsSuccessStatusCode)
                    throw new TcmClientException(
                        $"Capabilities endpoint returned {(int)response.StatusCode} {response.ReasonPhrase}. " +
                        $"Ensure the Tridion Sites server is reachable at '{baseUrl}'.");

                json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            catch (TcmClientException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new TcmClientException(
                    $"Failed to query capabilities at '{baseUrl}{CapabilitiesPath}': {ex.Message}", ex);
            }

            var amUrl = ExtractAccessManagementUrl(json);
            if (string.IsNullOrWhiteSpace(amUrl))
                throw new TcmClientException(
                    "Access Management is not installed or not configured on this Tridion Sites server. " +
                    "Ensure the Access Management capability is enabled, or supply -AccessManagementUrl directly.");

            return amUrl!.TrimEnd('/');
        }

        // ------------------------------------------------------------------ parsing

        /// <summary>
        /// Extracts the Access Management URL from a capabilities JSON payload.
        /// Tolerates the key being either <c>accessManagementUrl</c> (camelCase property)
        /// or <c>Access Management Url</c> (display-name dictionary key) — the format changed
        /// across Tridion Sites server versions.
        /// </summary>
        internal static string? ExtractAccessManagementUrl(string json)
        {
            JObject root;
            try
            {
                root = JObject.Parse(json);
            }
            catch
            {
                return null;
            }

            // Try the Links dictionary (display-name key format used by current servers)
            // e.g. "Links": { "Access Management Url": "http://..." }
            var links = root["Links"] as JObject ?? root["links"] as JObject;
            if (links != null)
            {
                foreach (var prop in links.Properties())
                {
                    if (prop.Name.Replace(" ", "").Replace("_", "")
                            .Equals("AccessManagementUrl", StringComparison.OrdinalIgnoreCase))
                    {
                        var val = prop.Value.Value<string>();
                        if (!string.IsNullOrWhiteSpace(val))
                            return val;
                    }
                }
            }

            // Fallback: legacy camelCase property at root level
            // e.g. { "accessManagementUrl": "http://..." }
            var legacy = root["accessManagementUrl"]?.Value<string>()
                      ?? root["AccessManagementUrl"]?.Value<string>();
            return legacy;
        }
    }
}
