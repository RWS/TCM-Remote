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
    /// <para type="synopsis">Undoes a checkout of a versioned item in the Tridion Sites Content Manager.</para>
    /// <para type="description">Undo-TcmCheckOut releases the checkout lock on an item without saving a new version. Any unsaved changes made since checkout are lost.</para>
    /// </summary>
    /// <example>
    /// <code>Undo-TcmCheckOut -ItemId "tcm:1-5-16"</code>
    /// <para>Releases the checkout lock on tcm:1-5-16 without saving changes.</para>
    /// </example>
    [Cmdlet("Undo", "TcmCheckOut", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(VersionedItem))]
    public sealed class UndoTcmCheckOut : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public TcmSession? TcmSession { get; set; }

        /// <summary>
        /// <para type="description">TCM URI of the checked-out item, e.g. "tcm:1-5-16".</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// <para type="description">When specified, removes the permanent checkout lock during the undo operation.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter RemovePermanentLock { get; set; }

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
            if (!ShouldProcess(ItemId, "Undo CheckOut"))
                return;

            try
            {
                WriteVerbose($"Undoing checkout of item [{ItemId}]");
                var result = _resolvedSession!.Client.UndoCheckOutAsync(
                    ItemId,
                    removePermanentLock: RemovePermanentLock.IsPresent ? (bool?)true : null
                ).GetAwaiter().GetResult();
                WriteObject(result);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to undo checkout: {ex.Message}", ex),
                    "UndoTcmCheckOutFailed",
                    ErrorCategory.WriteError,
                    ItemId));
            }
        }
    }
}
