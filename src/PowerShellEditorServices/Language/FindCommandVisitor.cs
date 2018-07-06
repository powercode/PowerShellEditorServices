//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;

namespace Microsoft.PowerShell.EditorServices
{
    /// <summary>
    /// The vistior used to find the commandAst of a specific location in an AST
    /// </summary>
    public class FindCommandVisitor : AstVisitor
    {
        private readonly int lineNumber;
        private readonly int columnNumber;
        private readonly List<SymbolReference> result;

        public SymbolReference FoundCommandReference => result.FirstOrDefault();

        public FindCommandVisitor(int lineNumber, int columnNumber, List<SymbolReference> result)
        {
            this.lineNumber = lineNumber;
            this.columnNumber = columnNumber;
            this.result = result;
        }

        public override AstVisitAction VisitPipeline(PipelineAst pipelineAst)
        {
            if (this.lineNumber == pipelineAst.Extent.StartLineNumber)
            {
                // Which command is the cursor in?
                foreach (var commandAst in pipelineAst.PipelineElements.OfType<CommandAst>())
                {
                    int trueEndColumnNumber = commandAst.Extent.EndColumnNumber;
                    string currentLine = commandAst.Extent.StartScriptPosition.Line;

                    if (currentLine.Length >= trueEndColumnNumber)
                    {
                        // Get the text left in the line after the command's extent
                        string remainingLine =
                            currentLine.Substring(
                                commandAst.Extent.EndColumnNumber);

                        // Calculate the "true" end column number by finding out how many
                        // whitespace characters are between this command and the next (or
                        // the end of the line).
                        // NOTE: +1 is added to trueEndColumnNumber to account for the position
                        // just after the last character in the command string or script line.
                        int preTrimLength = remainingLine.Length;
                        int postTrimLength = remainingLine.TrimStart().Length;
                        trueEndColumnNumber =
                            commandAst.Extent.EndColumnNumber +
                            (preTrimLength - postTrimLength) + 1;
                    }

                    if (commandAst.Extent.StartColumnNumber <= columnNumber &&
                        trueEndColumnNumber >= columnNumber)
                    {
                        result.Add(
                            new SymbolReference(
                                SymbolType.Function,
                                commandAst.CommandElements[0].Extent));

                        return AstVisitAction.StopVisit;
                    }
                }
            }

            return base.VisitPipeline(pipelineAst);
        }

        /// <summary>
        /// Is the position of the given location is in the range of the start
        /// of the first element to the character before the second element
        /// </summary>
        /// <param name="firstExtent">The script extent of the first element of the command ast</param>
        /// <param name="secondExtent">The script extent of the second element of the command ast</param>
        /// <returns>True if the given position is in the range of the start of
        /// the first element to the character before the second element</returns>
        private bool IsPositionInExtent(IScriptExtent firstExtent, IScriptExtent secondExtent)
        {
            return (firstExtent.StartLineNumber == lineNumber &&
                    firstExtent.StartColumnNumber <= columnNumber &&
                    secondExtent.StartColumnNumber >= columnNumber - 1);
        }
    }
}
