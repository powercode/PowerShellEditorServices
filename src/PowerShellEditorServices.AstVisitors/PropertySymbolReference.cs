using System;
using System.Management.Automation.Language;

namespace Microsoft.PowerShell.EditorServices
{
    public class PropertySymbolReference : SymbolReference
    {
        public PropertySymbolReference(PropertyMemberAst ast) : base(SymbolType.Property, ast.Name, GetNameExtent(ast))
        {
            MemberOfType = ast.GetMemberClassName();
            ReturnType = ast.PropertyType?.TypeName.Name ?? "object";
            IsStatic = ast.IsStatic;
        }
        public  bool IsStatic { get; set; }
        public string ReturnType { get; set; }

        public string MemberOfType { get; set; }

        protected override string GetDisplayString() => $"{ReturnType} {MemberOfType}.{SymbolName}";

        static IScriptExtent GetNameExtent(PropertyMemberAst propertyMemberAst)
        {
            var scriptStartColumnNumber = propertyMemberAst.Extent.StartScriptPosition.ColumnNumber;
            var startColumnNumber = propertyMemberAst.Extent.Text.IndexOf(propertyMemberAst.Name, StringComparison.CurrentCulture);
            IScriptExtent nameExtent = new ScriptExtent
            {
                Text = propertyMemberAst.Name,
                StartLineNumber = propertyMemberAst.Extent.StartLineNumber,
                EndLineNumber = propertyMemberAst.Extent.EndLineNumber,
                StartColumnNumber = scriptStartColumnNumber + startColumnNumber,
                EndColumnNumber = scriptStartColumnNumber + startColumnNumber + propertyMemberAst.Name.Length
            };
            return nameExtent;
        }



    }
}
