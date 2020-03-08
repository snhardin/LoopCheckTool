using LoopCheckTool.Desktop.Commands;
using LoopCheckTool.Desktop.Model;
using LoopCheckTool.Lib.Spreadsheet;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public LoopCheckViewModel()
        {
            RunConfig = new LoopCheckModel()
            {
                InputFilePath = DEFAULT_PATH,
                OutputFilePath = DEFAULT_PATH,
                TemplateFolderPath = DEFAULT_PATH,
            };

            SelectInputFile = new CustomCommand(p => true, SelectInputFile_Execute);
            SelectOutputFile = new CustomCommand(SelectOutputFile_CanExecute, SelectOutputFile_Execute);
            SelectTemplateFolder = new CustomCommand(SelectTemplateFile_CanExecute, SelectTemplateFile_Execute);
        }

        private void SelectInputFile_Execute(object parameters)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                Filter = "All Files (*.*)|*.*",
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
                    RunConfig.SelectedSheet = RunConfig.Sheets.First().ID;
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
                Filter = "All Files (*.*)|*.*",
                Multiselect = false,
            };

            if (dialog.ShowDialog() == true)
            {
                RunConfig.OutputFilePath = dialog.FileName;
            }
        }

        private bool SelectTemplateFile_CanExecute(object parameters)
        {
            return RunConfig.InputFileSpecified;
        }

        private void SelectTemplateFile_Execute(object parameters)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = "All Files (*.*)|*.*",
                Multiselect = false,
            };

            if (dialog.ShowDialog() == true)
            {
                RunConfig.TemplateFolderPath = dialog.FileName;
            }
        }
    }
}
