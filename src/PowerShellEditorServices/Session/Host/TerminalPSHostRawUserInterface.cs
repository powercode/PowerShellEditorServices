﻿//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Microsoft.PowerShell.EditorServices.Utility;
using System;
using System.Management.Automation.Host;

namespace Microsoft.PowerShell.EditorServices
{
    /// <summary>
    /// Provides an implementation of the PSHostRawUserInterface class
    /// for the ConsoleService and routes its calls to an IConsoleHost
    /// implementation.
    /// </summary>
    internal class TerminalPSHostRawUserInterface : PSHostRawUserInterface
    {
        #region Private Fields

        private const int DefaultConsoleHeight = 100;
        private const int DefaultConsoleWidth = 120;

        private ILogger Logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the TerminalPSHostRawUserInterface
        /// class with the given IConsoleHost implementation.
        /// </summary>
        /// <param name="logger">The ILogger implementation to use for this instance.</param>
        public TerminalPSHostRawUserInterface(ILogger logger)
        {
            this.Logger = logger;
        }

        #endregion

        #region PSHostRawUserInterface Implementation

        /// <summary>
        /// Gets or sets the background color of the console.
        /// </summary>
        public override ConsoleColor BackgroundColor
        {
            get { return System.Console.BackgroundColor; }
            set { System.Console.BackgroundColor = value; }
        }

        /// <summary>
        /// Gets or sets the foreground color of the console.
        /// </summary>
        public override ConsoleColor ForegroundColor
        {
            get { return System.Console.ForegroundColor; }
            set { System.Console.ForegroundColor = value; }
        }

        /// <summary>
        /// Gets or sets the size of the console buffer.
        /// </summary>
        public override Size BufferSize
        {
            get
            {
                return
                    new Size(
                        System.Console.BufferWidth,
                        System.Console.BufferHeight);
            }
            set
            {
                System.Console.BufferWidth = value.Width;
                System.Console.BufferHeight = value.Height;
            }
        }

        /// <summary>
        /// Gets or sets the cursor's position in the console buffer.
        /// </summary>
        public override Coordinates CursorPosition
        {
            get
            {
                return
                    new Coordinates(
                        System.Console.CursorLeft,
                        System.Console.CursorTop);
            }
            set
            {
                System.Console.CursorLeft = value.X;
                System.Console.CursorTop = value.Y;
            }
        }

        /// <summary>
        /// Gets or sets the size of the cursor in the console buffer.
        /// </summary>
        public override int CursorSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the position of the console's window.
        /// </summary>
        public override Coordinates WindowPosition
        {
            get
            {
                return
                    new Coordinates(
                        System.Console.WindowLeft,
                        System.Console.WindowTop);
            }
            set
            {
                System.Console.WindowLeft = value.X;
                System.Console.WindowTop = value.Y;
            }
        }

        /// <summary>
        /// Gets or sets the size of the console's window.
        /// </summary>
        public override Size WindowSize
        {
            get
            {
                return
                    new Size(
                        System.Console.WindowWidth,
                        System.Console.WindowHeight);
            }
            set
            {
                System.Console.WindowWidth = value.Width;
                System.Console.WindowHeight = value.Height;
            }
        }

        /// <summary>
        /// Gets or sets the console window's title.
        /// </summary>
        public override string WindowTitle
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a boolean that determines whether a keypress is available.
        /// </summary>
        public override bool KeyAvailable
        {
            get { return System.Console.KeyAvailable; }
        }

        /// <summary>
        /// Gets the maximum physical size of the console window.
        /// </summary>
        public override Size MaxPhysicalWindowSize
        {
            get { return new Size(DefaultConsoleWidth, DefaultConsoleHeight); }
        }

        /// <summary>
        /// Gets the maximum size of the console window.
        /// </summary>
        public override Size MaxWindowSize
        {
            get { return new Size(DefaultConsoleWidth, DefaultConsoleHeight); }
        }

        /// <summary>
        /// Reads the current key pressed in the console.
        /// </summary>
        /// <param name="options">Options for reading the current keypress.</param>
        /// <returns>A KeyInfo struct with details about the current keypress.</returns>
        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            Logger.Write(
                LogLevel.Warning,
                "PSHostRawUserInterface.ReadKey was called");

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Flushes the current input buffer.
        /// </summary>
        public override void FlushInputBuffer()
        {
            Logger.Write(
                LogLevel.Warning,
                "PSHostRawUserInterface.FlushInputBuffer was called");
        }

        /// <summary>
        /// Gets the contents of the console buffer in a rectangular area.
        /// </summary>
        /// <param name="rectangle">The rectangle inside which buffer contents will be accessed.</param>
        /// <returns>A BufferCell array with the requested buffer contents.</returns>
        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            return new BufferCell[0,0];
        }

        /// <summary>
        /// Scrolls the contents of the console buffer.
        /// </summary>
        /// <param name="source">The source rectangle to scroll.</param>
        /// <param name="destination">The destination coordinates by which to scroll.</param>
        /// <param name="clip">The rectangle inside which the scrolling will be clipped.</param>
        /// <param name="fill">The cell with which the buffer will be filled.</param>
        public override void ScrollBufferContents(
            Rectangle source,
            Coordinates destination,
            Rectangle clip,
            BufferCell fill)
        {
            Logger.Write(
                LogLevel.Warning,
                "PSHostRawUserInterface.ScrollBufferContents was called");
        }

        /// <summary>
        /// Sets the contents of the buffer inside the specified rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle inside which buffer contents will be filled.</param>
        /// <param name="fill">The BufferCell which will be used to fill the requested space.</param>
        public override void SetBufferContents(
            Rectangle rectangle,
            BufferCell fill)
        {
            // If the rectangle is all -1s then it means clear the visible buffer
            if (rectangle.Top == -1 &&
                rectangle.Bottom == -1 &&
                rectangle.Left == -1 &&
                rectangle.Right == -1)
            {
                System.Console.Clear();
            }
            else
            {
                Logger.Write(
                    LogLevel.Warning,
                    "PSHostRawUserInterface.SetBufferContents was called with a specific region");
            }
        }

        /// <summary>
        /// Sets the contents of the buffer at the given coordinate.
        /// </summary>
        /// <param name="origin">The coordinate at which the buffer will be changed.</param>
        /// <param name="contents">The new contents for the buffer at the given coordinate.</param>
        public override void SetBufferContents(
            Coordinates origin,
            BufferCell[,] contents)
        {
            Logger.Write(
                LogLevel.Warning,
                "PSHostRawUserInterface.SetBufferContents was called");
        }

        #endregion
    }
}
