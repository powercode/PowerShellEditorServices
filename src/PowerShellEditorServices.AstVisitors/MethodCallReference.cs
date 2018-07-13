using System;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.PowerShell.EditorServices.Language5;

namespace Microsoft.PowerShell.EditorServices
{
    public class MethodCallReference : SymbolReference
    {
        public MethodCallReference(InvokeMemberExpressionAst methodCallAst) : base(GetSymbolType(methodCallAst), GetSymbolName(methodCallAst), GetNameExtent(methodCallAst))
        {
            var className = methodCallAst.GetMemberClassName();
            MemberOfType = className;
            Arguments = methodCallAst.Arguments?.Select(e => e.StaticType.ToString()).ToArray() ?? MethodSymbolReference.Empty;
            IsStatic = methodCallAst.Static;
        }

        public bool IsStatic { get; }
        public bool IsConstructor => SymbolType == SymbolType.Constructor;

        public string[] Arguments { get; set; }

        public string MemberOfType { get; set; }

        private static SymbolType GetSymbolType(InvokeMemberExpressionAst invokeMemberExpressionAst) => IsAstAConstructor(invokeMemberExpressionAst) ? SymbolType.Constructor : SymbolType.Method;

        private static IScriptExtent GetNameExtent(InvokeMemberExpressionAst invokeMemberExpressionAst)
        {
            var name = invokeMemberExpressionAst.GetMemberName();
            var scriptStartColumnNumber = invokeMemberExpressionAst.Extent.StartScriptPosition.ColumnNumber;
            var startColumnNumber = invokeMemberExpressionAst.Extent.Text.IndexOf(name, StringComparison.CurrentCulture);
            IScriptExtent nameExtent = new ScriptExtent
            {
                Text = invokeMemberExpressionAst.Member.Extent.Text,
                StartLineNumber = invokeMemberExpressionAst.Extent.StartLineNumber,
                EndLineNumber = invokeMemberExpressionAst.Extent.EndLineNumber,
                StartColumnNumber = scriptStartColumnNumber + startColumnNumber,
                EndColumnNumber = scriptStartColumnNumber + startColumnNumber + name.Length
            };
            return nameExtent;
        }

        private static string GetSymbolName(InvokeMemberExpressionAst ast)
        {
            if (ast.Member is StringConstantExpressionAst mem
                && ast.Expression is TypeExpressionAst typeExpressionAst
                && mem.Value == "new")
            {
                return typeExpressionAst.TypeName.Name;
            }

            return ast.Member.Extent.Text;
        }

        private static bool IsAstAConstructor(InvokeMemberExpressionAst methodCallAst) =>
            methodCallAst.Expression is TypeExpressionAst
            && methodCallAst.Member is StringConstantExpressionAst sce
            && sce.Value.PSEquals("new");
    }
}
