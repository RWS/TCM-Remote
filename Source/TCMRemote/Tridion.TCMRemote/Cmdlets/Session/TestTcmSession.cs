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
    /// <para type="synopsis">Tests whether a TcmSession is still valid by pinging the server.</para>
    /// <para type="description">Test-TcmSession verifies that the session can reach the Tridion Sites server and that the access token is still accepted. Returns $true on success or $false (with a warning) on failure.</para>
    /// </summary>
    /// <example>
    /// <code>Test-TcmSession</code>
    /// <para>Tests the session stored in the current PowerShell session.</para>
    /// </example>
    /// <example>
    /// <code>Test-TcmSession -TcmSession $tcmSession</code>
    /// <para>Tests an explicit session object.</para>
    /// </example>
    [Cmdlet(VerbsDiagnostic.Test, "TcmSession")]
    [OutputType(typeof(bool))]
    public sealed class TestTcmSession : SessionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to test. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public TcmSession? TcmSession { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var session = TcmSession ?? GetSessionFromState();
                if (session == null)
                {
                    WriteWarning(TCMRemoteSessionStateTcmSessionException);
                    WriteObject(false);
                    return;
                }

                // Ping the server by retrieving the current user — lightweight and auth-dependent
                var _ = session.Client.GetCurrentUserAsync().GetAwaiter().GetResult();
                WriteVerbose($"Session '{session.Name}' is valid.");
                WriteObject(true);
            }
            catch (Exception ex)
            {
                WriteWarning($"Session test failed: {ex.Message}");
                WriteObject(false);
            }
        }

        private TcmSession? GetSessionFromState()
        {
            var variable = SessionState.PSVariable.Get(TCMRemoteSessionStateTcmSession);
            return variable?.Value as TcmSession;
        }
    }
}
