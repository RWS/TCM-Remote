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
    /// <para type="synopsis">Updates an existing item in the Tridion Sites Content Manager.</para>
    /// <para type="description">Set-TcmItem replaces the server-side representation of an item with the provided object. The item must already exist (identified by its Id). Use Get-TcmItem first to retrieve the current state, modify properties, then pass it to Set-TcmItem.</para>
    /// </summary>
    /// <example>
    /// <code>$item = Get-TcmItem -ItemId "tcm:1-5"; $item.Title = "Updated Title"; Set-TcmItem -ItemId "tcm:1-5" -Item $item</code>
    /// <para>Retrieves an item, changes its title, and saves the update.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "TcmItem", SupportsShouldProcess = true)]
    [OutputType(typeof(IdentifiableObject))]
    public sealed class SetTcmItem : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public TcmSession? TcmSession { get; set; }

        /// <summary>
        /// <para type="description">TCM URI of the item to update, e.g. "tcm:1-5".</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// <para type="description">The updated item object to save.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ValueFromPipeline = true)]
        [ValidateNotNull]
        public IdentifiableObject Item { get; set; } = null!;

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
            if (!ShouldProcess(ItemId, "Set Item"))
                return;

            try
            {
                WriteVerbose($"Updating item [{ItemId}]");
                var result = _resolvedSession!.Client.UpdateItemAsync(ItemId, Item).GetAwaiter().GetResult();
                WriteObject(result);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to update item: {ex.Message}", ex),
                    "SetTcmItemFailed",
                    ErrorCategory.WriteError,
                    ItemId));
            }
        }
    }
}
