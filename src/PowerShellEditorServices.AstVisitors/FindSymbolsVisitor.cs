//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System;
using System.Collections.Generic;
using System.Management.Automation.Language;

namespace Microsoft.PowerShell.EditorServices.Language5
{
    /// <summary>
    /// The visitor used to find all the symbols (function and class defs) in the AST.
    /// </summary>
    /// <remarks>
    /// Requires PowerShell v3 or higher
    /// </remarks>
    public class FindSymbolsVisitor : AstVisitor2
    {
        private readonly List<SymbolReference> _results;

        public FindSymbolsVisitor(List<SymbolReference> result)
        {
            _results= result;
        }

        void AddResult(SymbolReference symref)
        {
            _results.Add(symref);
        }

        /// <summary>
        /// Adds each function defintion as a
        /// </summary>
        /// <param name="functionDefinitionAst">A functionDefinitionAst object in the script's AST</param>
        /// <returns>A decision to stop searching if the right symbol was found,
        /// or a decision to continue if it wasn't found</returns>
        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            if (functionDefinitionAst.Parent is FunctionMemberAst)
            {
                return AstVisitAction.Continue;
            }
            IScriptExtent nameExtent = new ScriptExtent
            {
                Text = functionDefinitionAst.Name,
                StartLineNumber = functionDefinitionAst.Extent.StartLineNumber,
                EndLineNumber = functionDefinitionAst.Extent.EndLineNumber,
                StartColumnNumber = functionDefinitionAst.Extent.StartColumnNumber,
                EndColumnNumber = functionDefinitionAst.Extent.EndColumnNumber
            };

            SymbolType symbolType = functionDefinitionAst.IsWorkflow ?
                  SymbolType.Workflow : SymbolType.Function;

            var symbolReference = new SymbolReference(symbolType,nameExtent);
            AddResult(symbolReference);
            return AstVisitAction.Continue;
        }

        /// <summary>
        ///  Checks to see if this variable expression is the symbol we are looking for.
        /// </summary>
        /// <param name="variableExpressionAst">A VariableExpressionAst object in the script's AST</param>
        /// <returns>A descion to stop searching if the right symbol was found,
        /// or a decision to continue if it wasn't found</returns>
        public override AstVisitAction VisitVariableExpression(VariableExpressionAst variableExpressionAst)
        {
            if (!IsAssignedAtScriptScope(variableExpressionAst))
            {
                return AstVisitAction.Continue;
            }

            AddResult(new SymbolReference(SymbolType.Variable,variableExpressionAst.Extent));
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitTypeDefinition(TypeDefinitionAst typeDefinitionAst)
        {
            AddResult(new ClassDefinitionSymbolReference(typeDefinitionAst));
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitFunctionMember(FunctionMemberAst functionMemberAst)
        {
            AddResult(new MethodSymbolReference(functionMemberAst));

            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitPropertyMember(PropertyMemberAst propertyMemberAst)
        {
            AddResult(new PropertySymbolReference(propertyMemberAst));

            return AstVisitAction.Continue;
        }

        private bool IsAssignedAtScriptScope(VariableExpressionAst variableExpressionAst)
        {
            Ast parent = variableExpressionAst.Parent;
            if (!(parent is AssignmentStatementAst))
            {
                return false;
            }

            parent = parent.Parent;
            if (parent?.Parent?.Parent == null)
            {
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Visitor to find all the keys in Hashtable AST
    /// </summary>
    internal class FindHashtableSymbolsVisitor : AstVisitor
    {
        /// <summary>
        /// List of symbols (keys) found in the hashtable
        /// </summary>
        public List<SymbolReference> SymbolReferences { get; }

        /// <summary>
        /// Initializes a new instance of FindHashtableSymbolsVisitor class
        /// </summary>
        public FindHashtableSymbolsVisitor()
        {
            SymbolReferences = new List<SymbolReference>();
        }

        /// <summary>
        /// Adds keys in the input hashtable to the symbol reference
        /// </summary>
        public override AstVisitAction VisitHashtable(HashtableAst hashtableAst)
        {
            if (hashtableAst.KeyValuePairs == null)
            {
                return AstVisitAction.Continue;
            }

            foreach (var kvp in hashtableAst.KeyValuePairs)
            {
                if (kvp.Item1 is StringConstantExpressionAst keyStrConstExprAst)
                {
                    IScriptExtent nameExtent = new ScriptExtent()
                    {
                        Text = keyStrConstExprAst.Value,
                        StartLineNumber = kvp.Item1.Extent.StartLineNumber,
                        EndLineNumber = kvp.Item2.Extent.EndLineNumber,
                        StartColumnNumber = kvp.Item1.Extent.StartColumnNumber,
                        EndColumnNumber = kvp.Item2.Extent.EndColumnNumber
                    };

                    SymbolType symbolType = SymbolType.HashtableKey;

                    SymbolReferences.Add(new SymbolReference(symbolType,nameExtent));

                }
            }

            return AstVisitAction.Continue;
        }
    }
}
