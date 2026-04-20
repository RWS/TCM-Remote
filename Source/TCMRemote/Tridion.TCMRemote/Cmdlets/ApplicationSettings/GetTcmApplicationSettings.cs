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

namespace Tridion.TCMRemote.Cmdlets.ApplicationSettings
{
    /// <summary>
    /// <para type="synopsis">Retrieves application settings for the specified application from the Tridion Sites Content Manager.</para>
    /// <para type="description">Get-TcmApplicationSettings returns the Settings object for a named application. Application settings are key-value pairs stored per application identifier in the Content Manager.</para>
    /// </summary>
    /// <example>
    /// <code>Get-TcmApplicationSettings -ApplicationId "MyApp"</code>
    /// <para>Returns the settings for the application "MyApp".</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "TcmApplicationSettings")]
    [OutputType(typeof(Settings))]
    public sealed class GetTcmApplicationSettings : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public TcmSession? TcmSession { get; set; }

        /// <summary>
        /// <para type="description">The application identifier whose settings to retrieve.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ApplicationId { get; set; } = string.Empty;

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
                WriteVerbose($"Retrieving application settings for [{ApplicationId}]");
                var settings = _resolvedSession!.Client.GetApplicationSettingsAsync(ApplicationId).GetAwaiter().GetResult();
                WriteObject(settings);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to get application settings: {ex.Message}", ex),
                    "GetTcmApplicationSettingsFailed",
                    ErrorCategory.ReadError,
                    ApplicationId));
            }
        }
    }
}
