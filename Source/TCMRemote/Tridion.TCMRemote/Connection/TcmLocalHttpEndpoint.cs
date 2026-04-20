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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tridion.TCMRemote.Connection.Exceptions;

namespace Tridion.TCMRemote.Connection
{
    /// <summary>
    /// Lightweight embedded HTTP server bound to a random free port on 127.0.0.1.
    /// Used as the OAuth2 / OIDC redirect-URI receiver during interactive browser login.
    /// </summary>
    internal sealed class TcmLocalHttpEndpoint : IDisposable
    {
        private readonly HttpListener _listener;
        private HttpListenerContext? _context;
        private bool _disposed;

        /// <summary>Path suffix appended to <see cref="BaseUrl"/> to signal an error callback.</summary>
        internal string ErrorUrlPath { get; set; } = "/error";

        /// <summary>Base URL at which this endpoint is listening, e.g. <c>http://127.0.0.1:52301/</c>.</summary>
        internal string BaseUrl { get; }

        internal TcmLocalHttpEndpoint()
        {
            int port = GetFreePort();
            BaseUrl = $"http://127.0.0.1:{port}/";
            _listener = new HttpListener();
            _listener.Prefixes.Add(BaseUrl);
        }

        /// <summary>Starts the HTTP listener.</summary>
        internal void StartListening()
        {
            _listener.Start();
        }

        /// <summary>
        /// Synchronously waits for a single inbound HTTP request (the OIDC callback).
        /// </summary>
        /// <param name="cancellationToken">Token used to abort the wait.</param>
        internal void AwaitHttpRequest(CancellationToken cancellationToken)
        {
            var contextTask = _listener.GetContextAsync();

            // Run synchronously — PowerShell cmdlets use .GetAwaiter().GetResult() throughout
            var completedIndex = Task.WaitAny(
                new Task[] { contextTask },
                (int)TimeSpan.FromMinutes(2).TotalMilliseconds,
                cancellationToken);

            if (completedIndex < 0)
                throw new TcmClientException("Interactive authentication timed out after 2 minutes.");

            _context = contextTask.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Signals an error to the browser.
        /// </summary>
        internal void HandleErrorNotification(string errorMessage)
        {
            if (_context == null) return;
            WriteHtmlResponse(_context.Response, $"<h2>Authentication Error</h2><p>{WebUtility.HtmlEncode(errorMessage)}</p>");
        }

        /// <summary>
        /// Returns the full request URL (including query string) so that
        /// <c>OidcClient.ProcessResponseAsync</c> can parse the authorization code.
        /// </summary>
        internal string GetRequestData()
        {
            if (_context == null)
                throw new TcmClientException("No HTTP request has been received yet.");

            return _context.Request.Url?.ToString() ?? string.Empty;
        }

        /// <summary>Sends an HTTP redirect response to the browser.</summary>
        internal async Task SendHttpRedirectAsync(string redirectUrl, CancellationToken cancellationToken)
        {
            if (_context == null) return;

            var response = _context.Response;
            response.StatusCode = 302;
            response.RedirectLocation = redirectUrl;
            response.ContentLength64 = 0;
            await response.OutputStream.FlushAsync(cancellationToken).ConfigureAwait(false);
            response.Close();
        }

        /// <summary>Sends a plain HTML success page and closes the response.</summary>
        internal async Task WriteHttpResponseAsync(string contentType, string responseBody, CancellationToken cancellationToken)
        {
            if (_context == null) return;

            var buffer = Encoding.UTF8.GetBytes(responseBody);
            var response = _context.Response;
            response.ContentType = contentType;
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
            response.Close();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try { _listener.Stop(); } catch { /* best-effort */ }
            try { _listener.Close(); } catch { /* best-effort */ }
        }

        // ------------------------------------------------------------------ helpers

        private static void WriteHtmlResponse(HttpListenerResponse response, string body)
        {
            var html = $"<html><body>{body}</body></html>";
            var buffer = Encoding.UTF8.GetBytes(html);
            response.ContentType = "text/html; charset=utf-8";
            response.ContentLength64 = buffer.Length;
            try
            {
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.Close();
            }
            catch { /* best-effort */ }
        }

        private static int GetFreePort()
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            return ((IPEndPoint)socket.LocalEndPoint!).Port;
        }
    }
}
