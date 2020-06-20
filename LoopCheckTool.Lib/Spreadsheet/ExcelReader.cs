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
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly SpreadsheetDocument document;

        public ExcelReader(string fileName)
        {
            try
            {
                document = SpreadsheetDocument.Open(fileName, false);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "An error occurred while creating the Excel Reader");
                throw ex;
            }
        }

        public void Dispose()
        {
            try
            {
                document.Close();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "An error occurred while disposing of the Excel Reader");
                throw ex;
            }
        }

        public IEnumerable<Worksheet> GetWorksheets()
        {
            try
            {
                return document.WorkbookPart.Workbook.Descendants<Sheet>().Select(s => new Worksheet(s.Id.Value, s.Name.Value));
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "An error occurred while getting spreadsheets.");
                throw ex;
            }
        }

        public IList<string> GetHeader(Worksheet worksheet)
        {
            try
            {
                List<string> result = new List<string>();
                SharedStringTable sharedStrings = document.WorkbookPart.SharedStringTablePart.SharedStringTable;

                WorksheetPart worksheetData = (WorksheetPart)document.WorkbookPart.GetPartById(worksheet.ID);
                using (OpenXmlReader reader = OpenXmlReader.Create(worksheetData))
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
                                        if (c.DataType != null && c.DataType == CellValues.SharedString)
                                        {
                                            result.Add(sharedStrings.ElementAt(int.Parse(c.CellValue.Text)).InnerText);
                                        }
                                        else
                                        {
                                            result.Add(c.CellValue.Text);
                                        }
                                    }
                                } while (reader.ReadNextSibling());

                                return result;
                            }

                            // If logic reaches here, then this row had no cells.
                            // Skip to the next row and check.
                            Logger.Warn("Found a row with no cells.");
                        }
                    }
                }

                throw new ExcelReaderException("No header found for worksheet.");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "An error occurred while getting header.");
                throw ex;
            }
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
                try
                {
                    this.document = document;
                    WorksheetPart worksheetData = (WorksheetPart)document.WorkbookPart.GetPartById(worksheet.ID);
                    reader = OpenXmlReader.Create(worksheetData);
                    headers = LoadHeaders();
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex, "An error occurred while creating a RowReaderContext");
                    throw ex;
                }
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
                        Logger.Warn("Found a row with no cells.");
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

                string cellText = c.CellValue?.Text;
                if (c.DataType != null && c.DataType == CellValues.SharedString)
                    return sharedStrings.ElementAt(int.Parse(cellText)).InnerText;

                // Check if the text is a number.
                if (!string.IsNullOrEmpty(cellText) && double.TryParse(cellText, out double convDbl))
                {
                    System.Globalization.NumberStyles styles = System.Globalization.NumberStyles.Float;
                    System.Globalization.CultureInfo cultureInfo = System.Globalization.CultureInfo.InvariantCulture;

                    // Try to parse as a long and a decimal.
                    if (long.TryParse(cellText, styles, cultureInfo, out long convLong))
                        return convLong.ToString();
                    if (decimal.TryParse(cellText, styles, cultureInfo, out decimal convDec))
                        return convDec.ToString();

                    // If all else fails, return a formatted double.
                    return convDbl.ToString("N").Replace(",", "");
                }

                return cellText;
            }

            public IDictionary<string, string> ReadNextRow()
            {
                try
                {
                    Dictionary<string, string> keyValues = new Dictionary<string, string>();
                    uint row = uint.MaxValue;

                    void func(Cell cell)
                    {
                        string cellColumn = GetCellColumn(cell);
                        row = GetCellRow(cell);
                        if (headers.TryGetValue(cellColumn, out string header))
                        {
                            string val = GetCellValue(cell);
                            if (val != null)
                            {
                                keyValues.Add(header, val);
                            }
                        }
                    }

                    if (ReadNextRow(func))
                    {
                        // Now check if there were any columns with empty cells.
                        IEnumerable<string> missingHeaders = headers.Values.Except(keyValues.Keys);
                        foreach (string missingHeader in missingHeaders)
                        {
                            Logger.Warn($"Cell for column \"{missingHeader}\" missing for row \"{row}\".");
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
                catch (Exception ex)
                {
                    Logger.Fatal(ex, "An error occurred while reading the next row.");
                    throw ex;
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
