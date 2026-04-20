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
    /// <para type="synopsis">Creates a new user in the Tridion Sites Content Manager.</para>
    /// <para type="description">New-TcmUser creates a new User object via the REST API.</para>
    /// </summary>
    /// <example>
    /// <code>New-TcmUser -Title "jsmith" -Description "John Smith"</code>
    /// <para>Creates a new user account with the login name "jsmith".</para>
    /// </example>
    [Cmdlet(VerbsCommon.New, "TcmUser", SupportsShouldProcess = true)]
    [OutputType(typeof(OpenApiTCM30.User))]
    public sealed class NewTcmUser : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public TcmSession? TcmSession { get; set; }

        /// <summary>
        /// <para type="description">The login name (Title) for the new user.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// <para type="description">Optional description / display name for the new user.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string? Description { get; set; }

        /// <summary>
        /// <para type="description">Whether the user account is enabled at creation time. Defaults to true.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public bool IsEnabled { get; set; } = true;

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
            if (!ShouldProcess(Title, "Create User"))
                return;

            try
            {
                WriteVerbose($"Creating user [{Title}]");
                var user = new OpenApiTCM30.User
                {
                    Title = Title,
                    Description = Description,
                    IsEnabled = IsEnabled
                };

                var result = _resolvedSession!.Client.CreateItemAsync(user).GetAwaiter().GetResult();
                WriteObject(result);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to create user: {ex.Message}", ex),
                    "NewTcmUserFailed",
                    ErrorCategory.WriteError,
                    Title));
            }
        }
    }
}
