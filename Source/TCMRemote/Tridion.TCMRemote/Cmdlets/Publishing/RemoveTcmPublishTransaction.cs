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

namespace Tridion.TCMRemote.Cmdlets.Publishing
{
    /// <summary>
    /// <para type="synopsis">Deletes (removes) a publish transaction from the Tridion Sites Content Manager.</para>
    /// <para type="description">Remove-TcmPublishTransaction deletes the specified publish transaction. Only transactions that are in a terminal state (Success, Failed, Aborted) can be deleted.</para>
    /// </summary>
    /// <example>
    /// <code>Remove-TcmPublishTransaction -ItemId "tcm:0-123-66560"</code>
    /// <para>Removes the publish transaction tcm:0-123-66560.</para>
    /// </example>
    /// <example>
    /// <code>Get-TcmPublishTransaction -State Failed | Remove-TcmPublishTransaction</code>
    /// <para>Removes all failed publish transactions.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "TcmPublishTransaction", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public sealed class RemoveTcmPublishTransaction : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public TcmSession? TcmSession { get; set; }

        /// <summary>
        /// <para type="description">TCM URI of the publish transaction to remove, e.g. "tcm:0-123-66560".</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ItemId { get; set; } = string.Empty;

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
            if (!ShouldProcess(ItemId, "Remove Publish Transaction"))
                return;

            try
            {
                WriteVerbose($"Removing publish transaction [{ItemId}]");
                _resolvedSession!.Client.DeleteItemAsync(ItemId).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to remove publish transaction: {ex.Message}", ex),
                    "RemoveTcmPublishTransactionFailed",
                    ErrorCategory.WriteError,
                    ItemId));
            }
        }
    }
}
