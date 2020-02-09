using CommandLine;
using LoopCheckTool.Lib.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopCheckTool.Console
{
    public class Program
    {
        private class Options
        {
            [Option('i', "inputFile", Required = true, HelpText = "The spreadsheet file to read from.")]
            public string InputFilePath { get; set; }
            [Option('s', "sheet", Required = true, HelpText = "The worksheet in the loop check list to pull data from.")]
            public string Sheet { get; set; }
        }

        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(HandleParsed);
        }

        private static void HandleParsed(Options o)
        {
            if (!string.IsNullOrEmpty(o.Sheet) && !string.IsNullOrEmpty(o.InputFilePath))
            {
                ExcelReader reader = new ExcelReader(o.InputFilePath);
                ExcelReader.Worksheet sheet = reader.GetWorksheets().Where(ws => o.Sheet.Equals(ws.Name)).FirstOrDefault();

                if (sheet != default(ExcelReader.Worksheet))
                {
                    using (ExcelReader.RowReaderContext rowReader = reader.CreateRowReader(sheet))
                    {
                        IDictionary<string, string> rows = null;
                        while ((rows = rowReader.ReadNextRow()) != null)
                        {
                            foreach (KeyValuePair<string, string> kv in rows)
                            {
                                System.Console.WriteLine($"{kv.Key}: {kv.Value}");
                            }

                            System.Console.WriteLine("---------------------------------------------------------------------------");
                        }
                    }
                }
                else
                {
                    System.Console.WriteLine("The specified sheet does not exist.");
                }
            }
            else
            {
                System.Console.WriteLine("Reached an invalid option mixture.");
            }
        }
    }
}
