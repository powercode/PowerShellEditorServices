using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Management.Automation.Language;

namespace Microsoft.PowerShell.EditorServices
{
    static class AstHelpers
    {
        internal static string GetMemberClassName(this Ast ast)
        {
            if (ast is MemberExpressionAst memberExpressionAst)
            {
                if (memberExpressionAst.Expression is TypeExpressionAst typeExpressionAst)
                {
                    return typeExpressionAst.TypeName.Name;
                }

                if (memberExpressionAst.Expression.Extent.Text != "$this")
                {
                    return InferTypeOf(memberExpressionAst.Expression);
                }
            }

            var parent = ast.Parent;



            while (parent != null)
            {
                if (parent is TypeDefinitionAst td)
                {
                    return td.Name;
                }
                parent = parent.Parent;
            }

            return string.Empty;
        }

        internal static string InferTypeOf(Ast ast) => TypeInferer.InferTypeOf(ast).FirstOrDefault()?.Name ?? string.Empty;

        internal static string GetMemberName(this Ast ast)
        {
            switch (ast)
            {
                case InvokeMemberExpressionAst ime:
                {
                    if (ime.Member is StringConstantExpressionAst s)
                    {
                        return s.Value;
                    }
                    break;
                }
                case MemberExpressionAst me:
                {
                    if (me.Member is StringConstantExpressionAst s)
                    {
                        return s.Value;
                    }
                    break;
                }
                case PropertyMemberAst pme:
                {
                    return pme.Name;
                }
            }
            return String.Empty;
        }
    }
}
