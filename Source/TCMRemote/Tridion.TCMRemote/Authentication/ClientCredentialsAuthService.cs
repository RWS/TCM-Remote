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
    /// Authentication service that uses the OAuth2 Client Credentials flow.
    /// Suitable for server-to-server automation where no human interaction is required.
    /// Delegates all token management to <see cref="TcmAccessManagementClient"/>.
    /// </summary>
    public sealed class ClientCredentialsAuthService : IAuthenticationService
    {
        private readonly TcmAccessManagementClient _tamClient;
        private readonly string _clientId;
        private readonly string _clientSecret;

        /// <summary>
        /// Initialises a new <see cref="ClientCredentialsAuthService"/>.
        /// </summary>
        /// <param name="clientId">OAuth2 client identifier.</param>
        /// <param name="clientSecret">OAuth2 client secret.</param>
        /// <param name="accessManagementUrl">
        /// Access Management base URL obtained via <see cref="TcmCapabilitiesHelper.GetAccessManagementUrl"/>.
        /// </param>
        public ClientCredentialsAuthService(string clientId, string clientSecret, string accessManagementUrl)
        {
            _tamClient     = new TcmAccessManagementClient(accessManagementUrl);
            _clientId      = clientId;
            _clientSecret  = clientSecret;
        }

        /// <inheritdoc/>
        public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
            => _tamClient.GetAccessTokenForClientCredentialsAsync(_clientId, _clientSecret, cancellationToken);
    }
}
