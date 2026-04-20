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
    /// <para type="synopsis">Searches for items in the Tridion Sites Content Manager using full-text search.</para>
    /// <para type="description">Find-TcmItem executes a full-text search across all repository items. The search results are written to the pipeline as IdentifiableObject instances.</para>
    /// </summary>
    /// <example>
    /// <code>Find-TcmItem -Query "product launch"</code>
    /// <para>Returns all items matching the text "product launch".</para>
    /// </example>
    /// <example>
    /// <code>Find-TcmItem -Query "press release" -ResultLimit 20</code>
    /// <para>Returns up to 20 items matching "press release".</para>
    /// </example>
    [Cmdlet(VerbsCommon.Find, "TcmItem")]
    [OutputType(typeof(IdentifiableObject))]
    public sealed class FindTcmItem : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public TcmSession? TcmSession { get; set; }

        /// <summary>
        /// <para type="description">Full-text search query string.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public string Query { get; set; } = string.Empty;

        /// <summary>
        /// <para type="description">Maximum number of results to return. When omitted the server default applies.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public int? ResultLimit { get; set; }

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
                WriteVerbose($"Searching for [{Query}]");
                var results = _resolvedSession!.Client.FindItemsAsync(Query, ResultLimit).GetAwaiter().GetResult();
                foreach (var item in results)
                    WriteObject(item);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to search: {ex.Message}", ex),
                    "FindTcmItemFailed",
                    ErrorCategory.ReadError,
                    Query));
            }
        }
    }
}
