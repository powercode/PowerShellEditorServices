using System;
using System.Management.Automation.Language;

namespace Microsoft.PowerShell.EditorServices
{
    public class ClassDefinitionSymbolReference : SymbolReference
    {
        public ClassDefinitionSymbolReference(TypeDefinitionAst ast) : base(SymbolType.Class, ast.Name, GetNameExtent(ast))
        {
        }

        private static IScriptExtent GetNameExtent(TypeDefinitionAst typeDefinitionAst)
        {
            var scriptStartColumnNumber = typeDefinitionAst.Extent.StartScriptPosition.ColumnNumber;
            var startColumnNumber = typeDefinitionAst.Extent.Text.IndexOf(typeDefinitionAst.Name, StringComparison.CurrentCulture);
            IScriptExtent nameExtent = new ScriptExtent
            {
                Text = typeDefinitionAst.Name,
                StartLineNumber = typeDefinitionAst.Extent.StartLineNumber,
                EndLineNumber = typeDefinitionAst.Extent.EndLineNumber,
                StartColumnNumber = scriptStartColumnNumber + startColumnNumber,
                EndColumnNumber = scriptStartColumnNumber + typeDefinitionAst.Extent.EndColumnNumber
            };
            return nameExtent;
        }
    }
}
