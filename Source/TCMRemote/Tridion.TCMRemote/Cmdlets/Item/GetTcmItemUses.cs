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
    /// <para type="synopsis">Returns the items that the specified item uses (outgoing links).</para>
    /// <para type="description">Get-TcmItemUses returns the list of repository items that are referenced by the specified item — i.e. items that this item depends on.</para>
    /// </summary>
    /// <example>
    /// <code>Get-TcmItemUses -ItemId "tcm:1-10-64"</code>
    /// <para>Returns all items used by the page tcm:1-10-64.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "TcmItemUses")]
    [OutputType(typeof(IdentifiableObject))]
    public sealed class GetTcmItemUses : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public TcmSession? TcmSession { get; set; }

        /// <summary>
        /// <para type="description">TCM URI of the item whose usages to retrieve.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// <para type="description">When specified, includes the blueprint parent item in the results.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter IncludeBlueprintParent { get; set; }

        /// <summary>
        /// <para type="description">When specified, uses the dynamic version to resolve the item.</para>
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
                WriteVerbose($"Retrieving uses for item [{ItemId}]");
                var uses = _resolvedSession!.Client.GetItemUsesAsync(
                    ItemId,
                    includeBlueprintParent: IncludeBlueprintParent.IsPresent ? (bool?)true : null,
                    useDynamicVersion: UseDynamicVersion.IsPresent ? (bool?)true : null
                ).GetAwaiter().GetResult();

                foreach (var item in uses)
                    WriteObject(item);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to get item uses: {ex.Message}", ex),
                    "GetTcmItemUsesFailed",
                    ErrorCategory.ReadError,
                    ItemId));
            }
        }
    }
}
