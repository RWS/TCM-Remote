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

using System.Management.Automation;
using Tridion.TCMRemote.Objects.Public;

namespace Tridion.TCMRemote.Cmdlets.Session
{
    /// <summary>
    /// <para type="synopsis">Returns the current TcmSession stored in the PowerShell session state.</para>
    /// <para type="description">Get-TcmSession retrieves the session object that was created by New-TcmSession and stored automatically. Returns $null if no session is active.</para>
    /// </summary>
    /// <example>
    /// <code>$tcmSession = Get-TcmSession</code>
    /// <para>Retrieves the active session.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "TcmSession")]
    [OutputType(typeof(TcmSession))]
    public sealed class GetTcmSession : SessionCmdlet
    {
        protected override void ProcessRecord()
        {
            var variable = SessionState.PSVariable.Get(TCMRemoteSessionStateTcmSession);
            var session  = variable?.Value as TcmSession;

            if (session == null)
            {
                WriteVerbose("No active TcmSession found in session state.");
                WriteObject(null);
            }
            else
            {
                WriteVerbose($"Found active TcmSession: {session.Name}");
                WriteObject(session);
            }
        }
    }
}
