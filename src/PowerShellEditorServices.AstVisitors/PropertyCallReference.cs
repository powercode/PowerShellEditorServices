using System.Linq;
using System.Management.Automation.Language;

namespace Microsoft.PowerShell.EditorServices
{
    public class PropertyCallReference : SymbolReference
    {
        public PropertyCallReference(MemberExpressionAst ast) : base(SymbolType.Property, ast.GetMemberName(),
            GetNameExtent(ast))
        {
            var className = ast.GetMemberClassName();
            if (className == "object") {
              className = TypeInferer.InferTypeOf(ast.Expression).FirstOrDefault()?.Name;
            }
            MemberOfType = className;
            ReturnType = string.Empty;
            IsStatic = ast.Static;
        }

        public bool IsStatic { get; set; }

        public string ReturnType { get; }

        public string MemberOfType { get; }

        static IScriptExtent GetNameExtent(MemberExpressionAst memberExpressionAst)
        {
            var memberExtent = memberExpressionAst.Member.Extent;
            IScriptExtent nameExtent = new ScriptExtent
            {
                Text = memberExtent.Text,
                StartLineNumber = memberExtent.StartLineNumber,
                EndLineNumber = memberExtent.EndLineNumber,
                StartColumnNumber = memberExtent.StartColumnNumber,
                EndColumnNumber = memberExtent.EndColumnNumber
            };
            return nameExtent;
        }


    }
}
