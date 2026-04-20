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

namespace Tridion.TCMRemote.Cmdlets.Publication
{
    /// <summary>
    /// <para type="synopsis">Retrieves a Tridion Sites publication by TCM URI.</para>
    /// <para type="description">Get-TcmPublication returns the Publication object for the given item identifier. Use Get-TcmPublications (plural) to list all publications.</para>
    /// </summary>
    /// <example>
    /// <code>Get-TcmPublication -ItemId "tcm:0-1-1"</code>
    /// <para>Returns the publication with TCM URI tcm:0-1-1.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "TcmPublication")]
    [OutputType(typeof(Tridion.TCMRemote.OpenApiTCM30.Publication))]
    public sealed class GetTcmPublication : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public TcmSession? TcmSession { get; set; }

        /// <summary>
        /// <para type="description">TCM URI of the publication, e.g. "tcm:0-1-1".</para>
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
                WriteVerbose($"Retrieving publication [{ItemId}]");
                var publication = _resolvedSession!.Client.GetPublicationAsync(ItemId).GetAwaiter().GetResult();
                WriteObject(publication);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to get publication: {ex.Message}", ex),
                    "GetTcmPublicationFailed",
                    ErrorCategory.ReadError,
                    ItemId));
            }
        }
    }
}
