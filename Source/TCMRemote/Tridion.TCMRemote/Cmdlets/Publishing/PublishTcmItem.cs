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

namespace Tridion.TCMRemote.Cmdlets.Publishing
{
    /// <summary>
    /// <para type="synopsis">Publishes one or more Tridion Sites content items to a publish target.</para>
    /// <para type="description">Publish-TcmItem submits a publish transaction for the specified item(s) and publish target. Returns the PublishTransactionsCreationResult from the server. Use -WhatIf to preview what would be published.</para>
    /// </summary>
    /// <example>
    /// <code>Publish-TcmItem -ItemId "tcm:1-2-64" -PublishTargetId "tcm:0-1-65537"</code>
    /// <para>Publishes a page to the specified publish target.</para>
    /// </example>
    [Cmdlet(VerbsData.Publish, "TcmItem", SupportsShouldProcess = true)]
    [OutputType(typeof(PublishTransactionsCreationResult))]
    public sealed class PublishTcmItem : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public TcmSession? TcmSession { get; set; }

        /// <summary>
        /// <para type="description">TCM URI of the item to publish, e.g. "tcm:1-2-64".</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// <para type="description">TCM URI of the publish target (publication target), e.g. "tcm:0-1-65537".</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        [ValidateNotNullOrEmpty]
        public string PublishTargetId { get; set; } = string.Empty;

        /// <summary>
        /// <para type="description">When set, the publish transaction is queued with high priority.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Republish { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                if (!ShouldProcess(ItemId, $"Publish to [{PublishTargetId}]"))
                    return;

                var session = ResolveSession(TcmSession);
                if (session == null) return;

                var request = new PublishRequest
                {
                    Ids                = new[] { ItemId },
                    TargetIdsOrPurposes = new[] { PublishTargetId },
                    Priority           = Republish.IsPresent ? PublishPriority.High : (PublishPriority?)null
                };

                WriteVerbose($"Publishing [{ItemId}] to [{PublishTargetId}]");
                var result = session.Client.PublishAsync(request).GetAwaiter().GetResult();
                WriteObject(result);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "PublishTcmItemFailed", ErrorCategory.WriteError, ItemId));
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
