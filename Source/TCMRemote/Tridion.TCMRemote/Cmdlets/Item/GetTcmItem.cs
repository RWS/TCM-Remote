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
    /// <para type="synopsis">Retrieves a Tridion Sites content item by its TCM URI.</para>
    /// <para type="description">Get-TcmItem queries the Tridion Sites Content Manager REST API and returns the IdentifiableObject for the given item. Supports components, pages, schemas, templates, and any other identifiable Tridion item.</para>
    /// </summary>
    /// <example>
    /// <code>Get-TcmItem -ItemId "tcm:1-2-16"</code>
    /// <para>Returns the component with TCM URI tcm:1-2-16.</para>
    /// </example>
    /// <example>
    /// <code>Get-TcmItem -ItemId "tcm:1-3-64" -UseDynamicVersion</code>
    /// <para>Returns the page using its dynamic (latest checked-in) version.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "TcmItem")]
    [OutputType(typeof(IdentifiableObject))]
    public sealed class GetTcmItem : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public TcmSession? TcmSession { get; set; }

        /// <summary>
        /// <para type="description">TCM URI of the item to retrieve, e.g. "tcm:1-2-16".</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// <para type="description">When set, retrieves the dynamic (latest checked-in) version of the item.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter UseDynamicVersion { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var session = ResolveSession(TcmSession);
                if (session == null) return;

                WriteVerbose($"Retrieving item [{ItemId}]");
                var item = session.Client.GetItemAsync(ItemId, UseDynamicVersion.IsPresent)
                                         .GetAwaiter().GetResult();
                WriteObject(item);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "GetTcmItemFailed", ErrorCategory.ReadError, ItemId));
            }
        }

        private TcmSession? ResolveSession(TcmSession? explicit_)
        {
            if (explicit_ != null) return explicit_;
            var variable = SessionState.PSVariable.Get(TCMRemoteSessionStateTcmSession);
            var session  = variable?.Value as TcmSession;
            if (session == null)
                WriteError(new ErrorRecord(
                    new InvalidOperationException(TCMRemoteSessionStateTcmSessionException),
                    "NoTcmSession", ErrorCategory.InvalidOperation, null));
            return session;
        }
    }
}
