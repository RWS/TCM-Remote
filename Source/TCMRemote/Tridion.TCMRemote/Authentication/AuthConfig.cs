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
// TridionCoreService/Authentication/AuthConfig.cs
using Newtonsoft.Json;

namespace Tridion.TCMRemote.Authentication
{
    public class AuthConfig
    {
        public string BaseUrl { get; set; }
        public string ClientId { get; set; } = "Tridion_Sites_Desktop_Client";
        public string ClientSecret { get; set; }
        public string Authority { get; set; }
        public string RedirectUri { get; set; } = "http://127.0.0.1"; // Default to match TAM config
        public string Scope { get; set; } = "openid profile email role forwarded offline_access";
    }

    public class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("id_token")]
        public string IdToken { get; set; }
    }
}