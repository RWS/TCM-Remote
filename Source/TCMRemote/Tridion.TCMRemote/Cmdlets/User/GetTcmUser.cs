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
using Tridion.TCMRemote.OpenApiTCM30;

namespace Tridion.TCMRemote.Cmdlets.User
{
    /// <summary>
    /// <para type="synopsis">Returns Tridion Sites users.</para>
    /// <para type="description">Get-TcmUser queries the Tridion Sites Content Manager REST API and returns user objects. Use the filter parameters to narrow the result set.</para>
    /// </summary>
    /// <example>
    /// <code>Get-TcmUser</code>
    /// <para>Returns all active (non-disabled) non-predefined users.</para>
    /// </example>
    /// <example>
    /// <code>Get-TcmUser -Search "john" -IncludeDisabled</code>
    /// <para>Returns all users whose name contains "john", including disabled accounts.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "TcmUser")]
    [OutputType(typeof(Tridion.TCMRemote.OpenApiTCM30.User))]
    public sealed class GetTcmUser : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public TcmSession? TcmSession { get; set; }

        /// <summary>
        /// <para type="description">Filter by a search string matched against the user name.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 0)]
        public string? Search { get; set; }

        /// <summary>
        /// <para type="description">When set, includes predefined system users in the results.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter IncludePredefined { get; set; }

        /// <summary>
        /// <para type="description">When set, includes disabled user accounts in the results.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter IncludeDisabled { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var session = ResolveSession(TcmSession);
                if (session == null) return;

                WriteVerbose("Retrieving users from Tridion Sites");
                var users = session.Client.GetUsersAsync(
                    predefined:      IncludePredefined.IsPresent ? (bool?)true : null,
                    includeDisabled: IncludeDisabled.IsPresent   ? (bool?)true : null,
                    search:          Search
                ).GetAwaiter().GetResult();

                foreach (var user in users)
                    WriteObject(user);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "GetTcmUserFailed", ErrorCategory.ReadError, null));
            }
        }

        private TcmSession? ResolveSession(TcmSession? explicit_)
        {
            if (explicit_ != null) return explicit_;
            var variable = SessionState.PSVariable.Get(TCMRemoteSessionStateTcmSession);
            var session  = variable?.Value as TcmSession;
            if (session == null)
                WriteError(new ErrorRecord(
                    new InvalidOperationException(TCMRemoteSessionStateTcmSessionException),
                    "NoTcmSession", ErrorCategory.InvalidOperation, null));
            return session;
        }
    }
}
