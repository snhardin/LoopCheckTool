using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopCheckTool.Lib.Document
{
    public class WriterConfiguration
    {
        /// <summary>
        /// Ignore all errors that can be ignored. They will be logged instead.
        /// </summary>
        public bool IgnoreErrors { get; set; } = false;
    }
}
