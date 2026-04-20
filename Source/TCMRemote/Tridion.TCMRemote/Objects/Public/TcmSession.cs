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

using System;
using Tridion.TCMRemote.Clients;

namespace Tridion.TCMRemote.Objects.Public
{
    /// <summary>
    /// <para type="description">Client session object to the Tridion Sites server instance required for every remote operation as it holds the rest service url and authentication.</para>
    /// <para type="description">Furthermore it tracks your security token, provides direct client access to the rest services API.</para>
    /// <para type="description">Gives access to contract parameters like separators, date formats, batch and chunk sizes.</para>
    /// </summary>
    public class TcmSession : IDisposable
    {
        /// <summary>
        /// Access Management Client Application Id that is typically configured in Access Management to allow a local redirect (http://127.0.0.1:SomePort/)
        /// This option is not typically used but allows validating other applications like Tridion_Sites_Content_Porter
        /// </summary>
        private readonly TridionCoreServiceClient _client;
        private readonly string _baseUrl;
        private readonly string _authenticationType;
        private readonly DateTime _createdAt;

        /// <summary>
        /// Creates a session object holding contracts and proxies to the rest services API. Takes care of username/password and 'Active Directory' authentication (NetworkCredential) to the Secure Token Service.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="baseUrl"></param>
        /// <param name="authenticationType"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public TcmSession(TridionCoreServiceClient client, string baseUrl, string authenticationType)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            _authenticationType = authenticationType;
            _createdAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets the Tridion Core Service client
        /// </summary>
        public TridionCoreServiceClient Client => _client;

        /// <summary>
        /// Gets the base URL
        /// </summary>
        public string BaseUrl => _baseUrl;

        /// <summary>
        /// Gets the authentication type used
        /// </summary>
        public string AuthenticationType => _authenticationType;

        /// <summary>
        /// Gets the session creation time
        /// </summary>
        public DateTime CreatedAt => _createdAt;

        /// <summary>
        /// Gets the session name for identification
        /// </summary>
        public string Name => $"TcmSession_{_createdAt:yyyyMMddHHmmss}";

        public void Dispose()
        {
            _client?.Dispose();
        }
    }

}
