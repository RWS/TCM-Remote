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

namespace Tridion.TCMRemote.Connection.Exceptions
{
    /// <summary>General exception thrown by the TCMRemote connection and authentication layer.</summary>
    public class TcmClientException : ApplicationException
    {
        /// <inheritdoc/>
        public TcmClientException(string message) : base(message) { }

        /// <inheritdoc/>
        public TcmClientException(string message, Exception innerException) : base(message, innerException) { }
    }
}
