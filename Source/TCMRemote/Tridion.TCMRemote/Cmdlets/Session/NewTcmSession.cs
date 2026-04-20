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
using Tridion.TCMRemote.Authentication;
using Tridion.TCMRemote.Interfaces;
using Tridion.TCMRemote.Clients;
using Tridion.TCMRemote.Cmdlets.Session;
using Tridion.TCMRemote.Connection.Exceptions;
using Tridion.TCMRemote.Objects.Public;

namespace Tridion.TCMRemote.Cmdlets.Session
{
    /// <summary>
    /// <para type="synopsis">Creates a new authenticated TcmSession to a Tridion Sites Content Manager server.</para>
    /// <para type="description">The New-TcmSession cmdlet connects to a Tridion Sites Content Manager server and returns a TcmSession object.</para>
    /// <para type="description">All other TCMRemote cmdlets require a TcmSession.  The session is stored automatically in the current PowerShell session so that you do not need to pass -TcmSession explicitly.</para>
    /// <para type="description">Two authentication modes are supported:</para>
    /// <para type="description">  • Browser (default) — opens the system browser for interactive OIDC login with PKCE.</para>
    /// <para type="description">  • Client Credentials — for unattended automation; requires -ClientId and -ClientSecret.</para>
    /// </summary>
    /// <example>
    /// <code>$tcmSession = New-TcmSession -BaseUrl "https://tridion.example.com"</code>
    /// <para>Opens the system browser for interactive single sign-on and stores the resulting session.</para>
    /// </example>
    /// <example>
    /// <code>$tcmSession = New-TcmSession -BaseUrl "https://tridion.example.com" -ClientId "c826e7e1-..." -ClientSecret "ziKi..."</code>
    /// <para>Authenticates using OAuth2 Client Credentials — suitable for CI/CD pipelines.</para>
    /// </example>
    [Cmdlet(VerbsCommon.New, "TcmSession", SupportsShouldProcess = true)]
    [OutputType(typeof(TcmSession))]
    public sealed class NewTcmSession : SessionCmdlet
    {
        /// <summary>
        /// <para type="description">Root URL of the Tridion Sites Content Manager server, e.g. https://tridion.example.com</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// <para type="description">OAuth2 Client ID for the Client Credentials flow. Must be registered in Access Management.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ClientCredentials")]
        [ValidateNotNullOrEmpty]
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// <para type="description">OAuth2 Client Secret for the Client Credentials flow.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ClientCredentials")]
        [ValidateNotNullOrEmpty]
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// <para type="description">Override the OIDC Client ID used for browser-based login. Defaults to Tridion_Sites_Desktop_Client.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "Browser")]
        public string BrowserClientId { get; set; } = BrowserAuthService.DefaultClientId;

        /// <summary>
        /// <para type="description">
        /// Override the Access Management URL instead of auto-discovering it from the capabilities endpoint.
        /// Use this when Access Management is hosted at a non-standard address, or for troubleshooting.
        /// Example: "http://access.example.com:84/access-management"
        /// </para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string? AccessManagementUrl { get; set; }

        /// <summary>
        /// <para type="description">When set, skips TLS certificate validation. Use only in development/test environments.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter IgnoreSslErrors { get; set; }

        protected override void ProcessRecord()
        {
            if (!ShouldProcess(BaseUrl, "Create Tridion Sites Content Manager session"))
                return;

            try
            {
                if (IgnoreSslErrors.IsPresent)
                    HelperClasses.CertificateValidationHelper.OverrideCertificateValidation();

                string accessManagementUrl;
                if (!string.IsNullOrWhiteSpace(AccessManagementUrl))
                {
                    accessManagementUrl = AccessManagementUrl!.TrimEnd('/');
                    WriteVerbose($"Using supplied Access Management URL: {accessManagementUrl}");
                }
                else
                {
                    WriteVerbose($"Resolving Access Management URL from {BaseUrl}");
                    accessManagementUrl = TcmCapabilitiesHelper.GetAccessManagementUrl(BaseUrl);
                }
                WriteVerbose($"Access Management URL: {accessManagementUrl}");

                IAuthenticationService authService;
                string authType;

                if (ParameterSetName == "ClientCredentials")
                {
                    WriteVerbose("Using Client Credentials authentication");
                    authService = new ClientCredentialsAuthService(ClientId, ClientSecret, accessManagementUrl);
                    authType    = "ClientCredentials";
                }
                else
                {
                    WriteVerbose($"Using browser-based authentication (ClientId={BrowserClientId})");
                    authService = new BrowserAuthService(accessManagementUrl, BrowserClientId);
                    authType    = "Browser";
                }

                var tridionClient = new TridionCoreServiceClient(BaseUrl, authService);
                var tcmSession    = new TcmSession(tridionClient, BaseUrl, authType);

                WriteVerbose($"Session created: {tcmSession.Name}");
                SessionState.PSVariable.Set(TCMRemoteSessionStateTcmSession, tcmSession);
                WriteObject(tcmSession);
            }
            catch (TcmClientException ex)
            {
                WriteError(new ErrorRecord(ex, "TcmSessionCreationFailed", ErrorCategory.AuthenticationError, BaseUrl));
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to create TcmSession: {ex.Message}", ex),
                    "TcmSessionCreationFailed",
                    ErrorCategory.AuthenticationError,
                    BaseUrl));
            }
        }
    }
}
