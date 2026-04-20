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
    /// <para type="synopsis">Returns all publications from the Tridion Sites Content Manager.</para>
    /// <para type="description">Get-TcmPublications queries the REST API and returns the full list of Publication objects. Each publication is written to the pipeline so the results can be filtered with Where-Object.</para>
    /// </summary>
    /// <example>
    /// <code>Get-TcmPublications</code>
    /// <para>Returns all publications in the Content Manager.</para>
    /// </example>
    /// <example>
    /// <code>Get-TcmPublications | Where-Object { $_.Title -like "*Web*" }</code>
    /// <para>Returns publications whose title contains "Web".</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "TcmPublications")]
    [OutputType(typeof(Tridion.TCMRemote.OpenApiTCM30.Publication))]
    public sealed class GetTcmPublications : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public TcmSession? TcmSession { get; set; }

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
                WriteVerbose("Retrieving all publications");
                var publications = _resolvedSession!.Client.GetPublicationsAsync().GetAwaiter().GetResult();
                foreach (var pub in publications)
                    WriteObject(pub);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to get publications: {ex.Message}", ex),
                    "GetTcmPublicationsFailed",
                    ErrorCategory.ReadError,
                    null));
            }
        }
    }
}
