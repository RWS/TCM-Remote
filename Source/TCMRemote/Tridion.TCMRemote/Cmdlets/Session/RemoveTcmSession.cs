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
using System.Management.Automation;
using Tridion.TCMRemote.Objects.Public;

namespace Tridion.TCMRemote.Cmdlets.Session
{
    /// <summary>
    /// <para type="synopsis">Closes and removes a TcmSession from the current PowerShell session.</para>
    /// <para type="description">Remove-TcmSession disposes the session's HTTP client and removes the session variable from the current PowerShell session state.</para>
    /// </summary>
    /// <example>
    /// <code>Remove-TcmSession</code>
    /// <para>Removes the active session.</para>
    /// </example>
    /// <example>
    /// <code>Get-TcmSession | Remove-TcmSession</code>
    /// <para>Retrieves and removes the active session via the pipeline.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "TcmSession", SupportsShouldProcess = true)]
    public sealed class RemoveTcmSession : SessionCmdlet
    {
        /// <summary>
        /// <para type="description">The session to remove. When omitted, the stored session is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public TcmSession? TcmSession { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var session = TcmSession
                    ?? SessionState.PSVariable.Get(TCMRemoteSessionStateTcmSession)?.Value as TcmSession;

                if (session == null)
                {
                    WriteVerbose("No active TcmSession found — nothing to remove.");
                    return;
                }

                if (!ShouldProcess(session.Name, "Remove TcmSession"))
                    return;

                WriteVerbose($"Removing TcmSession: {session.Name}");
                session.Dispose();
                SessionState.PSVariable.Remove(TCMRemoteSessionStateTcmSession);
                WriteVerbose("TcmSession removed.");
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "TcmSessionRemoveFailed", ErrorCategory.ResourceUnavailable, null));
            }
        }
    }
}
