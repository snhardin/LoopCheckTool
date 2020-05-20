using LoopCheckTool.Lib.Document;
using LoopCheckTool.Lib.Spreadsheet;
using LoopCheckTool.Wizard.Models;
using LoopCheckTool.Wizard.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LoopCheckTool.Wizard.ViewModels
{
    public class AppViewModel : OnPropertyChangedNotifier
    {
        private const short NUM_OUTPUT_TRIES = 3;

        private WizardPageViewModel _currentView;

        public WizardPageViewModel CurrentView
        {
            get
            {
                return _currentView;
            }
            private set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        public ICommand FinishStep { get; }
        public ICommand NextStep { get; }
        public ICommand PrevStep { get; }

        public AppViewModel()
        {
            _currentView = new IntroPageViewModel();

            FinishStep = new CustomCommand(FinishStep_CanExecute, FinishStep_Execute);
            NextStep = new CustomCommand(NextStep_CanExecute, NextStep_Execute);
            PrevStep = new CustomCommand(PrevStep_CanExecute, PrevStep_Execute);
        }

        private bool FinishStep_CanExecute(object parameters)
        {
            return CurrentView.FinishButton_CanExecute();
        }

        private void FinishStep_Execute(object parameters)
        {
            DocumentGenerationModel model = CurrentView.FinishButton_OnClicked();

            using (ExcelReader.RowReaderContext rowReader = model.Reader.CreateRowReader(model.Sheet))
            {
                uint errors = 0;
                WordWriter writer = new WordWriter();
                IDictionary<string, string> rows = null;
                for (int i = 0; (rows = rowReader.ReadNextRow()) != null; i++)
                {
                    try
                    {
                        if (!rows.TryGetValue(model.Header, out string templateName))
                        {
                            throw new LibraryException($"No value exists for template key \"{model.Header}\".", i, rows);
                        }

                        if (string.IsNullOrWhiteSpace(templateName))
                        {
                            throw new LibraryException($"Cell for \"{model.Header}\" is blank.", i, rows);
                        }

                        string templatePath = null;
                        try
                        {
                            templatePath = Path.Combine(model.TemplatesPath, $"{templateName}.docx");
                        }
                        catch (ArgumentException ex)
                        {
                            throw new LibraryException("Failed to combine specified TemplateDirectory and TemplateKey. The result was an invalid path.\n" +
                                $"Key used: \"{model.Header}\".\n" +
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
                        if (!model.IgnoreErrors)
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
                    OpenFileDialog dialog = new OpenFileDialog()
                    {
                        CheckFileExists = false,
                        CheckPathExists = true,
                        Filter = "Microsoft Word Documents (*.docx)|*.docx|All Files (*.*)|*.*",
                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        Multiselect = false,
                    };

                    short attempts;
                    bool? lastResult;
                    for (attempts = 0, lastResult = dialog.ShowDialog(); attempts < NUM_OUTPUT_TRIES && lastResult != true; attempts++) { }
                    
                    if (lastResult == true)
                    {
                        File.WriteAllBytes(dialog.FileName, export.ToArray());
                        MessageBox.Show($"Document generation finished! {errors} errors occurred.", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Failed to select output file. I give up!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            if (parameters is Window window)
            {
                window.Close();
            }
        }

        private bool NextStep_CanExecute(object parameters)
        {
            return CurrentView.NextButton_CanExecute();
        }

        private void NextStep_Execute(object parameters)
        {
            CurrentView.NextButton_OnClicked();
            CurrentView = CurrentView.Next;
            CurrentView.OnNavigateFromNextButton();
        }

        private bool PrevStep_CanExecute(object parameters)
        {
            return CurrentView.Prev != null;
        }

        private void PrevStep_Execute(object parameters)
        {
            CurrentView.PrevButton_OnClicked();
            CurrentView = CurrentView.Prev;
            CurrentView.OnNavigateFromPrevButton();
        }
    }
}
