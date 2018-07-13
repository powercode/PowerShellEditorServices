//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System;
using System.Management.Automation.Runspaces;
using System.Reflection;

namespace Microsoft.PowerShell.EditorServices.Session
{
    internal class PowerShell5Operations : PowerShell4Operations
    {
        public PowerShell5Operations()
        {
            var asm = LoadVisitorAssembly();
            var factoryType = asm.GetType("Microsoft.PowerShell.EditorServices.Language5.AstVisitorFactory");
            var ctor = factoryType.GetConstructor(new Type[0]);
            var factoryInstance = ctor.Invoke(new object[0]);
            AstOperations.AstVisitorFactory = (IAstVisitorFactory) factoryInstance;
        }
        public override void PauseDebugger(Runspace runspace)
        {
#if !PowerShellv3 && !PowerShellv4
            runspace.Debugger?.SetDebuggerStepMode(true);
#endif
        }

        Assembly LoadVisitorAssembly()
        {
            var thisAssembly = typeof(PowerShell5Operations).GetTypeInfo().Assembly;
            var codeBaseUrl = new Uri(thisAssembly.CodeBase);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var dirPath = System.IO.Path.GetDirectoryName(codeBasePath);

            var assemblyPath = System.IO.Path.Combine(dirPath, "Microsoft.PowerShell.EditorServices.AstVisitors.dll");
#if CoreCLR
            return System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
#else
            return Assembly.LoadFile(assemblyPath);
#endif

        }
    }
}

