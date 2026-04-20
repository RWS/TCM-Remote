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

namespace Tridion.TCMRemote.Connection
{
    /// <summary>
    /// Callback interface for components that wish to react to interactive browser
    /// authentication events (e.g. showing/hiding a "Please log in" dialog).
    /// </summary>
    public interface ITcmUserAuthListener
    {
        /// <summary>Called immediately before the browser is opened for authentication.</summary>
        /// <param name="cancellationTokenSource">
        /// Token source that can be cancelled to abort the pending authentication.
        /// </param>
        void OnAuthStarting(CancellationTokenSource cancellationTokenSource);

        /// <summary>Called after authentication has completed or been cancelled.</summary>
        void OnAuthFinished();
    }
}
