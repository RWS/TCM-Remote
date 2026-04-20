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
    /// <para type="synopsis">Returns the version history of a versioned item in the Tridion Sites Content Manager.</para>
    /// <para type="description">Get-TcmItemHistory retrieves the complete list of saved versions for a component, page, or other versioned repository item. Each version is written to the pipeline as a VersionedItem.</para>
    /// </summary>
    /// <example>
    /// <code>Get-TcmItemHistory -ItemId "tcm:1-5-16"</code>
    /// <para>Returns all versions of the component tcm:1-5-16.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "TcmItemHistory")]
    [OutputType(typeof(VersionedItem))]
    public sealed class GetTcmItemHistory : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public TcmSession? TcmSession { get; set; }

        /// <summary>
        /// <para type="description">TCM URI of the versioned item, e.g. "tcm:1-5-16".</para>
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
            try
            {
                WriteVerbose($"Retrieving version history for [{ItemId}]");
                var versions = _resolvedSession!.Client.GetItemHistoryAsync(ItemId).GetAwaiter().GetResult();
                foreach (var version in versions)
                    WriteObject(version);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to get item history: {ex.Message}", ex),
                    "GetTcmItemHistoryFailed",
                    ErrorCategory.ReadError,
                    ItemId));
            }
        }
    }
}
