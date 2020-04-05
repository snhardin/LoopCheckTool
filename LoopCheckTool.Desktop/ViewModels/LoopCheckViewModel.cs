using LoopCheckTool.Desktop.Commands;
using LoopCheckTool.Desktop.Model;
using LoopCheckTool.Lib.Document;
using LoopCheckTool.Lib.Spreadsheet;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace LoopCheckTool.Desktop.ViewModels
{
    public class LoopCheckViewModel
    {
        public const string DEFAULT_PATH = "N/A";
        private ExcelReader _reader;
        public LoopCheckModel RunConfig { get; set; }
        public ICommand SelectInputFile { get; }
        public ICommand SelectOutputFile { get; }
        public ICommand SelectTemplateFolder { get; }
        public ICommand GenerateDocument { get; }
        public LoopCheckViewModel()
        {
            RunConfig = new LoopCheckModel()
            {
                InputFilePath = DEFAULT_PATH,
                OutputFilePath = DEFAULT_PATH,
                TemplateFolderPath = DEFAULT_PATH,
            };

            GenerateDocument = new CustomCommand(GenerateDocument_CanExecute, GenerateDocument_Execute);
            SelectInputFile = new CustomCommand(p => true, SelectInputFile_Execute);
            SelectOutputFile = new CustomCommand(SelectOutputFile_CanExecute, SelectOutputFile_Execute);
            SelectTemplateFolder = new CustomCommand(SelectTemplateFolder_CanExecute, SelectTemplateFolder_Execute);
        }

        public void LoadNewHeaders()
        {
            try
            {
                RunConfig.Headers = _reader.GetHeader(RunConfig.SelectedSheet);
                RunConfig.SelectedHeader = RunConfig.Headers.FirstOrDefault();
            }
            catch (ExcelReader.ExcelReaderException)
            {
                RunConfig.Headers = new string[0];
                RunConfig.SelectedHeader = null;
                MessageBox.Show("No headers were found in the selected worksheet.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetInitialDirectory(string directory, bool condition)
        {
            try
            {
                if (condition)
                {
                    return Path.GetDirectoryName(directory);
                }
            }
            catch
            {
                // Swallow the error.
            }

            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private void SelectInputFile_Execute(object parameters)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                Filter = "All Files (*.*)|*.*",
                InitialDirectory = GetInitialDirectory(RunConfig.InputFilePath, RunConfig.InputFileSpecified),
                Multiselect = false,
            };

            if (dialog.ShowDialog() == true)
            {
                ExcelReader reader = new ExcelReader(dialog.FileName);
                IEnumerable<ExcelReader.Worksheet> sheets = reader.GetWorksheets();
                if (sheets.Count() > 0)
                {
                    _reader = reader;
                    RunConfig.InputFilePath = dialog.FileName;
                    RunConfig.Sheets = sheets;
                    RunConfig.SelectedSheetId = RunConfig.Sheets.First().ID;
                    LoadNewHeaders();
                }
                else
                {
                    MessageBox.Show("There are no sheets present in this spreadsheet", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool SelectOutputFile_CanExecute(object parameters)
        {
            return RunConfig.InputFileSpecified;
        }

        private void SelectOutputFile_Execute(object parameters)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                CheckFileExists = false,
                CheckPathExists = true,
                Filter = "Microsoft Word Documents (*.docx)|*.docx|All Files (*.*)|*.*",
                InitialDirectory = GetInitialDirectory(RunConfig.OutputFilePath, RunConfig.OutputFileSpecified),
                Multiselect = false,
            };

            if (dialog.ShowDialog() == true)
            {
                RunConfig.OutputFilePath = dialog.FileName;
            }
        }

        private bool SelectTemplateFolder_CanExecute(object parameters)
        {
            return RunConfig.InputFileSpecified;
        }

        private void SelectTemplateFolder_Execute(object parameters)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                InitialDirectory = GetInitialDirectory(RunConfig.TemplateFolderPath, RunConfig.TemplateFolderSpecified),
                IsFolderPicker = true,
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                RunConfig.TemplateFolderPath = dialog.FileName;
            }
        }

        private bool GenerateDocument_CanExecute(object parameters)
        {
            return RunConfig.InputFileSpecified && RunConfig.OutputFileSpecified && RunConfig.TemplateFolderSpecified;
        }

        private void GenerateDocument_Execute(object parameters)
        {
            using (ExcelReader.RowReaderContext rowReader = _reader.CreateRowReader(RunConfig.SelectedSheet))
            {
                uint errors = 0;
                WordWriter writer = new WordWriter();
                IDictionary<string, string> rows = null;
                for (int i = 0; (rows = rowReader.ReadNextRow()) != null; i++)
                {
                    try
                    {
                        if (!rows.TryGetValue(RunConfig.SelectedHeader, out string templateName))
                        {
                            throw new LibraryException($"No value exists for template key \"{RunConfig.SelectedHeader}\".", i, rows);
                        }

                        if (string.IsNullOrWhiteSpace(templateName))
                        {
                            throw new LibraryException($"Cell for \"{RunConfig.SelectedHeader}\" is blank.", i, rows);
                        }

                        string templatePath = null;
                        try
                        {
                            templatePath = Path.Combine(RunConfig.TemplateFolderPath, $"{templateName}.docx");
                        }
                        catch (ArgumentException ex)
                        {
                            throw new LibraryException("Failed to combine specified TemplateDirectory and TemplateKey. The result was an invalid path.\n" +
                                $"Key used: \"{RunConfig.SelectedHeader}\".\n" +
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
                        if (!RunConfig.IgnoreErrors)
                        {
                            string msg = $"An error occurred while attempting to use the Loop Check Library: \"{ex.Message}\"\n" +
                                $"IO List row {ex.AffectedRow}.";
                            if (ex.InnerException != null)
                            {
                                msg += $"\nThe inner exception's message is: \"{ex.InnerException.Message}\"";
                            }

                            msg += "\nDo you wish to continue execution? This row will be ignored.";

                            MessageBoxResult result = MessageBox.Show(msg, "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);

                            if (result == MessageBoxResult.No)
                            {
                                return;
                            }
                        }
                        else
                        {
                            // TODO: Maybe log them somewhere?
                        }

                        errors++;
                    }
                }

                using (MemoryStream export = writer.ExportDocument())
                {
                    File.WriteAllBytes(RunConfig.OutputFilePath, export.ToArray());
                }

                MessageBox.Show($"Document generation finished! {errors} errors occurred.", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
