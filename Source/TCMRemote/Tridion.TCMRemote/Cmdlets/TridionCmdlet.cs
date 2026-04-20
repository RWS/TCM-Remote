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
using System.Linq;
using System.Management.Automation;
using System.Threading;
using Tridion.TCMRemote.Interfaces;

namespace Tridion.TCMRemote.Cmdlets
{

    /// <summary>
    /// Which verb to use: check http://msdn.microsoft.com/en-us/library/ms714428(v=vs.85).aspx
    /// How to trace the pipelin: http://www.eggheadcafe.com/software/aspnet/31521302/valuefrompipelinebypropertyname.aspx
    /// External MAML help: http://cmille19.wordpress.com/2009/09/24/external-maml-help-files/
    ///                and: http://technet.microsoft.com/en-us/library/dd819489.aspx
    /// Progress Record: http://community.bartdesmet.net/blogs/bart/archive/2006/11/26/PowerShell-_2D00_-A-cmdlet-that-reports-progress-_2D00_-A-simple-file-downloader-cmdlet.aspx
    /// </summary>
    public abstract class TridionCmdlet : PSCmdlet
    {
        /// <summary>
        /// Sleep constant used to slow down the progress bars to check the messages.
        /// </summary>
        protected const int SleepTime = 0;

        public readonly ILogger Logger;

        private int _tickCountStart;

        private readonly ProgressRecord _parentProgressRecord;
        protected int _parentCurrent;
        protected int _parentTotal;
        private readonly ProgressRecord _childProgressRecord;

        /// <summary>
        /// Name of the PSVariable so you don't have to specify '-TcmSession $TcmSession' anymore, should be set by New-TcmSession
        /// </summary>
        internal const string TCMRemoteSessionStateTcmSession = "TCMRemoteSessionStateTcmSession";
        /// <summary>
        /// Error message you get when you didn't pass an explicit -TcmSession on the cmdlet, or New-TcmSession didn't set the SessionState variable
        /// </summary>
        internal const string TCMRemoteSessionStateTcmSessionException = "TcmSession is null. Please create a session first using New-TcmSession. Or explicitly pass parameter -TcmSession to your cmdlet.";

        protected TridionCmdlet()
        {
            Logger = TridionCmdletLogger.Instance();
            TridionCmdletLogger.Initialize(this);
            const int parentActivityId = 1111; //whatever number
            const int childActivityId = 2222;
            _parentProgressRecord = new ProgressRecord(parentActivityId, base.GetType().Name, "Processing...");
            _parentProgressRecord.SecondsRemaining = -1;
            _childProgressRecord = new ProgressRecord(childActivityId, base.GetType().Name + " subtask ", "Subprocessing...");
            _childProgressRecord.ParentActivityId = parentActivityId;
            _childProgressRecord.SecondsRemaining = -1;
        }


        protected override void BeginProcessing()
        {
            WriteDebug("BeginProcessing");
            _tickCountStart = Environment.TickCount;
            base.BeginProcessing();
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
            WriteDebug($"EndProcessing  elapsed:{(Environment.TickCount - _tickCountStart)}ms");
        }

        /// <summary>
        /// Write progress over the complete cmdlet, so over updates and retrieves
        /// </summary>
        /// <param name="message">What are you processing</param>
        /// <param name="current">Current step number</param>
        /// <param name="total">Total number of steps</param>
        public void WriteParentProgress(string message, int current, int total)
        {
            try
            {
                _parentTotal = total;
                if (current <= total)
                {
                    _parentCurrent = current;
                }
                else
                {
                    WriteDebug($"WriteParentProgress Corrected to 100% progress for incoming message[{message}] current[{current}] total[{total}]");
                    _parentCurrent = total;
                }
                _parentProgressRecord.PercentComplete = _parentCurrent * 100 / _parentTotal;
                _parentProgressRecord.StatusDescription = $"{message}  ({_parentCurrent}/{_parentTotal})";
                base.WriteProgress(_parentProgressRecord);
#if DEBUG
                Thread.Sleep(SleepTime);
#endif
            }
            catch (Exception exception)
            {
                WriteWarning(exception.Message);
            }
        }

        /// <summary>
        /// Write progress over the specific (webservice) calls required to process a list of ids/tcmobjects
        /// </summary>
        /// <param name="message">What are you processing</param>
        /// <param name="current">Current step number</param>
        /// <param name="total">Total number of steps</param>
        public void WriteChildProgress(string message, int current, int total)
        {
            try
            {
                // Updating progress bar of the child
                _childProgressRecord.PercentComplete = current * 100 / total;
                _childProgressRecord.StatusDescription = message;
                WriteVerbose(message);
                // Skipping child write progress when current is '1', so only a progress when there is more to do
                // Alternative could be if current==total to avoid a lot progress records to scroll by
                if (current > 1)
                {
                    base.WriteProgress(_childProgressRecord);
                }
                // Updating progress bar of the parent
                _parentProgressRecord.PercentComplete = (int)((_parentCurrent * 100 / _parentTotal) + ((current * 100 / total) * (1.0 / _parentTotal)));
                base.WriteProgress(_parentProgressRecord);
#if DEBUG
                Thread.Sleep(SleepTime);
#endif
            }
            catch (Exception exception)
            {
                WriteWarning(exception.Message);
            }
        }

        /// <summary>
        /// Intentional override to include cmdlet origin
        /// </summary>
        public new void WriteVerbose(string message)
        {
            base.WriteVerbose(base.GetType().Name + "  " + message);
            WriteDebug(message);
        }

        /// <summary>
        /// Intentional override to include cmdlet origin and timestamp
        /// </summary>
        public new void WriteDebug(string message)
        {
            base.WriteDebug($"{base.GetType().Name} {DateTime.Now.ToString("yyyyMMdd.HHmmss.fff")} {message}");
        }

        /// <summary>
        /// Intentional override to include cmdlet origin 
        /// </summary>
        public new void WriteWarning(string message)
        {
            base.WriteWarning(base.GetType().Name + "  " + message);
        }

        /// <summary>
        /// Divides one list to multiple lists by batchsize
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="list">List to devide</param>
        /// <param name="batchSize"></param>
        /// <returns>Multiple lists, all having maximally batchsize elements</returns>
        internal List<List<T>> DivideListInBatches<T>(List<T> list, int batchSize)
        {
            var outList = new List<List<T>>();
            if (list != null)
            {
                for (int i = 0; i < list.Count; i += batchSize)
                {
                    outList.Add(list.Skip(i).Take(batchSize).ToList<T>());
                }
            }
            return outList;
        }
    }
}
