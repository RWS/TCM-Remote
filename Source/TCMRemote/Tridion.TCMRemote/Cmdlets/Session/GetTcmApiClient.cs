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

namespace Tridion.TCMRemote.Cmdlets.Session
{
    /// <summary>
    /// <para type="synopsis">Returns the authenticated low-level REST API client for the Tridion Sites Content Manager.</para>
    /// <para type="description">
    /// Get-TcmApiClient returns the underlying <see cref="OpenApiTCM30Client"/> — the auto-generated typed client
    /// for every Tridion Sites REST API endpoint.  This is the REST equivalent of the legacy
    /// <c>Get-TridionCoreServiceClient</c> cmdlet, which returned the WCF proxy object.
    /// </para>
    /// <para type="description">
    /// Use this when you need to call a REST endpoint that is not yet wrapped by a dedicated TCMRemote cmdlet.
    /// The client is fully authenticated (Bearer token) and ready to use.
    /// Call async methods with <c>.GetAwaiter().GetResult()</c> or <c>.Result</c> from within PowerShell.
    /// </para>
    /// </summary>
    /// <para type="description">
    /// IMPORTANT — PowerShell method binding rules for the raw client:
    /// </para>
    /// <para type="description">
    /// 1. TCM URI escaping: replace ":" with "_" before passing to the API  (tcm:1-2-64 → tcm_1-2-64).
    /// </para>
    /// <para type="description">
    /// 2. C# optional/nullable parameters are NOT optional in PowerShell — pass $null for each one.
    /// </para>
    /// <para type="description">
    /// 3. Use the overload WITHOUT CancellationToken to keep calls simple.
    /// </para>
    /// <example>
    /// <code>
    /// $api  = Get-TcmApiClient
    /// $user = $api.GetOwnUserProfileAsync().GetAwaiter().GetResult()
    /// $user.Title
    /// </code>
    /// <para>Retrieve the current user profile. GetOwnUserProfileAsync() takes no parameters.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $api  = Get-TcmApiClient
    /// # GetItemAsync(string escapedId, bool? useDynamicVersion)
    /// $item = $api.GetItemAsync("tcm_1-2-64", $null).GetAwaiter().GetResult()
    /// $item | Select-Object Title, Id
    /// </code>
    /// <para>Get any item by TCM URI. Pass $null for each optional (nullable) parameter.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $api  = Get-TcmApiClient
    /// # GetPublicationsAsync(ListDetails? details)
    /// $pubs = $api.GetPublicationsAsync($null).GetAwaiter().GetResult()
    /// $pubs | Select-Object Title, Id
    /// </code>
    /// <para>List all publications.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $api   = Get-TcmApiClient
    /// # GetItemsFromContainerAsync(string escapedId, bool? dynamicVer, IEnumerable rloTypes, bool? recursive, ListDetails? details)
    /// $items = $api.GetItemsFromContainerAsync("tcm_2-5-4", $null, $null, $true, $null).GetAwaiter().GetResult()
    /// $items | Select-Object Title, Id
    /// </code>
    /// <para>List all items in a container recursively.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $api | Get-Member -MemberType Method | Select-Object Name | Sort-Object Name
    /// </code>
    /// <para>Discover all available REST API methods on the client object.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "TcmApiClient")]
    [OutputType(typeof(OpenApiTCM30Client))]
    public sealed class GetTcmApiClient : SessionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public TcmSession? TcmSession { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var session = TcmSession ?? GetSessionFromState();
                if (session == null)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException(TCMRemoteSessionStateTcmSessionException),
                        "NoActiveSession",
                        ErrorCategory.InvalidOperation,
                        null));
                    return;
                }

                var apiClient = session.Client.GetApiClientAsync().GetAwaiter().GetResult();
                WriteObject(apiClient);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to get API client: {ex.Message}", ex),
                    "GetTcmApiClientFailed",
                    ErrorCategory.ConnectionError,
                    null));
            }
        }

        private TcmSession? GetSessionFromState()
        {
            var variable = SessionState.PSVariable.Get(TCMRemoteSessionStateTcmSession);
            return variable?.Value as TcmSession;
        }
    }
}
