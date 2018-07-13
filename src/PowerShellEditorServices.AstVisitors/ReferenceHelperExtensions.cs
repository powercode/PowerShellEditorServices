using System;
using System.Management.Automation.Language;

namespace Microsoft.PowerShell.EditorServices.Language5
{
    static class ReferenceHelperExtensions
    {
        public static bool IsReferencing(this ClassSymbolReference classToFind, TypeDefinitionAst typeDefinitionAst) => classToFind.SymbolName.PSEquals(typeDefinitionAst.Name);
        public static bool IsReferencing(this ClassSymbolReference classToFind, TypeExpressionAst typeExpressionAst) => classToFind.SymbolName.PSEquals(typeExpressionAst.TypeName.Name);
        public static bool IsReferencing(this ClassDefinitionSymbolReference classToFind, TypeDefinitionAst typeDefinitionAst) => classToFind.SymbolName.PSEquals(typeDefinitionAst.Name);
        public static bool IsReferencing(this ClassDefinitionSymbolReference classToFind, TypeExpressionAst typeExpressionAst) => classToFind.SymbolName.PSEquals(typeExpressionAst.TypeName.Name);

        public static bool IsReferencing(this MethodSymbolReference symRef, FunctionMemberAst functionMemberAst)
        {
            if (functionMemberAst.IsConstructor && symRef.IsConstructor
                && symRef.ParameterTypes.Length == functionMemberAst.ParameterCount()
                && functionMemberAst.IsMemberOfType(symRef.MemberOfType))
            {
                return true;
            }

            return symRef.IsStatic == functionMemberAst.IsStatic
                   && EqualsIgnoreCase(symRef.SymbolName, functionMemberAst.Name)
                   && symRef.ParameterTypes.Length == functionMemberAst.ParameterCount()
                   && (symRef.MemberOfType == "object" || functionMemberAst.IsMemberOfType(symRef.MemberOfType));
        }

        public static bool IsReferencing(this MethodSymbolReference symRef, InvokeMemberExpressionAst methodCallAst)
        {
            return symRef.IsStatic == methodCallAst.Static
                   && EqualsIgnoreCase(symRef.SymbolName, methodCallAst.GetMemberName())
                   && symRef.ParameterTypes.Length == methodCallAst.ArgumentCount()
                   && (symRef.MemberOfType == "object"
                       || methodCallAst.Parent is TypeDefinitionAst tda && EqualsIgnoreCase(symRef.MemberOfType, tda.Name));
        }

        public static bool IsReferencing(this MethodCallReference symRef, InvokeMemberExpressionAst methodCallAst)
        {
            return symRef.IsStatic == methodCallAst.Static
                   && EqualsIgnoreCase(symRef.SymbolName, methodCallAst.GetMemberName())
                   && symRef.Arguments.Length == methodCallAst.ArgumentCount()
                   && (symRef.MemberOfType == "object" || methodCallAst.IsMemberOfType(symRef.MemberOfType));
        }
        public static bool IsReferencing(this MethodCallReference symRef, FunctionMemberAst functionMemberAst)
        {
            if (functionMemberAst.IsConstructor && symRef.IsConstructor
                && symRef.Arguments.Length == functionMemberAst.ParameterCount()
                && functionMemberAst.IsMemberOfType(symRef.MemberOfType))
            {
                return true;
            }

            return symRef.IsStatic == functionMemberAst.IsStatic
                   && EqualsIgnoreCase(symRef.SymbolName, functionMemberAst.Name)
                   && symRef.Arguments.Length == functionMemberAst.ParameterCount()
                   && (symRef.MemberOfType == "object" || functionMemberAst.IsMemberOfType(symRef.MemberOfType));
        }

        public static bool IsReferencing(this PropertyCallReference symRef, PropertyMemberAst propertyMemberAst)
        {
            return symRef.IsStatic == propertyMemberAst.IsStatic
                   && EqualsIgnoreCase(symRef.SymbolName, propertyMemberAst.Name)
                   && (symRef.MemberOfType == "object"
                       || propertyMemberAst.IsMemberOfType(symRef.MemberOfType));
        }

        public static bool IsReferencing(this PropertyCallReference symRef, MemberExpressionAst propertyCallAst)
        {
            return symRef.IsStatic == propertyCallAst.Static
                   && EqualsIgnoreCase(symRef.SymbolName, propertyCallAst.GetMemberName())
                   && (symRef.MemberOfType == "object"
                       || propertyCallAst.IsMemberOfType(symRef.MemberOfType));
        }

        public static bool IsReferencing(this PropertySymbolReference symRef, PropertyMemberAst propertyMemberAst)
        {
            return symRef.IsStatic == propertyMemberAst.IsStatic
                   && EqualsIgnoreCase(symRef.SymbolName, propertyMemberAst.Name)
                   && (symRef.MemberOfType == "object"
                       || propertyMemberAst.IsMemberOfType(symRef.MemberOfType));
        }
        public static bool IsReferencing(this PropertySymbolReference symRef, MemberExpressionAst propertyCallAst)
        {
            string propertyName = null;
            if (propertyCallAst.Member is StringConstantExpressionAst sce)
            {
                propertyName = sce.Value;
            }
            return EqualsIgnoreCase(symRef.SymbolName, propertyName)
                   && (symRef.MemberOfType == "object"
                       || propertyCallAst.IsMemberOfType(symRef.MemberOfType));
        }

        static bool EqualsIgnoreCase(string a, string b) {  return string.Compare(a, b, StringComparison.CurrentCultureIgnoreCase) == 0; }

        public static bool PSEquals(this string a, string b) => EqualsIgnoreCase(a, b);
        static int ArgumentCount(this InvokeMemberExpressionAst ast) => ast.Arguments?.Count ?? 0;
        static int ParameterCount(this FunctionMemberAst ast) => ast.Parameters?.Count ?? 0;

        static bool IsMemberOfType(this FunctionMemberAst functionMemberAst, string typeName) => functionMemberAst.GetMemberClassName().PSEquals(typeName);
        static bool IsMemberOfType(this InvokeMemberExpressionAst methodCallAst, string typeName) => methodCallAst.GetMemberClassName().PSEquals(typeName);
        static bool IsMemberOfType(this MemberExpressionAst methodCallAst, string typeName) => methodCallAst.GetMemberClassName().PSEquals(typeName);
        static bool IsMemberOfType(this PropertyMemberAst propertyMemberAst, string typeName) => propertyMemberAst.GetMemberClassName().PSEquals(typeName);

    }


}
