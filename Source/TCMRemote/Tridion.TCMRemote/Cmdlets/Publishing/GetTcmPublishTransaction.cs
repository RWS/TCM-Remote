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
    /// <para type="synopsis">Returns publish transactions from the Tridion Sites Content Manager.</para>
    /// <para type="description">Get-TcmPublishTransaction returns the list of PublishTransaction objects matching the provided filters. Use -State to filter by status, -PublicationId to scope to a publication, or -TargetId to scope to a specific target.</para>
    /// </summary>
    /// <example>
    /// <code>Get-TcmPublishTransaction</code>
    /// <para>Returns all publish transactions.</para>
    /// </example>
    /// <example>
    /// <code>Get-TcmPublishTransaction -State Waiting</code>
    /// <para>Returns all transactions currently in the Waiting state.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "TcmPublishTransaction")]
    [OutputType(typeof(PublishTransaction))]
    public sealed class GetTcmPublishTransaction : TridionCmdlet
    {
        /// <summary>
        /// <para type="description">Session to use. When omitted, the session stored by New-TcmSession is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public TcmSession? TcmSession { get; set; }

        /// <summary>
        /// <para type="description">Filter by user TCM URI.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string? UserId { get; set; }

        /// <summary>
        /// <para type="description">Filter by publication TCM URI.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string? PublicationId { get; set; }

        /// <summary>
        /// <para type="description">Filter by publication target (TargetType) TCM URI.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string? TargetId { get; set; }

        /// <summary>
        /// <para type="description">Filter by transaction state.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public PublishTransactionState? State { get; set; }

        /// <summary>
        /// <para type="description">Filter by publish priority.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public PublishPriority? Priority { get; set; }

        /// <summary>
        /// <para type="description">Return transactions created on or after this date/time (UTC).</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public DateTimeOffset? StartDate { get; set; }

        /// <summary>
        /// <para type="description">Return transactions created on or before this date/time (UTC).</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public DateTimeOffset? EndDate { get; set; }

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
            try
            {
                WriteVerbose("Retrieving publish transactions");
                var transactions = _resolvedSession!.Client.GetPublishTransactionsAsync(
                    userId: UserId,
                    publicationId: PublicationId,
                    targetTypeId: TargetId,
                    startDate: StartDate,
                    endDate: EndDate,
                    priority: Priority,
                    state: State
                ).GetAwaiter().GetResult();

                foreach (var t in transactions)
                    WriteObject(t);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to get publish transactions: {ex.Message}", ex),
                    "GetTcmPublishTransactionFailed",
                    ErrorCategory.ReadError,
                    null));
            }
        }
    }
}
