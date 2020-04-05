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
        private IEnumerable<string> _headers;
        private bool _ignoreErrors;
        private string _inputFilePath;
        private string _outputFilePath;
        private string _selectedHeader;
        private ExcelReader.Worksheet _selectedSheet;
        private string _selectedSheetId;
        private IEnumerable<ExcelReader.Worksheet> _sheets;
        private string _templateFolderPath;

        public IEnumerable<string> Headers
        {
            get
            {
                return _headers;
            }
            set
            {
                _headers = value;
                OnPropertyChanged(nameof(Headers));
            }
        }

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

        public bool OutputFileSpecified
        {
            get
            {
                return !string.IsNullOrEmpty(OutputFilePath) && !OutputFilePath.Equals(ViewModels.LoopCheckViewModel.DEFAULT_PATH);
            }
        }

        public string SelectedHeader
        {
            get
            {
                return _selectedHeader;
            }
            set
            {
                _selectedHeader = value;
                OnPropertyChanged(nameof(SelectedHeader));
            }
        }

        public ExcelReader.Worksheet SelectedSheet
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

        public string SelectedSheetId
        {
            get
            {
                return _selectedSheetId;
            }
            set
            {
                _selectedSheetId = value;
                OnPropertyChanged(nameof(SelectedSheetId));
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

        public bool TemplateFolderSpecified
        {
            get
            {
                return !string.IsNullOrEmpty(TemplateFolderPath) && !TemplateFolderPath.Equals(ViewModels.LoopCheckViewModel.DEFAULT_PATH);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
