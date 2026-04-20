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

namespace Tridion.TCMRemote.Cmdlets.Group
{
    /// <summary>
    /// <para type="synopsis">Returns groups from the Tridion Sites Content Manager.</para>
    /// <para type="description">Get-TcmGroup queries the REST API and returns Group objects. Use -Search to filter by name. Use -InPublicationId to scope to a publication. Use -IncludeEveryone to include the built-in Everyone group.</para>
    /// </summary>
    /// <example>
    /// <code>Get-TcmGroup</code>
    /// <para>Returns all groups.</para>
    /// </example>
    /// <example>
    /// <code>Get-TcmGroup -Search "Editors"</code>
    /// <para>Returns groups whose name contains "Editors".</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "TcmGroup")]
    [OutputType(typeof(OpenApiTCM30.Group))]
    public sealed class GetTcmGroup : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public TcmSession? TcmSession { get; set; }

        /// <summary>
        /// <para type="description">Scope results to groups that belong to this publication TCM URI.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string? InPublicationId { get; set; }

        /// <summary>
        /// <para type="description">When specified, includes the built-in Everyone group in the results.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter IncludeEveryone { get; set; }

        /// <summary>
        /// <para type="description">Filter groups by name (partial match).</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string? Search { get; set; }

        private TcmSession? _resolvedSession;

        protected override void BeginProcessing()
        {
            _resolvedSession = TcmSession
                ?? SessionState.PSVariable.Get(TCMRemoteSessionStateTcmSession)?.Value as TcmSession;

            if (_resolvedSession == null)
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException(TCMRemoteSessionStateTcmSessionException),
                    "NoTcmSession", ErrorCategory.InvalidOperation, null));

            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            try
            {
                WriteVerbose("Retrieving groups");
                var groups = _resolvedSession!.Client.GetGroupsAsync(
                    inPublicationId: InPublicationId,
                    includeEveryone: IncludeEveryone.IsPresent ? (bool?)true : null,
                    search: Search
                ).GetAwaiter().GetResult();

                foreach (var group in groups)
                    WriteObject(group);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to get groups: {ex.Message}", ex),
                    "GetTcmGroupFailed",
                    ErrorCategory.ReadError,
                    null));
            }
        }
    }
}
