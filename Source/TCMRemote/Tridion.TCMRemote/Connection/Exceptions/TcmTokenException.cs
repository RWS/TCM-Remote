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
    /// <summary>
    /// Exception thrown when an OAuth2/OIDC token operation fails.
    /// Carries the protocol-level <see cref="TcmTokenErrorCode"/> to allow callers to react
    /// to specific failure reasons (e.g. re-prompt credentials on <c>InvalidGrant</c>).
    /// </summary>
    public class TcmTokenException : TcmClientException
    {
        /// <summary>Gets the OAuth2 error code returned by the authorization server.</summary>
        public TcmTokenErrorCode TokenErrorCode { get; }

        /// <inheritdoc/>
        public TcmTokenException(string message, TcmTokenErrorCode tokenErrorCode)
            : base(message)
        {
            TokenErrorCode = tokenErrorCode;
        }
    }
}
