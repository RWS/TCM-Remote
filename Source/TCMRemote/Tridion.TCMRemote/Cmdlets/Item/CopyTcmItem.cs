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
    /// <para type="synopsis">Copies an item to a different container in the Tridion Sites Content Manager.</para>
    /// <para type="description">Copy-TcmItem creates a copy of a repository item in the specified destination folder or structure group.</para>
    /// </summary>
    /// <example>
    /// <code>Copy-TcmItem -ItemId "tcm:1-5" -DestinationId "tcm:1-10-2"</code>
    /// <para>Copies the item tcm:1-5 to the folder tcm:1-10-2.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Copy, "TcmItem", SupportsShouldProcess = true)]
    [OutputType(typeof(RepositoryLocalObject))]
    public sealed class CopyTcmItem : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public TcmSession? TcmSession { get; set; }

        /// <summary>
        /// <para type="description">TCM URI of the item to copy.</para>
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

        /// <summary>
        /// <para type="description">When specified, the copy is given a unique name to avoid conflicts. Enabled by default.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter NotUnique { get; set; }

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
            if (!ShouldProcess(ItemId, $"Copy to {DestinationId}"))
                return;

            try
            {
                WriteVerbose($"Copying item [{ItemId}] to [{DestinationId}]");
                var result = _resolvedSession!.Client.CopyItemAsync(ItemId, DestinationId, makeUnique: !NotUnique.IsPresent).GetAwaiter().GetResult();
                WriteObject(result);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to copy item: {ex.Message}", ex),
                    "CopyTcmItemFailed",
                    ErrorCategory.WriteError,
                    ItemId));
            }
        }
    }
}
