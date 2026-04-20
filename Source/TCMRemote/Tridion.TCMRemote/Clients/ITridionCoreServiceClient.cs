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
using System.Threading;
using System.Threading.Tasks;
using Tridion.TCMRemote.OpenApiTCM30;

namespace Tridion.TCMRemote.Clients
{
    /// <summary>Contract for the Tridion Sites Content Manager REST API client.</summary>
    public interface ITridionCoreServiceClient : IDisposable
    {
        /// <summary>
        /// Returns the authenticated low-level <see cref="OpenApiTCM30Client"/> with all REST methods available.
        /// Equivalent to <c>Get-TridionCoreServiceClient</c> in the legacy WCF module.
        /// The token is refreshed automatically on each call.
        /// </summary>
        Task<OpenApiTCM30Client> GetApiClientAsync(CancellationToken cancellationToken = default);

        // Users
        Task<UserProfile> GetCurrentUserAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetUsersAsync(bool? predefined = null, bool? includeDisabled = null, string? search = null, StringSearchMode? searchMode = null, CancellationToken cancellationToken = default);
        Task<IdentifiableObject> UpdateUserAsync(string userId, IdentifiableObject userItem, CancellationToken cancellationToken = default);

        // Groups
        Task<ICollection<Group>> GetGroupsAsync(string? inPublicationId = null, bool? includeEveryone = null, string? search = null, StringSearchMode? searchMode = null, CancellationToken cancellationToken = default);

        // Items (generic)
        Task<IdentifiableObject> GetItemAsync(string itemId, bool useDynamicVersion = false, CancellationToken cancellationToken = default);
        Task<IdentifiableObject> CreateItemAsync(IdentifiableObject item, bool autoCheckIn = true, CancellationToken cancellationToken = default);
        Task<IdentifiableObject> UpdateItemAsync(string itemId, IdentifiableObject item, CancellationToken cancellationToken = default);
        Task<bool> DeleteItemAsync(string itemId, CancellationToken cancellationToken = default);
        Task<bool> ItemExistsAsync(string itemId, CancellationToken cancellationToken = default);

        // Item operations
        Task<RepositoryLocalObject> MoveItemAsync(string itemId, string destinationId, CancellationToken cancellationToken = default);
        Task<RepositoryLocalObject> CopyItemAsync(string itemId, string destinationId, bool makeUnique = true, CancellationToken cancellationToken = default);
        Task<ICollection<IdentifiableObject>> FindItemsAsync(string fullTextQuery, int? resultLimit = null, IEnumerable<string>? keywordIds = null, CancellationToken cancellationToken = default);

        // Versioning
        Task<ICollection<VersionedItem>> GetItemHistoryAsync(string itemId, CancellationToken cancellationToken = default);
        Task<VersionedItem> CheckInAsync(string itemId, bool? removePermanentLock = null, CancellationToken cancellationToken = default);
        Task<VersionedItem> CheckOutAsync(string itemId, bool? setPermanentLock = null, CancellationToken cancellationToken = default);
        Task<VersionedItem> UndoCheckOutAsync(string itemId, bool? removePermanentLock = null, CancellationToken cancellationToken = default);

        // Relationships
        Task<ICollection<IdentifiableObject>> GetItemUsesAsync(string itemId, bool? includeBlueprintParent = null, bool? useDynamicVersion = null, CancellationToken cancellationToken = default);
        Task<ICollection<IdentifiableObject>> GetItemUsedByAsync(string itemId, bool? onlyLatestVersions = null, bool? useDynamicVersion = null, bool? includeLocalCopies = null, CancellationToken cancellationToken = default);
        Task<ICollection<RepositoryLocalObject>> GetItemsInContainerAsync(string containerId, bool? useDynamicVersion = null, bool? recursive = null, CancellationToken cancellationToken = default);

        // Publications
        Task<Publication[]> GetPublicationsAsync(CancellationToken cancellationToken = default);
        Task<Publication> GetPublicationAsync(string itemId, bool useDynamicVersion = false, CancellationToken cancellationToken = default);

        // Publication Targets
        Task<ICollection<TargetType>> GetTargetTypesAsync(CancellationToken cancellationToken = default);

        // Publishing
        Task<PublishTransactionsCreationResult> PublishAsync(PublishRequest request, CancellationToken cancellationToken = default);
        Task<PublishTransactionsCreationResult> UnpublishAsync(UnPublishRequest request, CancellationToken cancellationToken = default);
        Task<ICollection<PublishTransaction>> GetPublishTransactionsAsync(string? userId = null, string? publicationId = null, string? targetTypeId = null, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null, PublishPriority? priority = null, PublishTransactionState? state = null, CancellationToken cancellationToken = default);

        // Workflow / Activity
        Task<ICollection<ActivityInstance>> GetActivityInstancesAsync(bool? forAllUsers = null, string? ownerId = null, string? assigneeId = null, IEnumerable<ActivityState>? activityStates = null, string? processDefinitionId = null, CancellationToken cancellationToken = default);

        // Application Settings
        Task<Settings> GetApplicationSettingsAsync(string applicationId, CancellationToken cancellationToken = default);
        Task SaveApplicationSettingsAsync(string applicationId, Settings settings, CancellationToken cancellationToken = default);
        Task DeleteApplicationSettingsAsync(string applicationId, CancellationToken cancellationToken = default);
    }
}
