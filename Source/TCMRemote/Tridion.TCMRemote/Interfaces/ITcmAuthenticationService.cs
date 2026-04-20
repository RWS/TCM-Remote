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

namespace Tridion.TCMRemote.Interfaces
{
    /// <summary>
    /// Extended authentication contract, used for advanced scenarios that need
    /// explicit client-credential overrides or token validation.
    /// For most cmdlets, <see cref="IAuthenticationService"/> is sufficient.
    /// </summary>
    public interface ITcmAuthenticationService : IAuthenticationService
    {
        /// <summary>
        /// Returns a bearer token using the Client Credentials flow with the supplied credentials,
        /// bypassing any cached token.
        /// </summary>
        Task<string> GetAccessTokenAsync(string clientId, string clientSecret, CancellationToken cancellationToken = default);

        /// <summary>Validates whether the given bearer token is still accepted by the server.</summary>
        Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    }
}
