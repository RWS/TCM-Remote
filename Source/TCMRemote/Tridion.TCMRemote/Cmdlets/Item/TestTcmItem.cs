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
    /// <para type="synopsis">Tests whether an item exists in the Tridion Sites Content Manager.</para>
    /// <para type="description">Test-TcmItem pings the REST API to check if an item with the given TCM URI exists and is accessible. Returns $true or $false.</para>
    /// </summary>
    /// <example>
    /// <code>Test-TcmItem -ItemId "tcm:1-5"</code>
    /// <para>Returns $true if the item exists, $false otherwise.</para>
    /// </example>
    [Cmdlet(VerbsDiagnostic.Test, "TcmItem")]
    [OutputType(typeof(bool))]
    public sealed class TestTcmItem : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public TcmSession? TcmSession { get; set; }

        /// <summary>
        /// <para type="description">TCM URI of the item to test, e.g. "tcm:1-5-16".</para>
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
                WriteVerbose($"Testing existence of item [{ItemId}]");
                var exists = _resolvedSession!.Client.ItemExistsAsync(ItemId).GetAwaiter().GetResult();
                WriteObject(exists);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to test item: {ex.Message}", ex),
                    "TestTcmItemFailed",
                    ErrorCategory.ReadError,
                    ItemId));
            }
        }
    }
}
