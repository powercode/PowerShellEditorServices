//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System;
using System.Collections.Generic;
using System.Management.Automation.Language;

namespace Microsoft.PowerShell.EditorServices.Language5
{
    public class AstVisitorFactory : IAstVisitorFactory
    {
        public AstVisitor FindReferencesVisitor(SymbolReference symbolReference, Dictionary<string, List<string>> cmdletToAliasDictionary,
            Dictionary<string, string> aliasToCmdletDictionary, List<SymbolReference> result)
        {
            return new FindReferencesVisitor(symbolReference, cmdletToAliasDictionary, aliasToCmdletDictionary, result);
        }

        public AstVisitor FindSymbolsVisitor(List<SymbolReference> result)
        {
            return new FindSymbolsVisitor(result);
        }

        public AstVisitor FindSymbolVisitor(int lineNumber, int columnNumber, bool includeFunctionDefinitions, List<SymbolReference> result)
        {
            return new FindSymbolVisitor(lineNumber, columnNumber, includeFunctionDefinitions, result);
        }

        public AstVisitor FindCommandVisitor(int lineNumber, int columnNumber, List<SymbolReference> result)
        {
            return new FindCommandVisitor(lineNumber, columnNumber, result);
        }

        public AstVisitor FindReferencesVisitor(SymbolReference symbolReference, List<SymbolReference> result)
        {
            return new FindReferencesVisitor(symbolReference,result);
        }

        public AstVisitor FindDeclarationVisitor(SymbolReference symbolReference, List<SymbolReference> result)
        {
            return new FindDeclarationVisitor(symbolReference, result);
        }
    }
}

