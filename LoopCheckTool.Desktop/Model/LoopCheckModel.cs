using LoopCheckTool.Lib.Spreadsheet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopCheckTool.Desktop.Model
{
    public class LoopCheckModel : INotifyPropertyChanged
    {
        private bool _ignoreErrors;
        private string _inputFilePath;
        private string _outputFilePath;
        private string _selectedSheet;
        private IEnumerable<ExcelReader.Worksheet> _sheets;
        private string _templateFolderPath;

        public bool IgnoreErrors
        {
            get
            {
                return _ignoreErrors;
            }
            set
            {
                _ignoreErrors = value;
                OnPropertyChanged(nameof(IgnoreErrors));
            }
        }

        public string InputFilePath
        {
            get
            {
                return _inputFilePath;
            }
            set
            {
                bool tempAlreadySpecified = InputFileSpecified;
                _inputFilePath = value;
                OnPropertyChanged(nameof(InputFilePath));

                if (tempAlreadySpecified != InputFileSpecified)
                {
                    OnPropertyChanged(nameof(InputFileSpecified));
                }
            }
        }

        public bool InputFileSpecified
        {
            get
            {
                return !string.IsNullOrEmpty(InputFilePath) && !InputFilePath.Equals(ViewModels.LoopCheckViewModel.DEFAULT_PATH);
            }
        }

        public string OutputFilePath
        {
            get
            {
                return _outputFilePath;
            }
            set
            {
                _outputFilePath = value;
                OnPropertyChanged(nameof(OutputFilePath));
            }
        }

        public string SelectedSheet
        {
            get
            {
                return _selectedSheet;
            }
            set
            {
                _selectedSheet = value;
                OnPropertyChanged(nameof(SelectedSheet));
            }
        }

        public IEnumerable<ExcelReader.Worksheet> Sheets
        {
            get
            {
                return _sheets;
            }
            set
            {
                _sheets = value;
                OnPropertyChanged(nameof(Sheets));
            }
        }

        public string TemplateFolderPath
        {
            get
            {
                return _templateFolderPath;
            }
            set
            {
                _templateFolderPath = value;
                OnPropertyChanged(nameof(TemplateFolderPath));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
