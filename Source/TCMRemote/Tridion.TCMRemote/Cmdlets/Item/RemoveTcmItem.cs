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

namespace Tridion.TCMRemote.Cmdlets.Item
{
    /// <summary>
    /// <para type="synopsis">Deletes a Tridion Sites content item by its TCM URI.</para>
    /// <para type="description">Remove-TcmItem permanently deletes the specified content item from the Tridion Sites Content Manager. This operation is irreversible. Use -WhatIf to preview the deletion.</para>
    /// </summary>
    /// <example>
    /// <code>Remove-TcmItem -ItemId "tcm:1-2-16" -WhatIf</code>
    /// <para>Previews the deletion without actually removing the item.</para>
    /// </example>
    /// <example>
    /// <code>Remove-TcmItem -ItemId "tcm:1-2-16"</code>
    /// <para>Permanently deletes the item.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "TcmItem", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveTcmItem : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public TcmSession? TcmSession { get; set; }

        /// <summary>
        /// <para type="description">TCM URI of the item to delete, e.g. "tcm:1-2-16".</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ItemId { get; set; } = string.Empty;

        protected override void ProcessRecord()
        {
            try
            {
                if (!ShouldProcess(ItemId, "Delete Tridion item"))
                    return;

                var session = ResolveSession(TcmSession);
                if (session == null) return;

                WriteVerbose($"Deleting item [{ItemId}]");
                var success = session.Client.DeleteItemAsync(ItemId).GetAwaiter().GetResult();
                if (!success)
                    WriteWarning($"Server returned a non-success response for item [{ItemId}]. The item may have already been deleted.");
                else
                    WriteVerbose($"Item [{ItemId}] deleted.");
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "RemoveTcmItemFailed", ErrorCategory.WriteError, ItemId));
            }
        }

        private TcmSession? ResolveSession(TcmSession? explicit_)
        {
            if (explicit_ != null) return explicit_;
            var variable = SessionState.PSVariable.Get(TCMRemoteSessionStateTcmSession);
            var session  = variable?.Value as TcmSession;
            if (session == null)
                WriteError(new ErrorRecord(
                    new InvalidOperationException(TCMRemoteSessionStateTcmSessionException),
                    "NoTcmSession", ErrorCategory.InvalidOperation, null));
            return session;
        }
    }
}
