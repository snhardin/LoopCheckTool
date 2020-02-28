using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopCheckTool.Console
{
    public class LoopCheckInputException : Exception
    {
        public LoopCheckInputException() : base() { }
        public LoopCheckInputException(string message) : base(message) { }
        public LoopCheckInputException(string message, Exception innerException) : base(message, innerException) { }
    }
}
