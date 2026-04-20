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

using System.Collections.Concurrent;
using System.IO;
using System.Management.Automation;
using System.Reflection;
#if NET48
using System;
#else
using System.Runtime.Loader;
#endif

namespace Tridion.TCMRemote.HelperClasses
{
    /// <summary>
    /// PROBLEM:
    /// On NET Framework 4.8 / Windows PowerShell 5.1, the GAC and PowerShell's own assembly
    /// resolver can serve older versions of shared assemblies (e.g. System.Text.Json 4.x)
    /// before our newer versions are loaded, causing cryptic MethodMissingException or
    /// TypeLoadException errors inside Duende.IdentityModel.OidcClient.
    ///
    /// SOLUTION:
    /// <see cref="IModuleAssemblyInitializer.OnImport"/> fires early — before any cmdlet runs —
    /// and force-loads the exact assembly versions shipped with the module.  A custom
    /// AssemblyResolve / AssemblyLoadContext.Resolving handler then returns these pre-loaded
    /// assemblies whenever the CLR asks for any version of the same name.
    ///
    /// MAPPINGS (NET48):
    ///   System.Runtime.CompilerServices.Unsafe  4.0.4 / 5.0.0  →  6.0.0 (shipped)
    ///   System.Text.Json                         5.0.0          →  latest (shipped)
    ///   Microsoft.Bcl.AsyncInterfaces            5.0.0          →  6.0.0+ (shipped)
    ///   System.Text.Encodings.Web               (any)           →  latest (shipped)
    ///   System.Memory                           (any)           →  shipped
    ///   System.ComponentModel.Annotations       4.2.0          →  5.0.0 (shipped)
    ///   Duende.IdentityModel                    (any)           →  shipped
    ///   Duende.IdentityModel.OidcClient         (any)           →  shipped
    /// </summary>
    public class AppDomainModuleAssemblyInitializer : IModuleAssemblyInitializer
    {
        private static readonly ConcurrentDictionary<string, Assembly> _forcedLoadedAssemblies =
            new ConcurrentDictionary<string, Assembly>();

        /// <summary>Called automatically when the module binary is imported into PowerShell.</summary>
        public void OnImport()
        {
#if NET48
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly_NetFramework;

            PreloadAssembly("System.Runtime.CompilerServices.Unsafe");
            PreloadAssembly("System.Text.Json");
            PreloadAssembly("Microsoft.Bcl.AsyncInterfaces");
            PreloadAssembly("System.Text.Encodings.Web");
            PreloadAssembly("System.Memory");
            PreloadAssembly("System.ComponentModel.Annotations");
            PreloadAssembly("Duende.IdentityModel");
            PreloadAssembly("Duende.IdentityModel.OidcClient");
#else
            AssemblyLoadContext.Default.Resolving += ResolveAssembly_NetCore;

            PreloadAssembly("Duende.IdentityModel");
            PreloadAssembly("Duende.IdentityModel.OidcClient");
#endif
        }

        // ------------------------------------------------------------------ helpers

        private static void PreloadAssembly(string assemblyFileName)
        {
            var filePath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                assemblyFileName + ".dll");

            if (!File.Exists(filePath)) return;

            var assembly = Assembly.LoadFrom(filePath);
            _forcedLoadedAssemblies.GetOrAdd(assemblyFileName, assembly);
        }

#if NET48
        private static Assembly? ResolveAssembly_NetFramework(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name).Name!;
            _forcedLoadedAssemblies.TryGetValue(name, out var outAssembly);
            return outAssembly;
        }
#else
        private static Assembly? ResolveAssembly_NetCore(AssemblyLoadContext context, AssemblyName assemblyName)
        {
            var name = assemblyName.Name!;
            _forcedLoadedAssemblies.TryGetValue(name, out var outAssembly);
            return outAssembly;
        }
#endif
    }
}
