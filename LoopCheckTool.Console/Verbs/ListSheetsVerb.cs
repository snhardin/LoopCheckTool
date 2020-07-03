using CommandLine;
using CommandLine.Text;
using LoopCheckTool.Lib.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LoopCheckTool.Console.Verbs
{
    /// <summary>
    /// Handles list sheet functionality of command line utility.
    /// </summary>
    [Verb("list-sheets", false, HelpText =  "Lists all sheets available in the given Spreadsheet.")]
    class ListSheetsVerb : IVerb
    {
        /// <summary>
        /// Spreadsheet to open.
        /// </summary>
        [Value(0, HelpText = "The file to list sheets for.", MetaName = "File Name", Required = true)]
        public string FileName { get; set; }

        /// <summary>
        /// All usage examples of the list-sheets verb.
        /// </summary>
        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("List all of the sheets of a Spreadsheet", new ListSheetsVerb { FileName = "sheet.xlsx" });
            }
        }

        /// <summary>
        /// Lists all sheets of a Spreadsheet file.
        /// </summary>
        /// <returns></returns>
        public int Execute()
        {
            ExcelReader reader;
            try
            {
                reader = new ExcelReader(FileName);
            }
            catch
            {
                System.Console.WriteLine("An error occurred while reading the input file. Is it an actual Excel file?");
                return 1;
            }

            IEnumerable<ExcelReader.Worksheet> sheets = reader.GetWorksheets();
            System.Console.WriteLine($"The following sheets are present in the {Path.GetFileName(FileName)} file:");
            foreach (ExcelReader.Worksheet worksheet in sheets)
            {
                System.Console.WriteLine($"ID: {worksheet.ID}, Name: {worksheet.Name}");
            }

            return 0;
        }
    }
}
