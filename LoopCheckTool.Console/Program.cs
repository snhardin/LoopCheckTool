using CommandLine;
using LoopCheckTool.Lib.Document;
using LoopCheckTool.Lib.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopCheckTool.Console
{
    public class Program
    {
        private const string TEMPLATE_FILE = @".\testdoc.docx";
        private class Options
        {
            [Option('i', "inputFile", Required = true, HelpText = "The spreadsheet file to read from.")]
            public string InputFilePath { get; set; }
            [Option('o', "outputFile", Required = true, HelpText = "The path to write the file to.")]
            public string OutputFilePath { get; set; }
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
                        WordWriter writer = new WordWriter();
                        IDictionary<string, string> rows = null;
                        int i = 0;
                        while ((rows = rowReader.ReadNextRow()) != null)
                        {
                            byte[] rawTemplate = File.ReadAllBytes(TEMPLATE_FILE);

                            using (MemoryStream templateStream = new MemoryStream())
                            {
                                templateStream.Write(rawTemplate, 0, rawTemplate.Length);
                                writer.FillTemplate_Safe(templateStream, rows, i);
                            }

                            i++;
                        }

                        MemoryStream export = writer.ExportDocument();
                        File.WriteAllBytes(o.OutputFilePath, export.ToArray());
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
