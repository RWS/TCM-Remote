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
    /// <para type="synopsis">Creates a new publication in the Tridion Sites Content Manager.</para>
    /// <para type="description">New-TcmPublication creates a new Publication object via the REST API.</para>
    /// </summary>
    /// <example>
    /// <code>New-TcmPublication -Title "My Web Publication"</code>
    /// <para>Creates a new publication named "My Web Publication".</para>
    /// </example>
    [Cmdlet(VerbsCommon.New, "TcmPublication", SupportsShouldProcess = true)]
    [OutputType(typeof(Tridion.TCMRemote.OpenApiTCM30.Publication))]
    public sealed class NewTcmPublication : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public TcmSession? TcmSession { get; set; }

        /// <summary>
        /// <para type="description">The title for the new publication.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// <para type="description">Optional key (path segment) for the new publication.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string? Key { get; set; }

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
            if (!ShouldProcess(Title, "Create Publication"))
                return;

            try
            {
                WriteVerbose($"Creating publication [{Title}]");
                var publication = new Tridion.TCMRemote.OpenApiTCM30.Publication
                {
                    Title = Title,
                    Key = Key
                };

                var result = _resolvedSession!.Client.CreateItemAsync(publication).GetAwaiter().GetResult();
                WriteObject(result);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to create publication: {ex.Message}", ex),
                    "NewTcmPublicationFailed",
                    ErrorCategory.WriteError,
                    Title));
            }
        }
    }
}
