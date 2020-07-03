using CommandLine;
using LoopCheckTool.Console.Verbs;
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
        /// <summary>
        /// Main method of console application.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Return code</returns>
        public static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<ListSheetsVerb, GenerateVerb>(args)
                .MapResult((IVerb v) => v.Execute(), errors => 1);
        }

        //private static void HandleParsed(Options o)
        //{
        //    if (!string.IsNullOrEmpty(o.Sheet) && !string.IsNullOrEmpty(o.InputFilePath))
        //    {
        //        ExcelReader reader = new ExcelReader(o.InputFilePath);
        //        ExcelReader.Worksheet sheet = reader.GetWorksheets().Where(ws => o.Sheet.Equals(ws.Name)).FirstOrDefault();

        //        if (sheet != default(ExcelReader.Worksheet))
        //        {
        //            using (ExcelReader.RowReader rowReader = reader.CreateRowReader(sheet))
        //            {
        //                WordWriter writer = new WordWriter();
        //                IDictionary<string, string> rows = null;
        //                for (int i = 0; (rows = rowReader.ReadNextRow()) != null; i++)
        //                {
        //                    try
        //                    {
        //                        if (!rows.TryGetValue(o.TemplateKey, out string templateName))
        //                        {
        //                            throw new LibraryException($"No value exists template key \"{o.TemplateKey}\".", i, rows);
        //                        }

        //                        if (string.IsNullOrWhiteSpace(templateName))
        //                        {
        //                            throw new LibraryException($"Cell for \"{o.TemplateKey}\" is blank.", i, rows);
        //                        }

        //                        string templatePath = null;
        //                        try
        //                        {
        //                            templatePath = Path.Combine(o.TemplateDirectory, $"{templateName}.docx");
        //                        }
        //                        catch (ArgumentException ex)
        //                        {
        //                            throw new LibraryException("Failed to combine specified TemplateDirectory and TemplateKey. The result was an invalid path.\n" +
        //                                $"Key used: \"{o.TemplateKey}\".\n" +
        //                                $"Value retrieved: \"{templateName}\".",
        //                                ex, i, rows);
        //                        }

        //                        if (!File.Exists(templatePath))
        //                        {
        //                            throw new LibraryException($"Could not find template using calculated path: {templatePath}.", i, rows);
        //                        }

        //                        byte[] rawTemplate = File.ReadAllBytes(templatePath);
        //                        using (MemoryStream templateStream = new MemoryStream())
        //                        {
        //                            templateStream.Write(rawTemplate, 0, rawTemplate.Length);
        //                            try
        //                            {
        //                                writer.GenerateAndAppendTemplate(templateStream, rows, i);
        //                            }
        //                            catch (DocumentWriterException ex)
        //                            {
        //                                throw new LibraryException("Parse error", ex, i, rows);
        //                            }
                                    
        //                        }
        //                    }
        //                    catch (LibraryException ex)
        //                    {
        //                        System.Console.Error.WriteLine($"An error occurred while attempting to use the Loop Check Library: \"{ex.Message}\"");
        //                        System.Console.Error.WriteLine($"IO List row {ex.AffectedRow}.");
        //                        if (ex.InnerException != null)
        //                        {
        //                            System.Console.Error.WriteLine($"The inner exception's message is: \"{ex.InnerException.Message}\"");
        //                        }

        //                        if (!o.IgnoreErrors)
        //                        {
        //                            throw ex;
        //                        }
        //                    }
        //                }

        //                using (MemoryStream export = writer.ExportDocument())
        //                {
        //                    File.WriteAllBytes(o.OutputFilePath, export.ToArray());
        //                }
        //            }
        //        }
        //        else
        //        {
        //            System.Console.WriteLine("The specified sheet does not exist.");
        //        }
        //    }
        //    else
        //    {
        //        System.Console.WriteLine("Reached an invalid option mixture.");
        //    }
        //}
    }
}
