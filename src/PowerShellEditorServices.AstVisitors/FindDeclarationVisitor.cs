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
    /// The visitor used to find the definition of a symbol
    /// </summary>
    public class FindDeclarationVisitor : AstVisitor2
    {
        private readonly SymbolReference _symbolRef;
        private readonly List<SymbolReference> _result;
        private readonly string _variableName;


        public FindDeclarationVisitor(SymbolReference symbolRef, List<SymbolReference> result)
        {
            _symbolRef = symbolRef;
            _result = result;
            if (_symbolRef.SymbolType == SymbolType.Variable)
            {
                // converts `$varName` to `varName` or of the form ${varName} to varName
                _variableName = symbolRef.SymbolName.TrimStart('$').Trim('{', '}');
            }
        }

        private void AddResult(SymbolReference res)
        {
            _result.Add(res);
        }

        /// <summary>
        /// Decides if the current function definition is the right definition
        /// for the symbol being searched for. The definition of the symbol will be a of type
        /// SymbolType.Function and have the same name as the symbol
        /// </summary>
        /// <param name="functionDefinitionAst">A FunctionDefinitionAst in the script's AST</param>
        /// <returns>A descion to stop searching if the right FunctionDefinitionAst was found,
        /// or a decision to continue if it wasn't found</returns>
        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            // Get the start column number of the function name,
            // instead of the the start column of 'function' and create new extent for the functionName
            int startColumnNumber = functionDefinitionAst.Extent.Text.IndexOf(functionDefinitionAst.Name) + 1;

            IScriptExtent nameExtent = new ScriptExtent()
            {
                Text = functionDefinitionAst.Name,
                StartLineNumber = functionDefinitionAst.Extent.StartLineNumber,
                StartColumnNumber = startColumnNumber,
                EndLineNumber = functionDefinitionAst.Extent.StartLineNumber,
                EndColumnNumber = startColumnNumber + functionDefinitionAst.Name.Length
            };

            if (_symbolRef.SymbolType.Equals(SymbolType.Function) &&
                 nameExtent.Text.Equals(_symbolRef.ScriptRegion.Text, StringComparison.CurrentCultureIgnoreCase))
            {
                AddResult(new SymbolReference(SymbolType.Function,nameExtent));

                return AstVisitAction.StopVisit;
            }

            return base.VisitFunctionDefinition(functionDefinitionAst);
        }

        /// <summary>
        /// Check if the left hand side of an assignmentStatementAst is a VariableExpressionAst
        /// with the same name as that of symbolRef.
        /// </summary>
        /// <param name="assignmentStatementAst">An AssignmentStatementAst</param>
        /// <returns>A decision to stop searching if the right VariableExpressionAst was found,
        /// or a decision to continue if it wasn't found</returns>
        public override AstVisitAction VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst)
        {
            var variableExprAst = assignmentStatementAst.Left as VariableExpressionAst;
            if (variableExprAst == null ||
                _variableName == null ||
                !variableExprAst.VariablePath.UserPath.Equals(
                    _variableName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return AstVisitAction.Continue;
            }

            // TODO also find instances of set-variable

            AddResult(new SymbolReference(SymbolType.Variable, variableExprAst.Extent));
            return AstVisitAction.StopVisit;
        }

        public override AstVisitAction VisitTypeDefinition(TypeDefinitionAst typeDefinitionAst)
        {
            if (_symbolRef.SymbolType != SymbolType.Class)
            {
                return AstVisitAction.Continue;
            }

            if (_symbolRef is ClassSymbolReference csr)
            {
                if (csr.IsReferencing(typeDefinitionAst))
                {
                    AddResult(new ClassDefinitionSymbolReference(typeDefinitionAst));
                    return AstVisitAction.StopVisit;
                }
            }

            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitPropertyMember(PropertyMemberAst propertyMemberAst)
        {
            if (_symbolRef.SymbolType != SymbolType.Property)
            {
                return AstVisitAction.Continue;
            }

            switch (_symbolRef)
            {
                case PropertyCallReference pcrToFind:
                    if (pcrToFind.IsReferencing(propertyMemberAst))
                    {
                        AddResult(new PropertySymbolReference(propertyMemberAst));
                        return AstVisitAction.StopVisit;
                    }
                    break;
                case PropertySymbolReference pcrToFind:
                    if (pcrToFind.IsReferencing(propertyMemberAst))
                    {
                        AddResult(new PropertySymbolReference(propertyMemberAst));
                        return AstVisitAction.StopVisit;
                    }
                    break;
            }

            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitFunctionMember(FunctionMemberAst functionMemberAst)
        {

            if (_symbolRef.SymbolType != SymbolType.Method)
            {
                return AstVisitAction.Continue;
            }

            switch (_symbolRef)
            {
                case MethodCallReference mcrToFind:
                    if (mcrToFind.IsReferencing(functionMemberAst))
                    {
                        AddResult(new MethodSymbolReference(functionMemberAst));
                        return AstVisitAction.StopVisit;
                    }
                    break;
                case MethodSymbolReference msrToFind:
                    if (msrToFind.IsReferencing(functionMemberAst))
                    {
                        AddResult(new MethodSymbolReference(functionMemberAst));
                        return AstVisitAction.StopVisit;
                    }
                    break;
            }

            return AstVisitAction.Continue;
        }
    }
}
