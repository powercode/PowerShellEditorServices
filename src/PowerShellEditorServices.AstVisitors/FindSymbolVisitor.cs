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
    /// The visitor used to find the the symbol at a specfic location in the AST
    /// </summary>
    public class FindSymbolVisitor : AstVisitor2
    {
        private readonly int _lineNumber;
        private readonly int _columnNumber;
        private readonly bool _includeFunctionDefinitions;

        private readonly List<SymbolReference> _result;

        public FindSymbolVisitor(
            int lineNumber,
            int columnNumber,
            bool includeFunctionDefinitions, List<SymbolReference> result)
        {
            _lineNumber = lineNumber;
            _columnNumber = columnNumber;
            _includeFunctionDefinitions = includeFunctionDefinitions;
            _result = result;
        }

        void AddResult(SymbolReference symRef)
        {
            _result.Add(symRef);
        }

        /// <summary>
        /// Checks to see if this command ast is the symbol we are looking for.
        /// </summary>
        /// <param name="commandAst">A CommandAst object in the script's AST</param>
        /// <returns>A descion to stop searching if the right symbol was found,
        /// or a decision to continue if it wasn't found</returns>
        public override AstVisitAction VisitCommand(CommandAst commandAst)
        {
            Ast commandNameAst = commandAst.CommandElements[0];

            if (IsPositionInExtent(commandNameAst.Extent))
            {
                var res =
                    new SymbolReference(SymbolType.Function,
                        commandNameAst.Extent);
                AddResult(res);
                return AstVisitAction.StopVisit;
            }

            return base.VisitCommand(commandAst);
        }

        /// <summary>
        /// Checks to see if this function definition is the symbol we are looking for.
        /// </summary>
        /// <param name="functionDefinitionAst">A functionDefinitionAst object in the script's AST</param>
        /// <returns>A descion to stop searching if the right symbol was found,
        /// or a decision to continue if it wasn't found</returns>
        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            int startColumnNumber = 1;

            if (!_includeFunctionDefinitions)
            {
                startColumnNumber = functionDefinitionAst.Extent.Text.IndexOf(functionDefinitionAst.Name,
                                        StringComparison.CurrentCultureIgnoreCase) + 1;
            }

            IScriptExtent nameExtent = new ScriptExtent
            {
                Text = functionDefinitionAst.Name,
                StartLineNumber = functionDefinitionAst.Extent.StartLineNumber,
                EndLineNumber = functionDefinitionAst.Extent.StartLineNumber,
                StartColumnNumber = startColumnNumber,
                EndColumnNumber = startColumnNumber + functionDefinitionAst.Name.Length
            };

            if (IsPositionInExtent(nameExtent))
            {
                var res =
                    new SymbolReference(SymbolType.Function,
                        nameExtent);
                AddResult(res);
                return AstVisitAction.StopVisit;
            }

            return AstVisitAction.Continue;
        }

        /// <summary>
        /// Checks to see if this command parameter is the symbol we are looking for.
        /// </summary>
        /// <param name="commandParameterAst">A CommandParameterAst object in the script's AST</param>
        /// <returns>A descion to stop searching if the right symbol was found,
        /// or a decision to continue if it wasn't found</returns>
        public override AstVisitAction VisitCommandParameter(CommandParameterAst commandParameterAst)
        {
            if (IsPositionInExtent(commandParameterAst.Extent))
            {
                var res = new SymbolReference(SymbolType.Parameter, commandParameterAst.Extent);
                AddResult(res);
                return AstVisitAction.StopVisit;
            }

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
            if (IsPositionInExtent(variableExpressionAst.Extent))
            {
                var res = new SymbolReference(SymbolType.Variable, variableExpressionAst.Extent);
                AddResult(res);
                return AstVisitAction.StopVisit;
            }

            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitTypeDefinition(TypeDefinitionAst typeDefinitionAst)
        {
            if (IsPositionInExtent(typeDefinitionAst.Extent))
            {
                var res = new ClassDefinitionSymbolReference(typeDefinitionAst);
                AddResult(res);
                return AstVisitAction.StopVisit;
            }

            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitTypeExpression(TypeExpressionAst typeExpressionAst)
        {
            if (IsPositionInExtent(typeExpressionAst.Extent))
            {
                var res = new ClassSymbolReference(typeExpressionAst);
                AddResult(res);
                return AstVisitAction.StopVisit;
            }

            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitPropertyMember(PropertyMemberAst propertyMemberAst)
        {
            if (IsPositionInExtent(propertyMemberAst.Extent))
            {
                var res = new PropertySymbolReference(propertyMemberAst);
                AddResult(res);
                return AstVisitAction.StopVisit;
            }

            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitFunctionMember(FunctionMemberAst functionMemberAst)
        {
            if (IsPositionInExtent(functionMemberAst.Extent) && !IsPositionInExtent(functionMemberAst.Body.Extent))
            {
                var res = new MethodSymbolReference(functionMemberAst);
                AddResult(res);
                return AstVisitAction.StopVisit;
            }

            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitMemberExpression(MemberExpressionAst memberExpressionAst)
        {
            if (!IsPositionInExtent(memberExpressionAst.Member.Extent))
            {
                return AstVisitAction.Continue;
            }

            var res = new PropertyCallReference(memberExpressionAst);
            AddResult(res);

            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitInvokeMemberExpression(InvokeMemberExpressionAst methodCallAst)
        {
            if (!IsPositionInExtent(methodCallAst.Extent))
            {
                return AstVisitAction.Continue;
            }

            if (IsPositionInExtent(methodCallAst.Member.Extent))
            {
                var res = new MethodCallReference(methodCallAst);
                AddResult(res);
            }
            else if (IsPositionInExtent(methodCallAst.Expression.Extent) &&
                     methodCallAst.Expression is TypeExpressionAst typeExpressionAst)
            {
                var res = new ClassSymbolReference(typeExpressionAst);
                AddResult(res);
            }

            return AstVisitAction.Continue;
        }

        /// <summary>
        /// Is the position of the given location is in the ast's extent
        /// </summary>
        /// <param name="extent">The script extent of the element</param>
        /// <returns>True if the given position is in the range of the element's extent </returns>
        private bool IsPositionInExtent(IScriptExtent extent)
        {
            return extent.StartLineNumber == _lineNumber &&
                   extent.StartColumnNumber <= _columnNumber &&
                   (extent.EndLineNumber > _lineNumber ||
                    extent.EndColumnNumber >= _columnNumber);
        }
    }
}
