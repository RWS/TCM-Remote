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

namespace Tridion.TCMRemote.Cmdlets.PublicationTarget
{
    /// <summary>
    /// <para type="synopsis">Returns all publication targets (target types) from the Tridion Sites Content Manager.</para>
    /// <para type="description">Get-TcmPublicationTarget returns the list of TargetType objects configured in the Content Manager. These are the publishing destinations available when calling Publish-TcmItem.</para>
    /// </summary>
    /// <example>
    /// <code>Get-TcmPublicationTarget</code>
    /// <para>Returns all publication targets.</para>
    /// </example>
    /// <example>
    /// <code>Get-TcmPublicationTarget | Where-Object { $_.Title -like "*Live*" }</code>
    /// <para>Returns publication targets whose title contains "Live".</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "TcmPublicationTarget")]
    [OutputType(typeof(TargetType))]
    public sealed class GetTcmPublicationTarget : TridionCmdlet
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
                WriteVerbose("Retrieving publication targets");
                var targets = _resolvedSession!.Client.GetTargetTypesAsync().GetAwaiter().GetResult();
                foreach (var target in targets)
                    WriteObject(target);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to get publication targets: {ex.Message}", ex),
                    "GetTcmPublicationTargetFailed",
                    ErrorCategory.ReadError,
                    null));
            }
        }
    }
}
