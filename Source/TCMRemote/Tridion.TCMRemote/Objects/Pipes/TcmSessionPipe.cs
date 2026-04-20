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

using System.Management.Automation;
using Tridion.TCMRemote.Objects.Public;

namespace Tridion.TCMRemote.Objects.Pipes
{
    public class TcmSessionPipe
    {
        private readonly PSCmdlet _cmdlet;

        public TcmSessionPipe(PSCmdlet cmdlet)
        {
            _cmdlet = cmdlet;
        }

        public TcmSession GetSession()
        {
            // Try to get session from pipeline
            var session = _cmdlet.SessionState.PSVariable.GetValue("TcmSession") as TcmSession;

            if (session == null)
            {
                throw new PSArgumentException(
                    "No TcmSession found. Please create a session using New-TcmSession or specify the -TcmSession parameter.",
                    "TcmSession");
            }

            return session;
        }
    }
}