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
using System.Collections.Generic;
using System.Management.Automation;
using Tridion.TCMRemote.Objects.Public;
using Tridion.TCMRemote.OpenApiTCM30;

namespace Tridion.TCMRemote.Cmdlets.Workflow
{
    /// <summary>
    /// <para type="synopsis">Returns workflow activity instances from the Tridion Sites Content Manager.</para>
    /// <para type="description">Get-TcmActivityInstance retrieves the list of current workflow activity instances. Use -ForAllUsers to see all users' activities, -ActivityState to filter by state, or -ProcessDefinitionId to scope to a specific workflow type.</para>
    /// </summary>
    /// <example>
    /// <code>Get-TcmActivityInstance</code>
    /// <para>Returns the current user's workflow activity instances.</para>
    /// </example>
    /// <example>
    /// <code>Get-TcmActivityInstance -ForAllUsers -ActivityState Active</code>
    /// <para>Returns all active workflow activities across all users.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "TcmActivityInstance")]
    [OutputType(typeof(ActivityInstance))]
    public sealed class GetTcmActivityInstance : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public TcmSession? TcmSession { get; set; }

        /// <summary>
        /// <para type="description">When specified, returns activity instances for all users. Requires administrator permissions.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter ForAllUsers { get; set; }

        /// <summary>
        /// <para type="description">Filter by activity owner user TCM URI.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string? OwnerId { get; set; }

        /// <summary>
        /// <para type="description">Filter by activity assignee user TCM URI.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string? AssigneeId { get; set; }

        /// <summary>
        /// <para type="description">Filter by one or more activity states (e.g. Active, Waiting, Finished).</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public ActivityState[]? ActivityState { get; set; }

        /// <summary>
        /// <para type="description">Filter by process definition (workflow type) TCM URI.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string? ProcessDefinitionId { get; set; }

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
                WriteVerbose("Retrieving workflow activity instances");
                IEnumerable<ActivityState>? states = ActivityState;
                var instances = _resolvedSession!.Client.GetActivityInstancesAsync(
                    forAllUsers: ForAllUsers.IsPresent ? (bool?)true : null,
                    ownerId: OwnerId,
                    assigneeId: AssigneeId,
                    activityStates: states,
                    processDefinitionId: ProcessDefinitionId
                ).GetAwaiter().GetResult();

                foreach (var instance in instances)
                    WriteObject(instance);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to get activity instances: {ex.Message}", ex),
                    "GetTcmActivityInstanceFailed",
                    ErrorCategory.ReadError,
                    null));
            }
        }
    }
}
