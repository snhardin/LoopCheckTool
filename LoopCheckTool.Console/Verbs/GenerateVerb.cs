using CommandLine;
using CommandLine.Text;
using LoopCheckTool.Lib.Document;
using LoopCheckTool.Lib.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LoopCheckTool.Console.Verbs
{
    /// <summary>
    /// Handle generate document functionality of command line utility.
    /// </summary>
    [Verb("generate", false, HelpText = "Generates a Document from the provided Spreadsheet.")]
    class GenerateVerb : IVerb
    {
        /// <summary>
        /// Option to silently ignore errors that are not fatal.
        /// </summary>
        [Option("ignoreErrors", Required = false, HelpText = "Ignores most errors. They will be logged instead.", Default = false)]
        public bool IgnoreErrors { get; set; } = false;

        /// <summary>
        /// The path to the input IO List to use to generate the document.
        /// </summary>
        [Option('i', "inputFile", Required = true, HelpText = "The spreadsheet file to read from.")]
        public string InputFilePath { get; set; }

        /// <summary>
        /// The path to write the output generated document.
        /// </summary>
        [Option('o', "outputFile", Required = false, HelpText = "The path to write the generated file to.", Default = "out.docx")]
        public string OutputFilePath { get; set; } = @"out.docx";

        /// <summary>
        /// The sheet to use to generate the document.
        /// </summary>
        [Option('s', "sheet", Required = true, HelpText = "The worksheet in the loop check list to pull data from.")]
        public string Sheet { get; set; }

        /// <summary>
        /// The path to the directory containing all templates files for document generation.
        /// </summary>
        [Option('t', "templateDir", Required = false, HelpText = "The directory to find DOCX templates in.", Default = "templates")]
        public string TemplateDirectory { get; set; } = @"templates";

        /// <summary>
        /// The column to use in the IO List to determine the template to use for a row.
        /// </summary>
        [Option('k', "templateKey", Required = false, HelpText = "The column in the IO List that will determine what template to use.", Default = "IO Type")]
        public string TemplateKey { get; set; } = "IO Type";

        /// <summary>
        /// Examples to be displayed in the command help.
        /// </summary>
        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Generates a Loop Check document using the sheet \"Sheet1\"",
                    new UnParserSettings() { SkipDefault = true },
                    new GenerateVerb() { InputFilePath = "IO List.xlsx", Sheet = "Sheet1" });
                yield return new Example("Generates a document at a specific path",
                    new UnParserSettings() { SkipDefault = true },
                    new GenerateVerb() { InputFilePath = "IO List.xlsx", OutputFilePath = "My New Document.docx", Sheet = "Sheet1" });
                yield return new Example("Generates a document, ignoring non-fatal errors",
                    new UnParserSettings() { SkipDefault = true },
                    new GenerateVerb() { InputFilePath = "IO List.xlsx", Sheet = "East Side Plant", IgnoreErrors = true });
                yield return new Example("Generates a document with fully-specified, specific parameters",
                    new GenerateVerb() { InputFilePath = "IO List.xlsx", OutputFilePath = "Document.docx", Sheet = "East Side Plant", TemplateDirectory = "../templates/" });
            }
        }

        /// <summary>
        /// Generates a document via LoopCheckTool.Lib.
        /// </summary>
        /// <returns>Return code</returns>
        public int Execute()
        {
            try
            {
                ExcelReader reader = new ExcelReader(InputFilePath);
                ExcelReader.Worksheet sheet = reader.GetWorksheets().Where(ws => Sheet.Equals(ws.Name)).FirstOrDefault();

                if (sheet != default(ExcelReader.Worksheet))
                {
                    using (ExcelReader.RowReader rowReader = reader.CreateRowReader(sheet))
                    {
                        WordWriter writer = new WordWriter();
                        IDictionary<string, string> rows = null;
                        for (int i = 0; (rows = rowReader.ReadNextRow()) != null; i++)
                        {
                            try
                            {
                                if (!rows.TryGetValue(TemplateKey, out string templateName))
                                {
                                    throw new LibraryException($"No value exists template key \"{TemplateKey}\".", i, rows);
                                }

                                if (string.IsNullOrWhiteSpace(templateName))
                                {
                                    throw new LibraryException($"Cell for \"{TemplateKey}\" is blank.", i, rows);
                                }

                                string templatePath = null;
                                try
                                {
                                    templatePath = Path.Combine(TemplateDirectory, $"{templateName}.docx");
                                }
                                catch (ArgumentException ex)
                                {
                                    throw new LibraryException("Failed to combine specified TemplateDirectory and TemplateKey. The result was an invalid path.\n" +
                                        $"Key used: \"{TemplateKey}\".\n" +
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
                                        writer.GenerateAndAppendTemplate(templateStream, rows, i);
                                    }
                                    catch (DocumentWriterException ex)
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

                                if (!IgnoreErrors)
                                {
                                    throw ex;
                                }
                            }
                        }

                        using (MemoryStream export = writer.ExportDocument())
                        {
                            File.WriteAllBytes(OutputFilePath, export.ToArray());
                        }
                    }
                }
                else
                {
                    System.Console.WriteLine("The specified sheet does not exist.");

                    return 1;
                }
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

                return 1;
            }

            return 0;
        }
    }
}
