using System;
using System.Linq;
using System.Management.Automation.Language;

namespace Microsoft.PowerShell.EditorServices
{
    public class MethodSymbolReference : SymbolReference
    {
        internal static readonly string[] Empty = new string[0];
        public MethodSymbolReference(FunctionMemberAst ast) :
            base(
                ast.IsConstructor ? SymbolType.Constructor : SymbolType.Method,
                ast.Name,
                GetNameExtent(ast))
        {
            MemberOfType = ast.GetMemberClassName();
            Signature = GetSignatureString(ast);
            ParameterTypes = ast.Parameters?.Select(p => p.StaticType.Name).ToArray() ?? Empty;
            SignatureName = GetSignatureName(ast);
            ReturnType = ast.ReturnType?.TypeName.Name ?? String.Empty;
            IsConstructor = ast.IsConstructor;
            IsStatic = ast.IsStatic;
        }

        public bool IsStatic { get; set; }
        public bool IsConstructor { get; set; }

        public string SignatureName { get; set; }

        public string ReturnType { get; set; }

        public string MemberOfType { get; set; }

        public string Signature { get; set; }

        public string[] ParameterTypes { get; set; }

        protected override string GetDisplayString() => SignatureName;

        static IScriptExtent GetNameExtent(FunctionMemberAst functionMemberAst)
        {
            var scriptStartColumnNumber = functionMemberAst.Extent.StartScriptPosition.ColumnNumber;
            var startColumnNumber = functionMemberAst.Extent.Text.IndexOf(functionMemberAst.Name, StringComparison.CurrentCulture);
            IScriptExtent nameExtent = new ScriptExtent
            {
                Text = functionMemberAst.Name,
                StartLineNumber = functionMemberAst.Extent.StartLineNumber,
                EndLineNumber = functionMemberAst.Extent.EndLineNumber,
                StartColumnNumber = scriptStartColumnNumber + startColumnNumber,
                EndColumnNumber = scriptStartColumnNumber + functionMemberAst.Name.Length
            };
            return nameExtent;
        }

        static string GetSignatureName(FunctionMemberAst ast) => $"{ast.Name}({GetSignatureString(ast)})";

        static string GetSignatureString(FunctionMemberAst ast) => string.Join(", ", ast.Parameters.Select(p => p.StaticType.Name));
    }
}
