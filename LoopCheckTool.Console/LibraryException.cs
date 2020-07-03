using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopCheckTool.Console
{
    public class LibraryException : Exception
    {
        public int AffectedRow { get; }
        public IDictionary<string, string> RowData { get; }
        public LibraryException(int affectedRow, IDictionary<string, string> rowData) : base()
        {
            AffectedRow = affectedRow;
            RowData = rowData;
        }

        public LibraryException(string message, int affectedRow, IDictionary<string, string> rowData) : base(message)
        {
            AffectedRow = affectedRow;
            RowData = rowData;
        }

        public LibraryException(string message, Exception innerException, int affectedRow, IDictionary<string, string> rowData) : base(message, innerException)
        {
            AffectedRow = affectedRow;
            RowData = rowData;
        }
    }
}
