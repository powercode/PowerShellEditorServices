using System.Collections.Generic;
using System.Management.Automation.Language;

namespace Microsoft.PowerShell.EditorServices
{
    internal class LegacyAstVisitorFactory : IAstVisitorFactory
    {
        public AstVisitor FindReferencesVisitor(SymbolReference symbolReference, Dictionary<string, List<string>> cmdletToAliasDictionary,
            Dictionary<string, string> aliasToCmdletDictionary, List<SymbolReference> foundReferences)
        {
            return new FindReferencesVisitor(symbolReference, cmdletToAliasDictionary, aliasToCmdletDictionary, foundReferences);
        }

        public AstVisitor FindSymbolsVisitor(List<SymbolReference> result)
        {
            return new FindSymbolsVisitor(result);
        }

        public AstVisitor FindSymbolVisitor(int lineNumber, int columnNumber, bool includeFunctionDefinitions, List<SymbolReference> result)
        {
            return new FindSymbolVisitor(lineNumber, columnNumber, includeFunctionDefinitions,  result);
        }

        public AstVisitor FindCommandVisitor(int lineNumber, int columnNumber, List<SymbolReference> result)
        {
            return new FindCommandVisitor(lineNumber, columnNumber, result);
        }

        public AstVisitor FindReferencesVisitor(SymbolReference symbolReference, List<SymbolReference> result)
        {
            return new FindReferencesVisitor(symbolReference, result);
        }

        public AstVisitor FindDeclarationVisitor(SymbolReference symbolReference, List<SymbolReference> result)
        {
            return new FindDeclarationVisitor(symbolReference, result);
        }
    }
}