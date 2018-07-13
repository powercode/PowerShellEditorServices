using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Reflection;

namespace Microsoft.PowerShell.EditorServices
{
    static class TypeInferer
    {
        static readonly PSTypeName[] Empty = new PSTypeName[0];
        private static readonly Func<Ast, IList<PSTypeName>> InferenceFunction = GetInferenceFunction();

        private static Func<Ast, IList<PSTypeName>> GetInferenceFunction()
        {
            var astTypeInferenceType = (typeof(Ast).GetTypeInfo().Assembly.GetType("System.Management.Automation.AstTypeInference"));
            if (astTypeInferenceType == null)
            {
                return ast => Empty;
            }

            var inferTypeOf = astTypeInferenceType.GetMethod("InferTypeOf", new[] {typeof(Ast)});
            return (Func<Ast, IList<PSTypeName>>) inferTypeOf.CreateDelegate(typeof(Func<Ast, IList<PSTypeName>>));
        }

        public static IList<PSTypeName> InferTypeOf(Ast ast)
        {
            return new List<PSTypeName>
            {
                new PSTypeName("object")
            };
            //return InferenceFunction.Invoke(ast);
        }
    }
}
