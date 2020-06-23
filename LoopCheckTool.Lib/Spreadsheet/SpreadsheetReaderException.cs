using System;
using System.Collections.Generic;
using System.Text;

namespace LoopCheckTool.Lib.Spreadsheet
{
    /// <summary>
    /// Custom exception for Spreadsheet read-related errors
    /// </summary>
    public class SpreadsheetReaderException : Exception
    {
        public SpreadsheetReaderException() { }
        public SpreadsheetReaderException(string message) : base(message) { }
        public SpreadsheetReaderException(string message, Exception inner) : base(message, inner) { }
    }
}
