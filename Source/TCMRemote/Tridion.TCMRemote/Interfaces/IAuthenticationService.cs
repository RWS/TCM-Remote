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
    /// Minimal contract for authentication services used by <see cref="Clients.TridionCoreServiceClient"/>.
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>Returns a valid bearer access token, acquiring or refreshing it as needed.</summary>
        Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    }
}
