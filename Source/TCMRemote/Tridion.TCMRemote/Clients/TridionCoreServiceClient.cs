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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tridion.TCMRemote.Interfaces;
using Tridion.TCMRemote.OpenApiTCM30;

namespace Tridion.TCMRemote.Clients
{
    /// <summary>
    /// HTTP client wrapper for the Tridion Sites Content Manager REST API (v3.0).
    /// Authenticates every request with a bearer token obtained from <see cref="IAuthenticationService"/>.
    /// </summary>
    public sealed class TridionCoreServiceClient : ITridionCoreServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthenticationService _authService;
        private readonly string _apiBaseUrl;

        /// <summary>
        /// Initialises a new <see cref="TridionCoreServiceClient"/>.
        /// </summary>
        /// <param name="baseUrl">Root URL of the Tridion Sites server, e.g. <c>https://tridion.example.com</c>.</param>
        /// <param name="authService">Authentication service that provides bearer tokens.</param>
        public TridionCoreServiceClient(string baseUrl, IAuthenticationService authService)
        {
            _apiBaseUrl  = baseUrl.TrimEnd('/') + "/api";
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _httpClient  = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
        }

        // ------------------------------------------------------------------ Raw API client

        /// <inheritdoc/>
        public async Task<OpenApiTCM30Client> GetApiClientAsync(CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            return GetOpenApiClient();
        }

        // ------------------------------------------------------------------ Users

        /// <inheritdoc/>
        public async Task<UserProfile> GetCurrentUserAsync(CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            return await oapi.GetOwnUserProfileAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<User>> GetUsersAsync(
            bool? predefined = null,
            bool? includeDisabled = null,
            string? search = null,
            StringSearchMode? searchMode = null,
            CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            return await oapi.GetUsersAsync(predefined, includeDisabled, search, searchMode, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IdentifiableObject> UpdateUserAsync(string userId, IdentifiableObject userItem, CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            return await oapi.UpdateAsync(EscapeItemId(userId), userItem, cancellationToken).ConfigureAwait(false);
        }

        // ------------------------------------------------------------------ Groups

        /// <inheritdoc/>
        public async Task<ICollection<Group>> GetGroupsAsync(
            string? inPublicationId = null,
            bool? includeEveryone = null,
            string? search = null,
            StringSearchMode? searchMode = null,
            CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            return await oapi.GetGroupsAsync(inPublicationId, includeEveryone, search, searchMode, cancellationToken).ConfigureAwait(false);
        }

        // ------------------------------------------------------------------ Items

        /// <inheritdoc/>
        public async Task<IdentifiableObject> GetItemAsync(
            string itemId,
            bool useDynamicVersion = false,
            CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            return await oapi.GetItemAsync(EscapeItemId(itemId), useDynamicVersion, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IdentifiableObject> CreateItemAsync(
            IdentifiableObject item,
            bool autoCheckIn = true,
            CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            return await oapi.CreateAsync(item, autoCheckIn, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IdentifiableObject> UpdateItemAsync(
            string itemId,
            IdentifiableObject item,
            CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            return await oapi.UpdateAsync(EscapeItemId(itemId), item, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteItemAsync(string itemId, CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            try
            {
                await oapi.DeleteAsync(EscapeItemId(itemId), cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (OpenApiTCM30Exception)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ItemExistsAsync(string itemId, CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            try
            {
                await oapi.ItemExistsAsync(EscapeItemId(itemId), cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (OpenApiTCM30Exception ex) when (ex.StatusCode == 404)
            {
                return false;
            }
        }

        // ------------------------------------------------------------------ Item operations

        /// <inheritdoc/>
        public async Task<RepositoryLocalObject> MoveItemAsync(string itemId, string destinationId, CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            return await oapi.MoveAsync(EscapeItemId(itemId), EscapeItemId(destinationId), cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<RepositoryLocalObject> CopyItemAsync(string itemId, string destinationId, bool makeUnique = true, CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            return await oapi.CopyAsync(EscapeItemId(itemId), EscapeItemId(destinationId), makeUnique, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<ICollection<IdentifiableObject>> FindItemsAsync(string fullTextQuery, int? resultLimit = null, IEnumerable<string>? keywordIds = null, CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            return await oapi.SystemSearchAsync(fullTextQuery, resultLimit, keywordIds, cancellationToken).ConfigureAwait(false);
        }

        // ------------------------------------------------------------------ Versioning

        /// <inheritdoc/>
        public async Task<ICollection<VersionedItem>> GetItemHistoryAsync(string itemId, CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            return await oapi.GetHistoryAsync(EscapeItemId(itemId), cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<VersionedItem> CheckInAsync(string itemId, bool? removePermanentLock = null, CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            return await oapi.CheckInAsync(EscapeItemId(itemId), new CheckInRequest { RemovePermanentLock = removePermanentLock }, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<VersionedItem> CheckOutAsync(string itemId, bool? setPermanentLock = null, CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            return await oapi.CheckOutAsync(EscapeItemId(itemId), new CheckOutRequest { SetPermanentLock = setPermanentLock }, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<VersionedItem> UndoCheckOutAsync(string itemId, bool? removePermanentLock = null, CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            return await oapi.UndoCheckOutAsync(EscapeItemId(itemId), new UndoCheckOutRequest { RemovePermanentLock = removePermanentLock }, cancellationToken).ConfigureAwait(false);
        }

        // ------------------------------------------------------------------ Relationships

        /// <inheritdoc/>
        public async Task<ICollection<IdentifiableObject>> GetItemUsesAsync(string itemId, bool? includeBlueprintParent = null, bool? useDynamicVersion = null, CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            return await oapi.GetUsesAsync(EscapeItemId(itemId), includeBlueprintParent, useDynamicVersion, null, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<ICollection<IdentifiableObject>> GetItemUsedByAsync(string itemId, bool? onlyLatestVersions = null, bool? useDynamicVersion = null, bool? includeLocalCopies = null, CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            return await oapi.GetUsedByAsync(EscapeItemId(itemId), onlyLatestVersions, useDynamicVersion, includeLocalCopies, null, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<ICollection<RepositoryLocalObject>> GetItemsInContainerAsync(string containerId, bool? useDynamicVersion = null, bool? recursive = null, CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            return await oapi.GetItemsFromContainerAsync(EscapeItemId(containerId), useDynamicVersion, null, recursive, null, cancellationToken).ConfigureAwait(false);
        }

        // ------------------------------------------------------------------ Publications

        /// <inheritdoc/>
        public async Task<Publication[]> GetPublicationsAsync(CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            var result = await oapi.GetPublicationsAsync(details: null, cancellationToken).ConfigureAwait(false);
            var arr = new Publication[result.Count];
            result.CopyTo(arr, 0);
            return arr;
        }

        /// <inheritdoc/>
        public async Task<Publication> GetPublicationAsync(
            string itemId,
            bool useDynamicVersion = false,
            CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            var item = await oapi.GetItemAsync(EscapeItemId(itemId), useDynamicVersion, cancellationToken).ConfigureAwait(false);
            return (Publication)item;
        }

        // ------------------------------------------------------------------ Publication Targets

        /// <inheritdoc/>
        public async Task<ICollection<TargetType>> GetTargetTypesAsync(CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            return await oapi.GetTargetTypesAsync(cancellationToken).ConfigureAwait(false);
        }

        // ------------------------------------------------------------------ Publishing

        /// <inheritdoc/>
        public async Task<PublishTransactionsCreationResult> PublishAsync(
            PublishRequest request,
            CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            return await oapi.PublishAsync(request, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<PublishTransactionsCreationResult> UnpublishAsync(
            UnPublishRequest request,
            CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            return await oapi.UnpublishAsync(request, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<ICollection<PublishTransaction>> GetPublishTransactionsAsync(
            string? userId = null,
            string? publicationId = null,
            string? targetTypeId = null,
            DateTimeOffset? startDate = null,
            DateTimeOffset? endDate = null,
            PublishPriority? priority = null,
            PublishTransactionState? state = null,
            CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            return await oapi.GetPublishTransactionsAsync(userId, publicationId, targetTypeId, startDate, endDate, priority, state, null, cancellationToken).ConfigureAwait(false);
        }

        // ------------------------------------------------------------------ Workflow / Activity

        /// <inheritdoc/>
        public async Task<ICollection<ActivityInstance>> GetActivityInstancesAsync(
            bool? forAllUsers = null,
            string? ownerId = null,
            string? assigneeId = null,
            IEnumerable<ActivityState>? activityStates = null,
            string? processDefinitionId = null,
            CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            return await oapi.ListActivityInstancesAsync(forAllUsers, ownerId, assigneeId, activityStates, processDefinitionId, cancellationToken).ConfigureAwait(false);
        }

        // ------------------------------------------------------------------ Application Settings

        /// <inheritdoc/>
        public async Task<Settings> GetApplicationSettingsAsync(string applicationId, CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            return await oapi.GetApplicationSettingsAsync(applicationId, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task SaveApplicationSettingsAsync(string applicationId, Settings settings, CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            await oapi.SaveApplicationSettingsAsync(applicationId, settings, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task DeleteApplicationSettingsAsync(string applicationId, CancellationToken cancellationToken = default)
        {
            await GetAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
            var oapi = GetOpenApiClient();
            await oapi.DeleteApplicationSettingsAsync(applicationId, cancellationToken).ConfigureAwait(false);
        }

        // ------------------------------------------------------------------ IDisposable

        /// <inheritdoc/>
        public void Dispose() => _httpClient?.Dispose();

        // ------------------------------------------------------------------ helpers

        private async Task<HttpClient> GetAuthenticatedClientAsync(CancellationToken cancellationToken)
        {
            var token = await _authService.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return _httpClient;
        }

        private OpenApiTCM30Client GetOpenApiClient()
        {
            var client = new OpenApiTCM30Client(_httpClient);
            client.BaseUrl = _apiBaseUrl;
            return client;
        }

        private static string EscapeItemId(string itemId) =>
            Uri.EscapeDataString(itemId.Replace(":", "_"));

        private static string BuildUrl(string baseUrl, params (string Key, string? Value)[] queryParams)
        {
            var sb = new StringBuilder(baseUrl).Append('?');
            foreach (var (key, value) in queryParams)
            {
                if (!string.IsNullOrEmpty(value))
                    sb.Append(key).Append('=').Append(Uri.EscapeDataString(value)).Append('&');
            }
            return sb.ToString().TrimEnd('&', '?');
        }

        private static StringContent JsonContent<T>(T obj) =>
            new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");

        private static T? Deserialize<T>(string json) =>
            JsonConvert.DeserializeObject<T>(json);
    }
}
