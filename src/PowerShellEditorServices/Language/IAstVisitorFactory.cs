using System;
using System.Collections.Generic;
using System.Management.Automation.Language;

namespace Microsoft.PowerShell.EditorServices
{
    public interface IAstVisitorFactory
    {
        AstVisitor FindReferencesVisitor(
            SymbolReference symbolReference,
            Dictionary<String, List<String>> cmdletToAliasDictionary,
            Dictionary<String, String> aliasToCmdletDictionary, List<SymbolReference> foundReferences);

        AstVisitor FindSymbolsVisitor(List<SymbolReference> result);
        AstVisitor FindSymbolVisitor(int lineNumber, int columnNumber, bool includeFunctionDefinitions, List<SymbolReference> result);
        AstVisitor FindCommandVisitor(int lineNumber, int columnNumber, List<SymbolReference> result);
        AstVisitor FindReferencesVisitor(SymbolReference symbolReference, List<SymbolReference> result);
        AstVisitor FindDeclarationVisitor(SymbolReference symbolReference, List<SymbolReference> result);
    }
}