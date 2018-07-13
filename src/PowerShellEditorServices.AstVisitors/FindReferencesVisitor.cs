//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using Microsoft.PowerShell.Commands;

namespace Microsoft.PowerShell.EditorServices.Language5
{
    /// <summary>
    /// The visitor used to find the references of a symbol in a script's AST
    /// </summary>
    public class FindReferencesVisitor : AstVisitor2
    {
        private readonly SymbolReference symbolRef;
        private readonly Dictionary<String, List<String>> CmdletToAliasDictionary;
        private readonly Dictionary<String, String> AliasToCmdletDictionary;
        private readonly string symbolRefCommandName;
        private readonly bool needsAliases;

        public List<SymbolReference> Result { get; set; }

        /// <summary>
        /// Constructor used when searching for aliases is needed
        /// </summary>
        /// <param name="symbolReference">The found symbolReference that other symbols are being compared to</param>
        /// <param name="cmdletToAliasDictionary">Dictionary maping cmdlets to aliases for finding alias references</param>
        /// <param name="aliasToCmdletDictionary">Dictionary maping aliases to cmdlets for finding alias references</param>
        public FindReferencesVisitor(
            SymbolReference symbolReference,
            Dictionary<String, List<String>> cmdletToAliasDictionary,
            Dictionary<String, String> aliasToCmdletDictionary, List<SymbolReference> result)
        {
            symbolRef = symbolReference;
            Result = new List<SymbolReference>();
            needsAliases = true;
            CmdletToAliasDictionary = cmdletToAliasDictionary;
            AliasToCmdletDictionary = aliasToCmdletDictionary;
            Result = result;

            // Try to get the symbolReference's command name of an alias,
            // if a command name does not exists (if the symbol isn't an alias to a command)
            // set symbolRefCommandName to and empty string value
            aliasToCmdletDictionary.TryGetValue(symbolReference.ScriptRegion.Text, out symbolRefCommandName);
            if (symbolRefCommandName == null) { symbolRefCommandName = string.Empty; }

        }

        private void AddResult(SymbolReference symRef)
        {
            Result.Add(symRef);
        }


        /// <summary>
        /// Constructor used when searching for aliases is not needed
        /// </summary>
        /// <param name="foundSymbol">The found symbolReference that other symbols are being compared to</param>
        /// <param name="result">the result of the operation.</param>
        public FindReferencesVisitor(SymbolReference foundSymbol, List<SymbolReference> result)
        {
            symbolRef = foundSymbol;
            Result = result;
            needsAliases = false;
        }

        /// <summary>
        /// Decides if the current command is a reference of the symbol being searched for.
        /// A reference of the symbol will be a of type SymbolType.Function
        /// and have the same name as the symbol
        /// </summary>
        /// <param name="commandAst">A CommandAst in the script's AST</param>
        /// <returns>A visit action that continues the search for references</returns>
        public override AstVisitAction VisitCommand(CommandAst commandAst)
        {
            Ast commandNameAst = commandAst.CommandElements[0];
            string commandName = commandNameAst.Extent.Text;

            if(symbolRef.SymbolType.Equals(SymbolType.Function))
            {
                if (needsAliases)
                {
                    // Try to get the commandAst's name and aliases,
                    // if a command does not exists (if the symbol isn't an alias to a command)
                    // set command to and empty string value string command
                    // if the aliases do not exist (if the symvol isn't a command that has aliases)
                    // set aliases to an empty List<string>
                    string command;
                    List<string> alaises;
                    CmdletToAliasDictionary.TryGetValue(commandName, out alaises);
                    AliasToCmdletDictionary.TryGetValue(commandName, out command);
                    if (alaises == null) { alaises = new List<string>(); }
                    if (command == null) { command = string.Empty; }

                    if (symbolRef.SymbolType.Equals(SymbolType.Function))
                    {
                        // Check if the found symbol's name is the same as the commandAst's name OR
                        // if the symbol's name is an alias for this commandAst's name (commandAst is a cmdlet) OR
                        // if the symbol's name is the same as the commandAst's cmdlet name (commandAst is a alias)
                        if (commandName.Equals(symbolRef.SymbolName, StringComparison.CurrentCultureIgnoreCase) ||
                        alaises.Contains(symbolRef.ScriptRegion.Text.ToLower()) ||
                        command.Equals(symbolRef.ScriptRegion.Text, StringComparison.CurrentCultureIgnoreCase) ||
                        (!command.Equals(string.Empty) && command.Equals(symbolRefCommandName, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            AddResult(new SymbolReference(
                                SymbolType.Function,
                                commandNameAst.Extent));
                        }
                    }

                }
                else // search does not include aliases
                {
                    if (commandName.Equals(symbolRef.SymbolName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        AddResult(new SymbolReference(
                            SymbolType.Function,
                            commandNameAst.Extent));
                    }
                }

            }
            return base.VisitCommand(commandAst);
        }

        /// <summary>
        /// Decides if the current function defintion is a reference of the symbol being searched for.
        /// A reference of the symbol will be a of type SymbolType.Function and have the same name as the symbol
        /// </summary>
        /// <param name="functionDefinitionAst">A functionDefinitionAst in the script's AST</param>
        /// <returns>A visit action that continues the search for references</returns>
        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            // Get the start column number of the function name,
            // instead of the the start column of 'function' and create new extent for the functionName
            int startColumnNumber =
                functionDefinitionAst.Extent.Text.IndexOf(functionDefinitionAst.Name, StringComparison.CurrentCultureIgnoreCase) + 1;

            IScriptExtent nameExtent = new ScriptExtent
            {
                Text = functionDefinitionAst.Name,
                StartLineNumber = functionDefinitionAst.Extent.StartLineNumber,
                EndLineNumber = functionDefinitionAst.Extent.StartLineNumber,
                StartColumnNumber = startColumnNumber,
                EndColumnNumber = startColumnNumber + functionDefinitionAst.Name.Length
            };

            if (symbolRef.SymbolType.Equals(SymbolType.Function) &&
                nameExtent.Text.Equals(symbolRef.SymbolName, StringComparison.CurrentCultureIgnoreCase))
            {
                AddResult(new SymbolReference(
                                          SymbolType.Function,
                                          nameExtent));
            }
            return base.VisitFunctionDefinition(functionDefinitionAst);
        }

        /// <summary>
        /// Decides if the current function defintion is a reference of the symbol being searched for.
        /// A reference of the symbol will be a of type SymbolType.Parameter and have the same name as the symbol
        /// </summary>
        /// <param name="commandParameterAst">A commandParameterAst in the script's AST</param>
        /// <returns>A visit action that continues the search for references</returns>
        public override AstVisitAction VisitCommandParameter(CommandParameterAst commandParameterAst)
        {
            if (symbolRef.SymbolType.Equals(SymbolType.Parameter) &&
                commandParameterAst.Extent.Text.Equals(symbolRef.SymbolName, StringComparison.CurrentCultureIgnoreCase))
            {
                AddResult(new SymbolReference(
                                         SymbolType.Parameter,
                                         commandParameterAst.Extent));
            }
            return AstVisitAction.Continue;
        }

        /// <summary>
        /// Decides if the current function defintion is a reference of the symbol being searched for.
        /// A reference of the symbol will be a of type SymbolType.Variable and have the same name as the symbol
        /// </summary>
        /// <param name="variableExpressionAst">A variableExpressionAst in the script's AST</param>
        /// <returns>A visit action that continues the search for references</returns>
        public override AstVisitAction VisitVariableExpression(VariableExpressionAst variableExpressionAst)
        {
            if(symbolRef.SymbolType.Equals(SymbolType.Variable) &&
                variableExpressionAst.Extent.Text.Equals(symbolRef.SymbolName, StringComparison.CurrentCultureIgnoreCase))
            {
                AddResult(new SymbolReference(
                                         SymbolType.Variable,
                                         variableExpressionAst.Extent));
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitTypeExpression(TypeExpressionAst typeExpressionAst)
        {
            if (symbolRef.SymbolType != SymbolType.Class)
            {
                return AstVisitAction.Continue;
            }

            if (string.Compare(symbolRef.SymbolName, typeExpressionAst.TypeName.Name,
                    StringComparison.CurrentCultureIgnoreCase) != 0)
            {
                return AstVisitAction.Continue;
            }

            switch (symbolRef)
            {
                case ClassDefinitionSymbolReference classDefToFind:
                {
                    if (classDefToFind.IsReferencing(typeExpressionAst))
                    {
                        AddResult(new ClassSymbolReference(typeExpressionAst));
                    }
                    break;
                }
                case ClassSymbolReference classRefToFind:
                {
                    if (classRefToFind.IsReferencing(typeExpressionAst))
                    {
                        AddResult(new ClassSymbolReference(typeExpressionAst));
                    }
                    break;
                }
            }

            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitMemberExpression(MemberExpressionAst memberExpressionAst)
        {
            if (symbolRef.SymbolType != SymbolType.Property)
            {
                return AstVisitAction.Continue;
            }

            switch (symbolRef)
            {
                case PropertyCallReference pcrToFind:
                    if (pcrToFind.IsReferencing(memberExpressionAst))
                    {
                        AddResult(new PropertyCallReference(memberExpressionAst));
                    }
                    break;
                case PropertySymbolReference psrToFind:
                    if (psrToFind.IsReferencing(memberExpressionAst))
                    {
                        AddResult(new PropertyCallReference(memberExpressionAst));
                    }
                    break;
            }

            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitInvokeMemberExpression(InvokeMemberExpressionAst methodCallAst)
        {
            if (symbolRef.SymbolType != SymbolType.Method
                && symbolRef.SymbolType != SymbolType.Constructor
                && symbolRef.SymbolType != SymbolType.Class )
            {
                return AstVisitAction.Continue;
            }

            switch (symbolRef)
            {
                case MethodSymbolReference msrToFind:
                    if (msrToFind.IsReferencing(methodCallAst))
                    {
                        AddResult(new MethodCallReference(methodCallAst));
                    }
                    break;
                case MethodCallReference mcrToFind:
                    if (mcrToFind.IsReferencing(methodCallAst))
                    {
                        AddResult(new MethodCallReference(methodCallAst));
                    }
                    break;
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitPropertyMember(PropertyMemberAst propertyMemberAst)
        {
            if (symbolRef.SymbolType != SymbolType.Property)
            {
                return AstVisitAction.Continue;
            }

            if (string.Compare(symbolRef.SymbolName, propertyMemberAst.Name,StringComparison.CurrentCultureIgnoreCase) != 0)
            {
                return AstVisitAction.Continue;
            }

            switch (symbolRef)
            {
                case PropertyCallReference pcrToFind:
                    if (pcrToFind.IsReferencing(propertyMemberAst))
                    {
                        AddResult(new PropertySymbolReference(propertyMemberAst));
                    }
                    break;
                case PropertySymbolReference pcrToFind:
                    if (pcrToFind.IsReferencing(propertyMemberAst))
                    {
                        AddResult(new PropertySymbolReference(propertyMemberAst));
                    }
                    break;
            }

            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitFunctionMember(FunctionMemberAst functionMemberAst)
        {
            if (symbolRef.SymbolType != SymbolType.Method && symbolRef.SymbolType != SymbolType.Constructor)
            {
                return AstVisitAction.Continue;
            }

            if (string.Compare(symbolRef.SymbolName, functionMemberAst.Name, StringComparison.CurrentCultureIgnoreCase) != 0)
            {
                return AstVisitAction.Continue;
            }


            switch (symbolRef)
            {
                case MethodSymbolReference msr:
                    if (msr.IsReferencing(functionMemberAst))
                    {
                        AddResult(new MethodSymbolReference(functionMemberAst));
                    }
                    break;
                case MethodCallReference mcr:
                    if (mcr.IsReferencing(functionMemberAst))
                    {
                        AddResult(new MethodSymbolReference(functionMemberAst));
                    }
                    break;
            }

            return AstVisitAction.Continue;
        }
    }
}
