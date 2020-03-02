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
        private class Options
        {
            [Option("ignoreErrors", Required = false, HelpText = "Ignores most errors. They will be logged instead.", Default = false)]
            public bool IgnoreErrors { get; set; } = false;
            [Option('i', "inputFile", Required = true, HelpText = "The spreadsheet file to read from.")]
            public string InputFilePath { get; set; }
            [Option('o', "outputFile", Required = false, HelpText = "The path to write the file to.")]
            public string OutputFilePath { get; set; } = @".\out.docx";
            [Option('s', "sheet", Required = true, HelpText = "The worksheet in the loop check list to pull data from.")]
            public string Sheet { get; set; }
            [Option('t', "templateDir", Required = false, HelpText = "The directory to find DOCX templates in. Defaults to \"templates/\".", Default = ".\\templates")]
            public string TemplateDirectory { get; set; } = @".\templates";
            [Option('k', "templateKey", Required = false, HelpText = "The column in the IO List that will determine what template to use. Defaults to \"IO Type\".", Default = "IO Type")]
            public string TemplateKey { get; set; } = "IO Type";
        }

        public static void Main(string[] args)
        {
            try
            {
                Parser.Default.ParseArguments<Options>(args)
                    .WithParsed<Options>(HandleParsed);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Stopping due to error.");
                System.Console.WriteLine("Run with the '--ignoreErrors' flag to attempt to bypass this error.");
                System.Console.Error.WriteLine(ex.StackTrace);

                if (ex is LibraryException libEx)
                {
                    foreach (KeyValuePair<string, string> pair in libEx.RowData)
                    {
                        System.Console.Error.WriteLine($"{pair.Key}: {pair.Value}");
                    }
                }
            }

            System.Console.ReadKey();
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
                        for (int i = 0; (rows = rowReader.ReadNextRow()) != null; i++)
                        {
                            try
                            {
                                if (!rows.TryGetValue(o.TemplateKey, out string templateName))
                                {
                                    throw new LibraryException($"No value exists template key \"{o.TemplateKey}\".", i, rows);
                                }

                                if (string.IsNullOrWhiteSpace(templateName))
                                {
                                    throw new LibraryException($"Cell for \"{o.TemplateKey}\" is blank.", i, rows);
                                }

                                string templatePath = null;
                                try
                                {
                                    templatePath = Path.Combine(o.TemplateDirectory, $"{templateName}.docx");
                                }
                                catch (ArgumentException ex)
                                {
                                    throw new LibraryException("Failed to combine specified TemplateDirectory and TemplateKey. The result was an invalid path.\n" +
                                        $"Key used: \"{o.TemplateKey}\".\n" +
                                        $"Value retrieved: \"{templateName}\".",
                                        ex, i, rows);
                                }

                                if (!File.Exists(templatePath))
                                {
                                    throw new LibraryException($"Could not find template using calculated path: {templatePath}.", i, rows);
                                }

                                byte[] rawTemplate = File.ReadAllBytes(templatePath);
                                using (MemoryStream templateStream = new MemoryStream())
                                {
                                    templateStream.Write(rawTemplate, 0, rawTemplate.Length);
                                    try
                                    {
                                        writer.FillTemplate_Safe(templateStream, rows, i);
                                    }
                                    catch (WordWriter.WordWriterException ex)
                                    {
                                        throw new LibraryException("Parse error", ex, i, rows);
                                    }
                                    
                                }
                            }
                            catch (LibraryException ex)
                            {
                                System.Console.Error.WriteLine($"An error occurred while attempting to use the Loop Check Library: \"{ex.Message}\"");
                                System.Console.Error.WriteLine($"IO List row {ex.AffectedRow}.");
                                if (ex.InnerException != null)
                                {
                                    System.Console.Error.WriteLine($"The inner exception's message is: \"{ex.InnerException.Message}\"");
                                }

                                if (!o.IgnoreErrors)
                                {
                                    throw ex;
                                }
                            }
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
