using LoopCheckTool.Lib.Document;
using LoopCheckTool.Lib.Spreadsheet;
using LoopCheckTool.Wizard.Models;
using LoopCheckTool.Wizard.Services;
using LoopCheckTool.Wizard.Utilities;
using LoopCheckTool.Wizard.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LoopCheckTool.Wizard.ViewModels
{
    public class AppViewModel : OnPropertyChangedNotifier
    {
        private WizardPageViewModel _currentView;
        private AutoResetEvent _mutex;
        private BackgroundWorker _worker;

        private readonly IAboutDialogService AboutService;
        private readonly ILoadingDialogService LoadingService;

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
        public ICommand AboutCommand { get; }

        public AppViewModel(ILoadingDialogService loadingService, IAboutDialogService aboutService)
        {
            AboutService = aboutService;
            LoadingService = loadingService;

            _currentView = new IntroPageViewModel();
            _mutex = new AutoResetEvent(false);
            _worker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true,
            };

            _worker.DoWork += BackgroundWorker_DoWork;
            _worker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
            _worker.ProgressChanged += BackgroundWorker_ProgressChanged;

            AboutCommand = new CustomCommand(AboutCommand_CanExecute, AboutCommand_Execute);
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
            CurrentView.FinishButton_BeforeClicked();
            DocumentGenerationModel model = CurrentView.Model;

            _worker.RunWorkerAsync(model);
            LoadingService.OpenLoadingDialog();
        }

        private bool NextStep_CanExecute(object parameters)
        {
            return CurrentView.NextButton_CanExecute();
        }

        private void NextStep_Execute(object parameters)
        {
            CurrentView.NextButton_BeforeClicked();
            CurrentView = CurrentView.Next;
            CurrentView.OnNavigateFromNextButton();
        }

        private bool PrevStep_CanExecute(object parameters)
        {
            return CurrentView.Prev != null;
        }

        private void PrevStep_Execute(object parameters)
        {
            CurrentView.PrevButton_BeforeClicked();
            CurrentView = CurrentView.Prev;
            CurrentView.OnNavigateFromPrevButton();
        }

        private bool AboutCommand_CanExecute(object parameters)
        {
            return true;
        }

        private void AboutCommand_Execute(object parameters)
        {
            AboutService.OpenAboutDialog();
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            DocumentGenerationModel model = (DocumentGenerationModel)e.Argument;

            using (StreamWriter logger = File.CreateText(model.OutputPath + ".log"))
            using (ExcelReader.RowReader rowReader = model.Reader.CreateRowReader(model.Sheet))
            {
                uint errors = 0;
                WordWriter writer = new WordWriter();
                IDictionary<string, string> rows = null;
                for (int i = 0; (rows = rowReader.ReadNextRow()) != null; i++)
                {
                    try
                    {
                        LoadingService.SetLoadingText($"Processing row {i}...");
                        if (!rows.TryGetValue(model.Header, out string templateName))
                        {
                            throw new LibraryException($"No value exists for template key \"{model.Header}\"", i, rows);
                        }

                        if (string.IsNullOrWhiteSpace(templateName))
                        {
                            throw new LibraryException($"Cell for \"{model.Header}\" is blank", i, rows);
                        }

                        string templatePath = null;
                        try
                        {
                            templatePath = Path.Combine(model.TemplatesPath, $"{templateName}.docx");
                        }
                        catch (ArgumentException ex)
                        {
                            throw new LibraryException("Failed to combine specified TemplateDirectory and TemplateKey. The result was an invalid path\n" +
                                $"Key used: \"{model.Header}\"\n" +
                                $"Value retrieved: \"{templateName}\"",
                                ex, i, rows);
                        }

                        if (!File.Exists(templatePath))
                        {
                            throw new LibraryException($"Could not find template using calculated path: {templatePath}", i, rows);
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
                        logger.WriteLine($"A row was skipped while processing: {ex.ToString()}");

                        if (!model.IgnoreErrors)
                        {
                            string msg = $"An error occurred while attempting to use the Loop Check Library: \"{ex.Message}\"";
                            if (ex.InnerException != null)
                            {
                                msg += $"\nThe inner exception's message is: \"{ex.InnerException.Message}\"";
                            }

                            msg += "\nDo you wish to continue execution? This row will be ignored.";

                            _worker.ReportProgress(0, new DocumentGenerationState(msg, DocumentGenerationStateType.BlockingMessage));
                            _mutex.WaitOne();

                            if (_worker.CancellationPending)
                            {
                                e.Cancel = true;
                                return;
                            }
                        }

                        errors++;
                    }
                }

                LoadingService.SetLoadingText("Writing file to disk...");
                using (MemoryStream export = writer.ExportDocument())
                {
                    File.WriteAllBytes(model.OutputPath, export.ToArray());
                }

                e.Result = errors;
            }
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LoadingService.CloseLoadingDialog();

            if (e.Error != null)
            {
                throw e.Error;
            }

            if (!e.Cancelled)
            {
                uint errors = (uint)e.Result;
                MessageBox.Show($"Document generation complete! {errors} errors occurred.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            App.Current.MainWindow.Close();
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            DocumentGenerationState state = (DocumentGenerationState)e.UserState;
            switch (state.Type)
            {
                case DocumentGenerationStateType.BlockingMessage:
                    MessageBoxResult result = MessageBox.Show(state.Content, "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (result != MessageBoxResult.Yes)
                    {
                        _worker.CancelAsync();
                    }
                    _mutex.Set();
                    break;
                case DocumentGenerationStateType.Progress:
                    // Do nothing.
                    break;
            }
        }

        private class DocumentGenerationState
        {
            public string Content { get; }
            public DocumentGenerationStateType Type { get; }

            public DocumentGenerationState(string content, DocumentGenerationStateType type)
            {
                Content = content;
                Type = type;
            }
        }

        private enum DocumentGenerationStateType
        {
            BlockingMessage,
            Progress,
        }
    }
}
