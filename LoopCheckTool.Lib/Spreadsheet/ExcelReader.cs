using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using LoopCheckTool.Lib.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LoopCheckTool.Lib.Spreadsheet
{
    public class ExcelReader : IDisposable
    {
        private static readonly log4net.ILog logger = Logger.GetOrLoadLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private SpreadsheetDocument document;

        public ExcelReader(string fileName)
        {
            document = SpreadsheetDocument.Open(fileName, false);
        }

        public void Dispose()
        {
            document.Close();
        }

        public IEnumerable<Worksheet> GetWorksheets()
        {
            return document.WorkbookPart.Workbook.Descendants<Sheet>().Select(s => new Worksheet(s.Id.Value, s.Name.Value));
        }

        public RowReaderContext CreateRowReader(Worksheet worksheet)
        {
            return new RowReaderContext(document, worksheet);
        }

        public class Worksheet
        {
            public Worksheet(string id, string name)
            {
                ID = id;
                Name = name;
            }

            public string ID { get; }
            public string Name { get; }
        }

        public class RowReaderContext : IDisposable
        {
            private SpreadsheetDocument document;
            private IDictionary<string, string> headers;
            private OpenXmlReader reader;

            public RowReaderContext(SpreadsheetDocument document, Worksheet worksheet)
            {
                this.document = document;
                WorksheetPart worksheetData = (WorksheetPart)document.WorkbookPart.GetPartById(worksheet.ID);
                reader = OpenXmlReader.Create(worksheetData);
                headers = LoadHeaders();
            }

            private bool ReadNextRow(Action<Cell> func)
            {
                // Read until a Row object is found.
                while (reader.Read())
                {
                    if (reader.ElementType == typeof(Row))
                    {
                        if (reader.ReadFirstChild())
                        {
                            do
                            {
                                if (reader.ElementType == typeof(Cell))
                                {
                                    Cell c = (Cell)reader.LoadCurrentElement();
                                    func(c);
                                }
                            } while (reader.ReadNextSibling());

                            return true;
                        }

                        // If logic reaches here, then this row had no cells.
                        // Skip to the next row and check.
                        logger.Warn("Found a row with no cells.");
                    }
                }

                return false;
            }

            private IDictionary<string, string> LoadHeaders()
            {
                // TODO: Check for duplicates.
                Dictionary<string, string> headers = new Dictionary<string, string>();
                void func(Cell cell)
                {
                    string key = GetCellColumn(cell);
                    string value = GetCellValue(cell);
                    headers.Add(key, value);
                }

                if (ReadNextRow(func))
                {
                    return headers;
                }
                else
                {
                    throw new ExcelReaderException("Could not find headers for the Excel spreadsheet.");
                }
            }

            private string GetCellColumn(Cell c)
            {
                // Taken from MSDN
                // https://docs.microsoft.com/en-us/office/open-xml/how-to-get-a-column-heading-in-a-spreadsheet
                Regex regex = new Regex("[A-Za-z]+");
                Match match = regex.Match(c.CellReference.Value);

                if (string.IsNullOrEmpty(match.Value))
                {
                    throw new ExcelReaderException($"Could not decipher the column name from the following reference: \"{c.CellReference.Value}\".");
                }

                return match.Value;
            }

            private uint GetCellRow(Cell c)
            {
                // Taken from MSDN
                // https://docs.microsoft.com/en-us/office/open-xml/how-to-get-a-column-heading-in-a-spreadsheet
                Regex regex = new Regex(@"\d+");
                Match match = regex.Match(c.CellReference.Value);

                if (string.IsNullOrEmpty(match.Value))
                {
                    throw new ExcelReaderException($"Could not decipher the column name from the following cell: \"{c.CellReference.Value}\".");
                }

                if (uint.TryParse(match.Value, out uint result))
                {
                    return result;
                }
                else
                {
                    throw new ExcelReaderException($"Could not parse integer from the following cell: \"{c.CellReference.Value}\"\n" +
                        $"Matched regex: {match.Value}.");
                }
            }

            private string GetCellValue(Cell c)
            {
                SharedStringTable sharedStrings = document.WorkbookPart.SharedStringTablePart.SharedStringTable;

                if (c.DataType != null && c.DataType == CellValues.SharedString)
                {
                    return sharedStrings.ElementAt(int.Parse(c.CellValue.Text)).InnerText;
                }
                else
                {
                    return c.CellValue.Text;
                }
            }

            public IDictionary<string, string> ReadNextRow()
            {
                Dictionary<string, string> keyValues = new Dictionary<string, string>();
                uint row = uint.MaxValue;

                void func(Cell cell)
                {
                    string cellColumn = GetCellColumn(cell);
                    row = GetCellRow(cell);
                    if (headers.TryGetValue(cellColumn, out string header))
                    {
                        keyValues.Add(header, GetCellValue(cell));
                    }
                }

                if (ReadNextRow(func))
                {
                    // Now check if there were any columns with empty cells.
                    IEnumerable<string> missingHeaders = headers.Values.Except(keyValues.Keys);
                    foreach (string missingHeader in missingHeaders)
                    {
                        logger.Warn($"Cell for column \"{missingHeader}\" missing for row \"{row}\".");
                        keyValues.Add(missingHeader, "");
                    }

                    return keyValues;
                }
                else
                {
                    // There are no more rows left.
                    return null;
                }
            }

            public void Dispose()
            {
                reader.Close();
            }
        }

        public class ExcelReaderException : Exception
        {
            public ExcelReaderException() { }
            public ExcelReaderException(string message) : base(message) { }
            public ExcelReaderException(string message, Exception inner) : base(message, inner) { }
        }
    }
}
