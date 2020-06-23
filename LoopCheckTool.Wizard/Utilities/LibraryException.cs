using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopCheckTool.Wizard.Utilities
{
    public class LibraryException : Exception
    {
        public ulong AffectedRow { get; }
        public override string Message
        {
            get
            {
                return $"{base.Message}, AffectedRow={AffectedRow}";
            }
        }
        public IDictionary<string, string> RowData { get; }
        public LibraryException(ulong affectedRow, IDictionary<string, string> rowData) : base()
        {
            AffectedRow = affectedRow;
            RowData = rowData;
        }

        public LibraryException(string message, ulong affectedRow, IDictionary<string, string> rowData) : base(message)
        {
            AffectedRow = affectedRow;
            RowData = rowData;
        }

        public LibraryException(string message, Exception innerException, ulong affectedRow, IDictionary<string, string> rowData) : base(message, innerException)
        {
            AffectedRow = affectedRow;
            RowData = rowData;
        }
    }
}
