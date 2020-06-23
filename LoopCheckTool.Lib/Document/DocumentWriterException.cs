using System;
using System.Collections.Generic;
using System.Text;

namespace LoopCheckTool.Lib.Document
{
    /// <summary>
    /// Custom exception for Document write-related errors
    /// </summary>
    public class DocumentWriterException : Exception
    {
        public DocumentWriterException() { }
        public DocumentWriterException(string message) : base(message) { }
        public DocumentWriterException(string message, Exception inner) : base(message, inner) { }
    }
}
