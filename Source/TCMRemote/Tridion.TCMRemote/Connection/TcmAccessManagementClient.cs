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
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient;
using Duende.IdentityModel.OidcClient.Infrastructure;
using Tridion.TCMRemote.Connection.Exceptions;

namespace Tridion.TCMRemote.Connection
{
    /// <summary>
    /// Provides OAuth2 / OpenID Connect token acquisition for Tridion Sites Content Manager.
    /// Supports two flows:
    /// <list type="bullet">
    ///   <item>Client Credentials — for server-to-server automation (ClientId + ClientSecret).</item>
    ///   <item>Authorization Code with PKCE — for interactive user login via the system browser.</item>
    /// </list>
    /// Tokens are cached per <c>clientId</c> and automatically refreshed using the stored refresh
    /// token before they expire.
    /// </summary>
    /// <remarks>
    /// This class is modelled on the <c>AccessManagementDesktopClient</c> from the Tridion Access
    /// Management SDK and is safe to share across multiple cmdlet invocations within a single
    /// PowerShell session.
    /// </remarks>
    public sealed class TcmAccessManagementClient : IDisposable
    {
        // ------------------------------------------------------------------ inner types

        private sealed class Tokens
        {
            internal string AccessToken { get; set; } = string.Empty;
            internal string? IdentityToken { get; set; }
            internal string? RefreshToken { get; set; }
            internal DateTimeOffset AccessTokenExpiration { get; set; }
        }

        // ------------------------------------------------------------------ fields

        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private static readonly Dictionary<string, Tokens> _tokensCache =
            new Dictionary<string, Tokens>(StringComparer.OrdinalIgnoreCase);

        private readonly string _accessManagementBaseUrl;
        private readonly Policy _oidcPolicy;
        private bool _disposed;

        // ------------------------------------------------------------------ public surface

        /// <summary>
        /// How long before the token expires the client should proactively refresh it.
        /// Default is 1 minute.
        /// </summary>
        public TimeSpan RefreshBeforeExpiration { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Optional listener that receives notifications when interactive browser
        /// authentication starts and finishes (useful for showing UI feedback).
        /// </summary>
        public ITcmUserAuthListener? UserAuthListener { get; set; }

        // ------------------------------------------------------------------ constructor

        /// <summary>
        /// Initialises a new <see cref="TcmAccessManagementClient"/>.
        /// </summary>
        /// <param name="accessManagementBaseUrl">
        /// Base URL of the Tridion Access Management server,
        /// e.g. <c>https://tridion.example.com/access-management</c>.
        /// </param>
        public TcmAccessManagementClient(string accessManagementBaseUrl)
        {
            if (string.IsNullOrWhiteSpace(accessManagementBaseUrl))
                throw new ArgumentNullException(nameof(accessManagementBaseUrl));

            _accessManagementBaseUrl = accessManagementBaseUrl.TrimEnd('/');

            _oidcPolicy = new Policy
            {
                Discovery = new DiscoveryPolicy
                {
                    ValidateIssuerName = false,
                    RequireHttps = false,
                    AuthorityValidationStrategy = new StringComparisonAuthorityValidationStrategy()
                }
            };
        }

        // ------------------------------------------------------------------ public methods

        /// <summary>
        /// Acquires an access token using the OAuth2 Client Credentials flow.
        /// The token is cached and automatically refreshed when it is about to expire.
        /// </summary>
        /// <param name="clientId">The client identifier registered in Access Management.</param>
        /// <param name="clientSecret">The client secret registered in Access Management.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A valid bearer access token.</returns>
        public async Task<string> GetAccessTokenForClientCredentialsAsync(
            string clientId,
            string clientSecret,
            CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var cacheKey = clientId;
                var cached = await GetValidCachedTokenAsync(cacheKey, cancellationToken).ConfigureAwait(false);
                if (cached != null) return cached.AccessToken;

                using var httpClient = new HttpClient();
                var disco = await httpClient.GetDiscoveryDocumentAsync(
                    new DiscoveryDocumentRequest { Address = _accessManagementBaseUrl, Policy = _oidcPolicy.Discovery },
                    cancellationToken).ConfigureAwait(false);

                if (disco.IsError)
                    throw new TcmClientException($"OIDC discovery failed: {disco.Error}");

                var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(
                    new ClientCredentialsTokenRequest
                    {
                        Address = disco.TokenEndpoint,
                        ClientId = clientId,
                        ClientSecret = clientSecret,
                        Scope = "openid profile email role forwarded offline_access"
                    }, cancellationToken).ConfigureAwait(false);

                if (tokenResponse.IsError)
                    throw new TcmTokenException(
                        $"Client credentials token request failed: {tokenResponse.Error} — {tokenResponse.ErrorDescription}",
                        MapTokenErrorCode(tokenResponse.Error));

                var tokens = new Tokens
                {
                    AccessToken = tokenResponse.AccessToken!,
                    RefreshToken = tokenResponse.RefreshToken,
                    AccessTokenExpiration = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn)
                };
                _tokensCache[cacheKey] = tokens;
                return tokens.AccessToken;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Acquires an access token for a human user via an interactive browser login
        /// (Authorization Code flow with PKCE).
        /// The token is cached and automatically refreshed using the stored refresh token.
        /// </summary>
        /// <param name="applicationClientId">
        /// The public client identifier configured in Access Management for desktop/interactive use,
        /// e.g. <c>Tridion_Sites_Desktop_Client</c>.
        /// </param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A valid bearer access token.</returns>
        public async Task<string> GetAccessTokenForUserAsync(
            string applicationClientId,
            CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var cacheKey = applicationClientId;
                var cached = await GetValidCachedTokenAsync(cacheKey, cancellationToken).ConfigureAwait(false);
                if (cached != null) return cached.AccessToken;

                var tokens = await AuthenticateUserAsync(applicationClientId, cancellationToken).ConfigureAwait(false);
                _tokensCache[cacheKey] = tokens;
                return tokens.AccessToken;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>Signs out the currently cached user session and revokes any stored tokens.</summary>
        public async Task SignOutUserAsync(
            string applicationClientId,
            CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                _tokensCache.Remove(applicationClientId);

                using var httpClient = new HttpClient();
                var disco = await httpClient.GetDiscoveryDocumentAsync(
                    new DiscoveryDocumentRequest { Address = _accessManagementBaseUrl, Policy = _oidcPolicy.Discovery },
                    cancellationToken).ConfigureAwait(false);

                if (!disco.IsError && !string.IsNullOrEmpty(disco.EndSessionEndpoint))
                {
                    var endSessionUrl = $"{disco.EndSessionEndpoint}?client_id={Uri.EscapeDataString(applicationClientId)}";
                    OpenBrowser(endSessionUrl);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _disposed = true;
        }

        // ------------------------------------------------------------------ private methods

        private async Task<Tokens?> GetValidCachedTokenAsync(string cacheKey, CancellationToken cancellationToken)
        {
            if (!_tokensCache.TryGetValue(cacheKey, out var tokens))
                return null;

            // Token is still valid with headroom
            if (tokens.AccessTokenExpiration - RefreshBeforeExpiration > DateTimeOffset.UtcNow)
                return tokens;

            // Try to refresh using the stored refresh token
            if (!string.IsNullOrEmpty(tokens.RefreshToken))
            {
                try
                {
                    await RefreshTokensAsync(cacheKey, tokens, cancellationToken).ConfigureAwait(false);
                    return _tokensCache.TryGetValue(cacheKey, out var refreshed) ? refreshed : null;
                }
                catch (TcmTokenException)
                {
                    _tokensCache.Remove(cacheKey);
                    throw;
                }
            }

            _tokensCache.Remove(cacheKey);
            return null;
        }

        private async Task RefreshTokensAsync(string cacheKey, Tokens tokens, CancellationToken cancellationToken)
        {
            using var httpClient = new HttpClient();
            var disco = await httpClient.GetDiscoveryDocumentAsync(
                new DiscoveryDocumentRequest { Address = _accessManagementBaseUrl, Policy = _oidcPolicy.Discovery },
                cancellationToken).ConfigureAwait(false);

            if (disco.IsError)
                throw new TcmClientException($"OIDC discovery failed during token refresh: {disco.Error}");

            var refreshResponse = await httpClient.RequestRefreshTokenAsync(
                new RefreshTokenRequest
                {
                    Address = disco.TokenEndpoint,
                    RefreshToken = tokens.RefreshToken!
                }, cancellationToken).ConfigureAwait(false);

            if (refreshResponse.IsError)
            {
                _tokensCache.Remove(cacheKey);
                throw new TcmTokenException(
                    $"Token refresh failed: {refreshResponse.Error} — {refreshResponse.ErrorDescription}",
                    MapTokenErrorCode(refreshResponse.Error));
            }

            _tokensCache[cacheKey] = new Tokens
            {
                AccessToken = refreshResponse.AccessToken!,
                IdentityToken = refreshResponse.IdentityToken,
                RefreshToken = refreshResponse.RefreshToken ?? tokens.RefreshToken,
                AccessTokenExpiration = DateTimeOffset.UtcNow.AddSeconds(refreshResponse.ExpiresIn)
            };
        }

        private async Task<Tokens> AuthenticateUserAsync(string clientId, CancellationToken cancellationToken)
        {
            using var localEndpoint = new TcmLocalHttpEndpoint();
            localEndpoint.StartListening();

            var oidcOptions = new OidcClientOptions
            {
                Authority = _accessManagementBaseUrl,
                ClientId = clientId,
                Scope = "openid profile email role forwarded offline_access",
                RedirectUri = localEndpoint.BaseUrl,
                Policy = _oidcPolicy
            };

            var oidcClient = new OidcClient(oidcOptions);
            var loginRequest = await oidcClient.PrepareLoginAsync(cancellationToken: cancellationToken)
                                               .ConfigureAwait(false);

            var cts = new CancellationTokenSource();
            UserAuthListener?.OnAuthStarting(cts);

            try
            {
                OpenBrowser(loginRequest.StartUrl);

                localEndpoint.AwaitHttpRequest(cts.Token);

                // Send a simple "you can close this window" page to the browser
                await localEndpoint.WriteHttpResponseAsync(
                    "text/html; charset=utf-8",
                    "<html><body><h2>Authentication successful</h2>" +
                    "<p>You may close this window and return to PowerShell.</p></body></html>",
                    cts.Token).ConfigureAwait(false);

                var requestData = localEndpoint.GetRequestData();
                var loginResult = await oidcClient.ProcessResponseAsync(requestData, loginRequest, cancellationToken: cancellationToken)
                                                  .ConfigureAwait(false);

                if (loginResult.IsError)
                    throw new TcmClientException($"Authentication failed: {loginResult.Error}");

                return new Tokens
                {
                    AccessToken = loginResult.AccessToken,
                    IdentityToken = loginResult.IdentityToken,
                    RefreshToken = loginResult.RefreshToken,
                    AccessTokenExpiration = loginResult.AccessTokenExpiration
                };
            }
            finally
            {
                UserAuthListener?.OnAuthFinished();
            }
        }

        private static void OpenBrowser(string url)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    System.Diagnostics.Process.Start("xdg-open", url);
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    System.Diagnostics.Process.Start("open", url);
            }
            catch (Exception ex)
            {
                throw new TcmClientException($"Failed to open browser for authentication: {ex.Message}", ex);
            }
        }

        private static TcmTokenErrorCode MapTokenErrorCode(string? error) =>
            error switch
            {
                "invalid_request"        => TcmTokenErrorCode.InvalidRequest,
                "invalid_client"         => TcmTokenErrorCode.InvalidClient,
                "invalid_grant"          => TcmTokenErrorCode.InvalidGrant,
                "unauthorized_client"    => TcmTokenErrorCode.UnauthorizedClient,
                "unsupported_grant_type" => TcmTokenErrorCode.UnsupportedGrantType,
                "invalid_scope"          => TcmTokenErrorCode.InvalidScope,
                _                        => TcmTokenErrorCode.Unknown
            };
    }
}
