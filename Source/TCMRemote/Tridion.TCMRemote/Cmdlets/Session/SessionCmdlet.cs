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

using Tridion.TCMRemote.HelperClasses;

namespace Tridion.TCMRemote.Cmdlets.Session
{
    /// <summary>
    /// Abstract class used for the session commandlets.
    /// </summary>
    /// <remarks>Inherits from <see cref="TridionCmdlet"/>.</remarks>
    public abstract class SessionCmdlet : TridionCmdlet
    {
        /// <summary>
        /// Solves the PS51/NET48 problem of OidcClient which continuously threw 
        /// 'Error connecting to https://sts.windows.net/{tenant}/.well-known/openid-configuration. 
        /// Operation is not valid due to the current state of the object' in GetDiscoveryDocumentAsync
        /// as described on https://github.com/IdentityModel/Documentation/issues/13
        /// <see cref="AppDomainModuleAssemblyInitializer"/>
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
#if NET48
            WriteVerbose("TCMRemote module on PS5.1/NET48 forces Assembly Redirects for System.Runtime.CompilerServices.Unsafe, System.Text.Json, Duende.IdentityModel.OidcClient, Microsoft.Bcl.AsyncInterfaces, System.Text.Encodings.Web, System.Memory, System.ComponentModel.Annotations");
#else
            WriteVerbose("TCMRemote module on PS7+/NET forces Assembly Redirects for Duende.IdentityModel and Duende.IdentityModel.OidcClient");
#endif
            //AppDomainAssemblyResolveHelper.Redirect(); is superseded with AppDomainModuleAssemblyInitializer based on IModuleAssemblyInitializer
        }

    }
}
