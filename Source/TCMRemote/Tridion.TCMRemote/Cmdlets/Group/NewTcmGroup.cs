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
    /// <para type="synopsis">Creates a new group in the Tridion Sites Content Manager.</para>
    /// <para type="description">New-TcmGroup creates a new Group object via the REST API. The group is created at the system level.</para>
    /// </summary>
    /// <example>
    /// <code>New-TcmGroup -Title "Web Editors" -Description "Editors for web publications"</code>
    /// <para>Creates a new group named "Web Editors".</para>
    /// </example>
    [Cmdlet(VerbsCommon.New, "TcmGroup", SupportsShouldProcess = true)]
    [OutputType(typeof(OpenApiTCM30.Group))]
    public sealed class NewTcmGroup : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public TcmSession? TcmSession { get; set; }

        /// <summary>
        /// <para type="description">The display name for the new group.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// <para type="description">Optional description for the new group.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string? Description { get; set; }

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
            if (!ShouldProcess(Title, "Create Group"))
                return;

            try
            {
                WriteVerbose($"Creating group [{Title}]");
                var group = new OpenApiTCM30.Group
                {
                    Title = Title,
                    Description = Description
                };

                var result = _resolvedSession!.Client.CreateItemAsync(group).GetAwaiter().GetResult();
                WriteObject(result);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to create group: {ex.Message}", ex),
                    "NewTcmGroupFailed",
                    ErrorCategory.WriteError,
                    Title));
            }
        }
    }
}
