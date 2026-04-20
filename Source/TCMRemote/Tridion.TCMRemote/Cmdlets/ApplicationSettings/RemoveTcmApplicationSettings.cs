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

namespace Tridion.TCMRemote.Cmdlets.ApplicationSettings
{
    /// <summary>
    /// <para type="synopsis">Removes the application settings for the specified application from the Tridion Sites Content Manager.</para>
    /// <para type="description">Remove-TcmApplicationSettings deletes all settings stored for a named application identifier.</para>
    /// </summary>
    /// <example>
    /// <code>Remove-TcmApplicationSettings -ApplicationId "MyApp"</code>
    /// <para>Removes all settings for the application "MyApp".</para>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "TcmApplicationSettings", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public sealed class RemoveTcmApplicationSettings : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public TcmSession? TcmSession { get; set; }

        /// <summary>
        /// <para type="description">The application identifier whose settings to remove.</para>
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
            if (!ShouldProcess(ApplicationId, "Remove Application Settings"))
                return;

            try
            {
                WriteVerbose($"Removing application settings for [{ApplicationId}]");
                _resolvedSession!.Client.DeleteApplicationSettingsAsync(ApplicationId).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to remove application settings: {ex.Message}", ex),
                    "RemoveTcmApplicationSettingsFailed",
                    ErrorCategory.WriteError,
                    ApplicationId));
            }
        }
    }
}
