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

namespace Tridion.TCMRemote.Cmdlets.User
{
    /// <summary>
    /// <para type="synopsis">Disables a user account in the Tridion Sites Content Manager.</para>
    /// <para type="description">Disable-TcmUser sets IsEnabled=false on the specified user, preventing them from logging in.</para>
    /// </summary>
    /// <example>
    /// <code>Disable-TcmUser -ItemId "tcm:0-12-65536"</code>
    /// <para>Disables the user with TCM URI tcm:0-12-65536.</para>
    /// </example>
    [Cmdlet("Disable", "TcmUser", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(OpenApiTCM30.User))]
    public sealed class DisableTcmUser : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public TcmSession? TcmSession { get; set; }

        /// <summary>
        /// <para type="description">TCM URI of the user to disable, e.g. "tcm:0-12-65536".</para>
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
            if (!ShouldProcess(ItemId, "Disable User"))
                return;

            try
            {
                WriteVerbose($"Disabling user [{ItemId}]");
                var current = _resolvedSession!.Client.GetItemAsync(ItemId).GetAwaiter().GetResult() as OpenApiTCM30.User
                              ?? new OpenApiTCM30.User { Id = ItemId };
                current.IsEnabled = false;
                var result = _resolvedSession.Client.UpdateUserAsync(ItemId, current).GetAwaiter().GetResult();
                WriteObject(result);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to disable user: {ex.Message}", ex),
                    "DisableTcmUserFailed",
                    ErrorCategory.WriteError,
                    ItemId));
            }
        }
    }
}
