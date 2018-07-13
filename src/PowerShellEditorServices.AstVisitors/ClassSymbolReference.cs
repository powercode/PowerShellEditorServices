using System;
using System.Management.Automation.Language;

namespace Microsoft.PowerShell.EditorServices
{
    public class ClassSymbolReference : SymbolReference
    {
        public ClassSymbolReference(TypeExpressionAst typeExpressionAst) : base(SymbolType.Class, typeExpressionAst.TypeName.Name, GetNameExtent(typeExpressionAst))
        {
        }

        private static IScriptExtent GetNameExtent(TypeExpressionAst typeExpressionAst)
        {
            var name = typeExpressionAst.TypeName.Name;
            var scriptStartColumnNumber = typeExpressionAst.Extent.StartScriptPosition.ColumnNumber;
            var startColumnNumber = typeExpressionAst.Extent.Text.IndexOf(name, StringComparison.CurrentCulture);
            IScriptExtent nameExtent = new ScriptExtent
            {
                Text = name,
                StartLineNumber = typeExpressionAst.Extent.StartLineNumber,
                EndLineNumber = typeExpressionAst.Extent.EndLineNumber,
                StartColumnNumber = scriptStartColumnNumber + startColumnNumber,
                EndColumnNumber = scriptStartColumnNumber + name.Length + 1
            };
            return nameExtent;
        }
    }
}
