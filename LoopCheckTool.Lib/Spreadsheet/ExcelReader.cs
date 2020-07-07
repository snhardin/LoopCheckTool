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
    /// <summary>
    /// Concrete implementation for reading an IO List in Excel
    /// </summary>
    public class ExcelReader : IDisposable
    {
        /// <summary>
        /// Logger used for debugging and bug reporting
        /// </summary>
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The open Excel document
        /// </summary>
        private readonly SpreadsheetDocument document;

        /// <summary>
        /// Basic constructor to open Excel IO List
        /// </summary>
        /// <param name="fileName">The path of the file to open</param>
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

        /// <summary>
        /// Disposes of open Excel IO List file handler
        /// </summary>
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

        /// <summary>
        /// Gets all worksheets in the Excel file
        /// </summary>
        /// <returns>Enumerable of Worksheets</returns>
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

        /// <summary>
        /// Gets the header (first row) of the specified sheet
        /// </summary>
        /// <param name="worksheet">The sheet to use</param>
        /// <returns>A list of cell values for the first row of the sheet</returns>
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

                throw new SpreadsheetReaderException("No header found for worksheet.");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "An error occurred while getting header.");
                throw ex;
            }
        }

        /// <summary>
        /// Creates a row reader for reading values from the Excel sheet
        /// </summary>
        /// <param name="worksheet">The worksheet to read values from</param>
        /// <returns>A row reader</returns>
        public RowReader CreateRowReader(Worksheet worksheet)
        {
            return new RowReader(document, worksheet);
        }

        /// <summary>
        /// Public concrete plain representation of a worksheet in Excel
        /// </summary>
        public class Worksheet
        {
            /// <summary>
            /// Constructor for plain worksheet
            /// </summary>
            /// <param name="id">ID of the worksheet</param>
            /// <param name="name">Name of the worksheet.</param>
            public Worksheet(string id, string name)
            {
                ID = id;
                Name = name;
            }

            /// <summary>
            /// ID of the worksheet
            /// </summary>
            public string ID { get; }

            /// <summary>
            /// Name of the worksheet
            /// </summary>
            public string Name { get; }
        }

        /// <summary>
        /// Class that keeps context surrounding reading the IO List row-by-row
        /// </summary>
        public class RowReader : IDisposable
        {
            /// <summary>
            /// The open Excel IO List document
            /// </summary>
            private readonly SpreadsheetDocument document;

            /// <summary>
            /// The headers to reference from the top of the document
            /// </summary>
            private readonly IDictionary<string, string> headers;

            /// <summary>
            /// Instance of the reader of the Excel document
            /// </summary>
            private readonly OpenXmlReader reader;

            /// <summary>
            /// Instantiates a new RowReader
            /// </summary>
            /// <param name="document">The spreadsheet document to read from</param>
            /// <param name="worksheet">The worksheet within the spreadsheet to read from</param>
            internal RowReader(SpreadsheetDocument document, Worksheet worksheet)
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

            /// <summary>
            /// Reads the next row in the spreadsheet and performs some action on said row
            /// </summary>
            /// <param name="func">The action to perform on the row of Cells</param>
            /// <returns>True if a row was found, false if not</returns>
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

            /// <summary>
            /// Loads headers from the Excel spreadsheet, getting a mapping of columns to headers
            /// </summary>
            /// <returns>Dictionary with column as the key and header names as the values</returns>
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
                    throw new SpreadsheetReaderException("Could not find headers for the Excel spreadsheet.");
                }
            }

            /// <summary>
            /// Gets the column of a cell in the form of a letter
            /// </summary>
            /// <param name="c">The cell to get the column of</param>
            /// <returns>The cell column in the form of a letter</returns>
            private string GetCellColumn(Cell c)
            {
                // Taken from MSDN
                // https://docs.microsoft.com/en-us/office/open-xml/how-to-get-a-column-heading-in-a-spreadsheet
                Regex regex = new Regex("[A-Za-z]+");
                Match match = regex.Match(c.CellReference.Value);

                if (string.IsNullOrEmpty(match.Value))
                {
                    throw new SpreadsheetReaderException($"Could not decipher the column name from the following reference: \"{c.CellReference.Value}\".");
                }

                return match.Value;
            }

            /// <summary>
            /// Gets the row of a cell in the form of a number
            /// </summary>
            /// <param name="c">The cell to get the row of</param>
            /// <returns>The cell row in the form of a number</returns>
            private uint GetCellRow(Cell c)
            {
                // Taken from MSDN
                // https://docs.microsoft.com/en-us/office/open-xml/how-to-get-a-column-heading-in-a-spreadsheet
                Regex regex = new Regex(@"\d+");
                Match match = regex.Match(c.CellReference.Value);

                if (string.IsNullOrEmpty(match.Value))
                {
                    throw new SpreadsheetReaderException($"Could not decipher the column name from the following cell: \"{c.CellReference.Value}\".");
                }

                if (uint.TryParse(match.Value, out uint result))
                {
                    return result;
                }
                else
                {
                    throw new SpreadsheetReaderException($"Could not parse integer from the following cell: \"{c.CellReference.Value}\"\n" +
                        $"Matched regex: {match.Value}.");
                }
            }

            /// <summary>
            /// Gets the value of a cell, automatically referencing shared strings and formatting numbers
            /// </summary>
            /// <param name="c">The cell to get the value of</param>
            /// <returns>The value of the cell formatted</returns>
            private string GetCellValue(Cell c)
            {
                SharedStringTable sharedStrings = document.WorkbookPart.SharedStringTablePart.SharedStringTable;

                string cellText = c.CellValue?.Text;
                if (c.DataType != null && c.DataType == CellValues.SharedString)
                    return sharedStrings.ElementAt(int.Parse(cellText)).InnerText;

                if (!string.IsNullOrEmpty(cellText) && c.StyleIndex != null)
                {
                    try
                    {
                        // Try to find cell formatting information if it's a number.
                        CellFormats cellFormats = document.WorkbookPart.WorkbookStylesPart.Stylesheet.CellFormats;
                        CellFormat cellFormat = (CellFormat)cellFormats.ElementAt((int)c.StyleIndex.Value);

                        NumberingFormats numberingFormats = document.WorkbookPart.WorkbookStylesPart.Stylesheet.NumberingFormats;
                        NumberingFormat numberFormat = numberingFormats.Elements<NumberingFormat>()
                            .Where(f => f.NumberFormatId.Value == cellFormat.NumberFormatId.Value)
                            .FirstOrDefault();

                        // Try to parse to double with format.
                        if (numberFormat != default(NumberingFormat) && double.TryParse(cellText, out double conv))
                            return conv.ToString(numberFormat.FormatCode);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "Error retrieving cell format information");
                    }
                }

                return cellText;
            }

            /// <summary>
            /// Reads the next row of the Excel spreadsheet
            /// </summary>
            /// <returns>A dictionary with the column header as the key and the corresponding
            ///          row values as the value</returns>
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

            /// <summary>
            /// Disposes of open resources
            /// </summary>
            public void Dispose()
            {
                reader.Close();
            }
        }
    }
}
