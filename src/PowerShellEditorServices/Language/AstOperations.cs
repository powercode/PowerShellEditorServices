//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Microsoft.PowerShell.EditorServices.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.PowerShell.EditorServices
{
    using System.Diagnostics;
    using System.Management.Automation;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;

    /// <summary>
    /// Provides common operations for the syntax tree of a parsed script.
    /// </summary>
    internal static class AstOperations
    {

        internal static IAstVisitorFactory AstVisitorFactory = new LegacyAstVisitorFactory();

        /// <summary>
        /// Gets completions for the symbol found in the Ast at
        /// the given file offset.
        /// </summary>
        /// <param name="scriptAst">
        /// The Ast which will be traversed to find a completable symbol.
        /// </param>
        /// <param name="currentTokens">
        /// The array of tokens corresponding to the scriptAst parameter.
        /// </param>
        /// <param name="fileOffset">
        /// The 1-based file offset at which a symbol will be located.
        /// </param>
        /// <param name="powerShellContext">
        /// The PowerShellContext to use for gathering completions.
        /// </param>
        /// <param name="logger">An ILogger implementation used for writing log messages.</param>
        /// <param name="cancellationToken">
        /// A CancellationToken to cancel completion requests.
        /// </param>
        /// <returns>
        /// A CommandCompletion instance that contains completions for the
        /// symbol at the given offset.
        /// </returns>
        public static async Task<CommandCompletion> GetCompletions(
            Ast scriptAst,
            Token[] currentTokens,
            int fileOffset,
            PowerShellContext powerShellContext,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var type = scriptAst.Extent.StartScriptPosition.GetType();
            var method =
#if CoreCLR
                type.GetMethod(
                    "CloneWithNewOffset",
                    BindingFlags.Instance | BindingFlags.NonPublic);
#else
                type.GetMethod(
                    "CloneWithNewOffset",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(int) }, null);
#endif

            IScriptPosition cursorPosition =
                (IScriptPosition)method.Invoke(
                    scriptAst.Extent.StartScriptPosition,
                    new object[] { fileOffset });

            logger.Write(
                LogLevel.Verbose,
                $"Getting completions at offset {fileOffset} (line: {cursorPosition.LineNumber}, column: {cursorPosition.ColumnNumber})");

            CommandCompletion commandCompletion = null;
            if (powerShellContext.IsDebuggerStopped)
            {
                PSCommand command = new PSCommand();
                command.AddCommand("TabExpansion2");
                command.AddParameter("Ast", scriptAst);
                command.AddParameter("Tokens", currentTokens);
                command.AddParameter("PositionOfCursor", cursorPosition);
                command.AddParameter("Options", null);

                PSObject outputObject =
                    (await powerShellContext.ExecuteCommand<PSObject>(command, false, false))
                        .FirstOrDefault();

                if (outputObject != null)
                {
                    if (outputObject.BaseObject is ErrorRecord errorRecord)
                    {
                        logger.WriteException(
                            "Encountered an error while invoking TabExpansion2 in the debugger",
                            errorRecord.Exception);
                    }
                    else
                    {
                        commandCompletion = outputObject.BaseObject as CommandCompletion;
                    }
                }
            }
            else if (powerShellContext.CurrentRunspace.Runspace.RunspaceAvailability ==
                        RunspaceAvailability.Available)
            {
                using (RunspaceHandle runspaceHandle = await powerShellContext.GetRunspaceHandle(cancellationToken))
                using (PowerShell powerShell = PowerShell.Create())
                {
                    powerShell.Runspace = runspaceHandle.Runspace;

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    commandCompletion =
                        CommandCompletion.CompleteInput(
                            scriptAst,
                            currentTokens,
                            cursorPosition,
                            null,
                            powerShell);

                    stopwatch.Stop();

                    logger.Write(LogLevel.Verbose, $"IntelliSense completed in {stopwatch.ElapsedMilliseconds}ms.");
                }
            }

            return commandCompletion;
        }

        /// <summary>
        /// Finds the symbol at a given file location
        /// </summary>
        /// <param name="scriptAst">The abstract syntax tree of the given script</param>
        /// <param name="lineNumber">The line number of the cursor for the given script</param>
        /// <param name="columnNumber">The coulumn number of the cursor for the given script</param>
        /// <param name="includeFunctionDefinitions">Includes full function definition ranges in the search.</param>
        /// <returns>SymbolReference of found symbol</returns>
        public static SymbolReference FindSymbolAtPosition(
            Ast scriptAst,
            int lineNumber,
            int columnNumber,
            bool includeFunctionDefinitions = false)
        {
            var result = new List<SymbolReference>();
            AstVisitor symbolVisitor =
                AstVisitorFactory.FindSymbolVisitor(
                    lineNumber,
                    columnNumber,
                    includeFunctionDefinitions, result);

            scriptAst.Visit(symbolVisitor);

            return result.FirstOrDefault();
        }

        /// <summary>
        /// Finds the symbol (always Command type) at a given file location
        /// </summary>
        /// <param name="scriptAst">The abstract syntax tree of the given script</param>
        /// <param name="lineNumber">The line number of the cursor for the given script</param>
        /// <param name="columnNumber">The column number of the cursor for the given script</param>
        /// <returns>SymbolReference of found command</returns>
        public static SymbolReference FindCommandAtPosition(Ast scriptAst, int lineNumber, int columnNumber)
        {
            var result = new List<SymbolReference>();
            AstVisitor commandVisitor = AstVisitorFactory.FindCommandVisitor(lineNumber, columnNumber, result);
            scriptAst.Visit(commandVisitor);

            return result.FirstOrDefault();
        }

        /// <summary>
        /// Finds all references (including aliases) in a script for the given symbol
        /// </summary>
        /// <param name="scriptAst">The abstract syntax tree of the given script</param>
        /// <param name="symbolReference">The symbol that we are looking for referneces of</param>
        /// <param name="cmdletToAliasDictionary">Dictionary maping cmdlets to aliases for finding alias references</param>
        /// <param name="aliasToCmdletDictionary">Dictionary maping aliases to cmdlets for finding alias references</param>
        /// <returns></returns>
        public static IEnumerable<SymbolReference> FindReferencesOfSymbol(
            Ast scriptAst,
            SymbolReference symbolReference,
            Dictionary<String, List<String>> cmdletToAliasDictionary,
            Dictionary<String, String> aliasToCmdletDictionary)
        {
            var results = new List<SymbolReference>();
            // find the symbol evaluators for the node types we are handling
            var referencesVisitor =
                AstVisitorFactory.FindReferencesVisitor(
                    symbolReference,
                    cmdletToAliasDictionary,
                    aliasToCmdletDictionary,
                    results);
            scriptAst.Visit(referencesVisitor);

            return results;
        }

        /// <summary>
        /// Finds all references (not including aliases) in a script for the given symbol
        /// </summary>
        /// <param name="scriptAst">The abstract syntax tree of the given script</param>
        /// <param name="foundSymbol">The symbol that we are looking for referneces of</param>
        /// <param name="needsAliases">If this reference search needs aliases.
        /// This should always be false and used for occurence requests</param>
        /// <returns>A collection of SymbolReference objects that are refrences to the symbolRefrence
        /// not including aliases</returns>
        public static IEnumerable<SymbolReference> FindReferencesOfSymbol(
            ScriptBlockAst scriptAst,
            SymbolReference foundSymbol,
            bool needsAliases)
        {
            var result = new List<SymbolReference>();
            AstVisitor referencesVisitor =
                AstVisitorFactory.FindReferencesVisitor(foundSymbol, result);
            scriptAst.Visit(referencesVisitor);

            return result;
        }

        /// <summary>
        /// Finds the definition of the symbol
        /// </summary>
        /// <param name="scriptAst">The abstract syntax tree of the given script</param>
        /// <param name="symbolReference">The symbol that we are looking for the definition of</param>
        /// <returns>A SymbolReference of the definition of the symbolReference</returns>
        public static SymbolReference FindDefinitionOfSymbol(
            Ast scriptAst,
            SymbolReference symbolReference)
        {
            var result = new List<SymbolReference>();
            var declarationVisitor =
                AstVisitorFactory.FindDeclarationVisitor(symbolReference, result);
            scriptAst.Visit(declarationVisitor);

            return result.FirstOrDefault();
        }

        /// <summary>
        /// Finds all symbols in a script
        /// </summary>
        /// <param name="scriptAst">The abstract syntax tree of the given script</param>
        /// <param name="powerShellVersion">The PowerShell version the Ast was generated from</param>
        /// <returns>A collection of SymbolReference objects</returns>
        public static IEnumerable<SymbolReference> FindSymbolsInDocument(Ast scriptAst, Version powerShellVersion)
        {
            // TODO: Restore this when we figure out how to support multiple
            //       PS versions in the new PSES-as-a-module world (issue #276)
            //            if (powerShellVersion >= new Version(5,0))
            //            {
            //#if PowerShellv5
            //                FindSymbolsVisitor2 findSymbolsVisitor = new FindSymbolsVisitor2();
            //                scriptAst.Visit(findSymbolsVisitor);
            //                symbolReferences = findSymbolsVisitor.SymbolReferences;
            //#endif
            //            }
            //            else
            var result = new List<SymbolReference>();
            var findSymbolsVisitor = AstVisitorFactory.FindSymbolsVisitor(result);
            scriptAst.Visit(findSymbolsVisitor);

            return result;
        }

        /// <summary>
        /// Checks if a given ast represents the root node of a *.psd1 file.
        /// </summary>
        /// <param name="ast">The abstract syntax tree of the given script</param>
        /// <returns>true if the AST represts a *.psd1 file, otherwise false</returns>
        public static bool IsPowerShellDataFileAst(Ast ast)
        {
            // sometimes we don't have reliable access to the filename
            // so we employ heuristics to check if the contents are
            // part of a psd1 file.
            return IsPowerShellDataFileAstNode(
                        new { Item = ast, Children = new List<dynamic>() },
                        new Type[] {
                            typeof(ScriptBlockAst),
                            typeof(NamedBlockAst),
                            typeof(PipelineAst),
                            typeof(CommandExpressionAst),
                            typeof(HashtableAst) },
                        0);
        }

        private static bool IsPowerShellDataFileAstNode(dynamic node, Type[] levelAstMap, int level)
        {
            var levelAstTypeMatch = node.Item.GetType().Equals(levelAstMap[level]);
            if (!levelAstTypeMatch)
            {
                return false;
            }

            if (level == levelAstMap.Length - 1)
            {
                return levelAstTypeMatch;
            }

            var astsFound = (node.Item as Ast)?.FindAll(a => a != null, false);
            if (astsFound != null)
            {
                foreach (var astFound in astsFound)
                {
                    if (!astFound.Equals(node.Item)
                        && node.Item.Equals(astFound.Parent)
                        && IsPowerShellDataFileAstNode(
                            new { Item = astFound, Children = new List<dynamic>() },
                            levelAstMap,
                            level + 1))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Finds all files dot sourced in a script
        /// </summary>
        /// <param name="scriptAst">The abstract syntax tree of the given script</param>
        /// <returns></returns>
        public static string[] FindDotSourcedIncludes(Ast scriptAst)
        {
            FindDotSourcedVisitor dotSourcedVisitor = new FindDotSourcedVisitor();
            scriptAst.Visit(dotSourcedVisitor);

            return dotSourcedVisitor.DotSourcedFiles.ToArray();
        }
    }
}
