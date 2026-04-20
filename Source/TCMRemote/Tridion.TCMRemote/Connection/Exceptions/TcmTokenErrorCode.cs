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
namespace Tridion.TCMRemote.Connection.Exceptions
{
    /// <summary>OAuth2/OIDC token error codes as defined by RFC 6749.</summary>
    public enum TcmTokenErrorCode
    {
        /// <summary>Unknown or unmapped error.</summary>
        Unknown,
        /// <summary>The request is missing a required parameter or is otherwise malformed.</summary>
        InvalidRequest,
        /// <summary>Client authentication failed.</summary>
        InvalidClient,
        /// <summary>The provided authorization grant or refresh token is invalid.</summary>
        InvalidGrant,
        /// <summary>The authenticated client is not authorized to use this grant type.</summary>
        UnauthorizedClient,
        /// <summary>The authorization grant type is not supported by the authorization server.</summary>
        UnsupportedGrantType,
        /// <summary>The requested scope is invalid, unknown, malformed, or exceeds what was granted.</summary>
        InvalidScope
    }
}
