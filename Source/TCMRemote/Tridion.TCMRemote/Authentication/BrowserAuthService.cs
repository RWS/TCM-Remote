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

using System.Threading;
using System.Threading.Tasks;
using Tridion.TCMRemote.Connection;
using Tridion.TCMRemote.Interfaces;

namespace Tridion.TCMRemote.Authentication
{
    /// <summary>
    /// Authentication service that uses the OAuth2 Authorization Code flow with PKCE,
    /// opening the system browser for interactive user login.
    /// Delegates all token management to <see cref="TcmAccessManagementClient"/>.
    /// </summary>
    public sealed class BrowserAuthService : IAuthenticationService
    {
        /// <summary>
        /// Default client identifier for interactive desktop/PowerShell use, as registered
        /// in Access Management with <c>http://127.0.0.1</c> as an allowed redirect URI.
        /// </summary>
        public const string DefaultClientId = "Tridion_Sites_Desktop_Client";

        private readonly TcmAccessManagementClient _tamClient;
        private readonly string _clientId;

        /// <summary>
        /// Initialises a new <see cref="BrowserAuthService"/>.
        /// </summary>
        /// <param name="accessManagementUrl">
        /// Access Management base URL obtained via <see cref="TcmCapabilitiesHelper.GetAccessManagementUrl"/>.
        /// </param>
        /// <param name="clientId">
        /// OIDC client identifier; defaults to <see cref="DefaultClientId"/>.
        /// </param>
        /// <param name="userAuthListener">Optional UI listener for auth start/finish events.</param>
        public BrowserAuthService(
            string accessManagementUrl,
            string clientId = DefaultClientId,
            ITcmUserAuthListener? userAuthListener = null)
        {
            _tamClient = new TcmAccessManagementClient(accessManagementUrl)
            {
                UserAuthListener = userAuthListener
            };
            _clientId = clientId;
        }

        /// <inheritdoc/>
        public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
            => _tamClient.GetAccessTokenForUserAsync(_clientId, cancellationToken);
    }
}
