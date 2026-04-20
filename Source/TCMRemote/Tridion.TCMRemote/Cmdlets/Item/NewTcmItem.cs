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

namespace Tridion.TCMRemote.Cmdlets.Item
{
    /// <summary>
    /// <para type="synopsis">Creates a new item in the Tridion Sites Content Manager.</para>
    /// <para type="description">New-TcmItem creates a new repository item (Component, Folder, StructureGroup, Schema, etc.) by passing an IdentifiableObject to the REST API. Use Get-TcmItem to retrieve a default model first with Get-TcmDefaultModel, or construct the object manually.</para>
    /// </summary>
    /// <example>
    /// <code>$folder = [Tridion.TCMRemote.OpenApiTCM30.Folder]@{ Title = "NewFolder"; LocationInfo = @{ OrganizationalItem = @{ Id = "tcm:1-1-2"; Title = "Root Folder" } } }; New-TcmItem -Item $folder</code>
    /// <para>Creates a new folder under the root folder of publication 1.</para>
    /// </example>
    [Cmdlet(VerbsCommon.New, "TcmItem", SupportsShouldProcess = true)]
    [OutputType(typeof(IdentifiableObject))]
    public sealed class NewTcmItem : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public TcmSession? TcmSession { get; set; }

        /// <summary>
        /// <para type="description">The item object to create. Must be a typed IdentifiableObject subclass (e.g. Folder, Component, Schema).</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
        [ValidateNotNull]
        public IdentifiableObject Item { get; set; } = null!;

        /// <summary>
        /// <para type="description">When specified, the item is checked in automatically after creation.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter NoAutoCheckIn { get; set; }

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
            var label = Item.Title ?? Item.Id ?? "new item";
            if (!ShouldProcess(label, "Create Item"))
                return;

            try
            {
                WriteVerbose($"Creating item [{label}]");
                var result = _resolvedSession!.Client.CreateItemAsync(Item, autoCheckIn: !NoAutoCheckIn.IsPresent).GetAwaiter().GetResult();
                WriteObject(result);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to create item: {ex.Message}", ex),
                    "NewTcmItemFailed",
                    ErrorCategory.WriteError,
                    label));
            }
        }
    }
}
