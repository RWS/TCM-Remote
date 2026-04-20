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
    /// <para type="synopsis">Moves an item to a different container in the Tridion Sites Content Manager.</para>
    /// <para type="description">Move-TcmItem moves a repository item to a different folder or structure group within the same publication.</para>
    /// </summary>
    /// <example>
    /// <code>Move-TcmItem -ItemId "tcm:1-5" -DestinationId "tcm:1-10-2"</code>
    /// <para>Moves the item tcm:1-5 into the folder tcm:1-10-2.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Move, "TcmItem", SupportsShouldProcess = true)]
    [OutputType(typeof(RepositoryLocalObject))]
    public sealed class MoveTcmItem : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public TcmSession? TcmSession { get; set; }

        /// <summary>
        /// <para type="description">TCM URI of the item to move.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// <para type="description">TCM URI of the destination container (Folder or StructureGroup).</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        [ValidateNotNullOrEmpty]
        public string DestinationId { get; set; } = string.Empty;

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
            if (!ShouldProcess(ItemId, $"Move to {DestinationId}"))
                return;

            try
            {
                WriteVerbose($"Moving item [{ItemId}] to [{DestinationId}]");
                var result = _resolvedSession!.Client.MoveItemAsync(ItemId, DestinationId).GetAwaiter().GetResult();
                WriteObject(result);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to move item: {ex.Message}", ex),
                    "MoveTcmItemFailed",
                    ErrorCategory.WriteError,
                    ItemId));
            }
        }
    }
}
