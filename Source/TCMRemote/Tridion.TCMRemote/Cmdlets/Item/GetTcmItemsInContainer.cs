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

namespace Tridion.TCMRemote.Cmdlets.Item
{
    /// <summary>
    /// <para type="synopsis">Returns the items in a Folder or StructureGroup container.</para>
    /// <para type="description">Get-TcmItemsInContainer retrieves the direct or recursive list of repository items within a Folder or StructureGroup. Results are written to the pipeline as RepositoryLocalObject instances.</para>
    /// </summary>
    /// <example>
    /// <code>Get-TcmItemsInContainer -ContainerId "tcm:1-1-2"</code>
    /// <para>Returns the direct children of the root folder in publication 1.</para>
    /// </example>
    /// <example>
    /// <code>Get-TcmItemsInContainer -ContainerId "tcm:1-1-2" -Recursive</code>
    /// <para>Returns all items recursively under the root folder.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "TcmItemsInContainer")]
    [OutputType(typeof(RepositoryLocalObject))]
    public sealed class GetTcmItemsInContainer : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public TcmSession? TcmSession { get; set; }

        /// <summary>
        /// <para type="description">TCM URI of the container (Folder or StructureGroup).</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ContainerId { get; set; } = string.Empty;

        /// <summary>
        /// <para type="description">When specified, returns all items recursively through all sub-folders.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Recursive { get; set; }

        /// <summary>
        /// <para type="description">When specified, uses the dynamic version to resolve items.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter UseDynamicVersion { get; set; }

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
                WriteVerbose($"Retrieving items in container [{ContainerId}]");
                var items = _resolvedSession!.Client.GetItemsInContainerAsync(
                    ContainerId,
                    useDynamicVersion: UseDynamicVersion.IsPresent ? (bool?)true : null,
                    recursive: Recursive.IsPresent ? (bool?)true : null
                ).GetAwaiter().GetResult();

                foreach (var item in items)
                    WriteObject(item);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to get items in container: {ex.Message}", ex),
                    "GetTcmItemsInContainerFailed",
                    ErrorCategory.ReadError,
                    ContainerId));
            }
        }
    }
}
