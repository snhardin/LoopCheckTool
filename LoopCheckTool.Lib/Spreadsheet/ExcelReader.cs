using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
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
        private SpreadsheetDocument document;
        private List<string> headers;

        public ExcelReader(string fileName)
        {
            this.document = SpreadsheetDocument.Open(fileName, false);
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

        public string ReadRows(Worksheet input)
        {
            StringBuilder str = new StringBuilder();
            SharedStringTable sharedStrings = document.WorkbookPart.SharedStringTablePart.SharedStringTable;
            WorksheetPart worksheetData = (WorksheetPart) document.WorkbookPart.GetPartById(input.ID);

            using (OpenXmlReader reader = OpenXmlReader.Create(worksheetData))
            {
                while (reader.Read())
                {
                    if (reader.ElementType == typeof(Row))
                    {
                        reader.ReadFirstChild();

                        do
                        {
                            if (reader.ElementType == typeof(Cell))
                            {
                                Cell c = (Cell)reader.LoadCurrentElement();

                                if (c.DataType != null && c.DataType == CellValues.SharedString)
                                {
                                    str.Append(sharedStrings.ElementAt(int.Parse(c.CellValue.Text)).InnerText + " ");
                                }
                                else
                                {
                                    str.Append(c.CellValue.Text);
                                }
                            }
                        } while (reader.ReadNextSibling());
                    }
                }
            }

            return str.ToString();
        }

        public class Worksheet
        {
            public Worksheet(string id, string name)
            {
                this.ID = id;
                this.Name = name;
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
                this.reader = OpenXmlReader.Create(worksheetData);
                this.headers = LoadHeaders();
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
                    throw new Exception("Did not find a Row before the end of the worksheet.");
                }
            }

            private string GetCellColumn(Cell c)
            {
                // Taken from MSDN
                // https://docs.microsoft.com/en-us/office/open-xml/how-to-get-a-column-heading-in-a-spreadsheet
                Regex regex = new Regex("[A-Za-z]+");
                Match match = regex.Match(c.CellReference.Value);

                return match.Value;
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

                void func(Cell cell)
                {
                    string cellColumn = GetCellColumn(cell);
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
                        keyValues.Add(missingHeader, "");
                    }

                    return keyValues;
                }
                else
                {
                    return null; // TODO
                }
            }

            public void Dispose()
            {
                reader.Close();
            }
        }
    }
}
