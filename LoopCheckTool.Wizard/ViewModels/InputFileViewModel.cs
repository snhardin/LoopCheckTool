using LoopCheckTool.Lib.Spreadsheet;
using LoopCheckTool.Wizard.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LoopCheckTool.Wizard.ViewModels
{
    public class InputFileViewModel : WizardPageViewModel
    {
        #region Constants
        /// <summary>
        /// Default value for InputFileName when the user first opens this page
        /// of the wizard
        /// </summary>
        private const string DEFAULT_INPUT_FILE = "N/A";
        #endregion

        #region Private Fields
        /// <summary>
        /// Stores the input file path
        /// </summary>
        private string _inputFileName;

        /// <summary>
        /// The sheet currently selected
        /// </summary>
        private ExcelReader.Worksheet _selectedSheet;

        /// <summary>
        /// Stores the sheets available from the input file
        /// </summary>
        private IEnumerable<ExcelReader.Worksheet> _sheets;
        #endregion

        #region Properties
        /// <summary>
        /// Helper property for determining if there are any sheets.
        /// </summary>
        public bool HasSheets
        {
            get
            {
                return Sheets?.Count() > 0;
            }
        }

        /// <summary>
        /// Stores the headers retrieved from the current sheet.
        /// </summary>
        public IEnumerable<string> Headers { get; private set; }

        /// <summary>
        /// Bindable property for the input file path
        /// </summary>
        public string InputFileName
        {
            get
            {
                return _inputFileName;
            }
            set
            {
                _inputFileName = value;
                OnPropertyChanged(nameof(InputFileName));
            }
        }

        /// <summary>
        /// Bindable property for the sheet selected. Does error checking
        /// and sets headers.
        /// </summary>
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
                LoadHeaders();
            }
        }

        /// <summary>
        /// Bindable property for sheets available from the
        /// selected file path
        /// </summary>
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
                OnPropertyChanged(nameof(HasSheets));
            }
        }

        /// <summary>
        /// Helper property for determining if the input
        /// file path has been specified or not
        /// </summary>
        private bool InputFileSpecified
        {
            get
            {
                return !string.IsNullOrEmpty(_inputFileName) && !_inputFileName.Equals(DEFAULT_INPUT_FILE);
            }
        }

        /// <summary>
        /// Stores the concrete instance of the file reader from
        /// the Loop Check library
        /// </summary>
        public ExcelReader Reader { get; private set; }

        /// <summary>
        /// Bindable command for when the Browse button is clicked for
        /// the input file path modal
        /// </summary>
        public ICommand btnBrowse_OnClick { get; }
        #endregion

        public InputFileViewModel(WizardPageViewModel prev) : base(prev)
        {
            ResetToDefaults();
            btnBrowse_OnClick = new CustomCommand(btnBrowse_CanExecute, btnBrowse_Execute);
        }

        private void ResetToDefaults()
        {
            _inputFileName = DEFAULT_INPUT_FILE;
            Reader = null;
            SelectedSheet = null;
            Sheets = null;
            Next = null;
        }

        public override bool FinishButton_CanExecute()
        {
            return false;
        }

        public override bool NextButton_CanExecute()
        {
            return InputFileSpecified && SelectedSheet != null && Headers?.Count() > 0;
        }

        public override void OnNavigateFromPrevButton()
        {
            ResetToDefaults();
        }

        public bool btnBrowse_CanExecute(object parameters)
        {
            return true;
        }

        public void btnBrowse_Execute(object parameters)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                Filter = "All Files (*.*)|*.*",
                InitialDirectory = GetInitialDirectory(InputFileName),
                Multiselect = false,
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    ExcelReader reader = new ExcelReader(dialog.FileName);
                    IEnumerable<ExcelReader.Worksheet> sheets = reader.GetWorksheets();
                    if (sheets.Count() > 0)
                    {
                        Reader = reader;
                        InputFileName = dialog.FileName;
                        Sheets = sheets;
                    }
                    else
                    {
                        MessageBox.Show("Excel workbook has no sheets!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error while opening file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Gets the initial directory for the browse modal. If it
        /// can't figure it out, it uses My Documents by default.
        /// </summary>
        /// <param name="directory">The initial directory to try</param>
        /// <returns>The initial directory to set for the browse modal</returns>
        private string GetInitialDirectory(string directory)
        {
            try
            {
                if (InputFileSpecified)
                {
                    return Path.GetDirectoryName(directory);
                }
            }
            catch
            {
                // Swallow the error. We're just going to use the default.
            }

            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private void LoadHeaders()
        {
            if (Reader != null && SelectedSheet != null)
            {
                try
                {
                    // ToList() forces evaluation.
                    Headers = Reader.GetHeader(SelectedSheet).ToList();
                }
                catch (ExcelReader.ExcelReaderException)
                {
                    Headers = null;
                    MessageBox.Show("No headers were found in the selected worksheet.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public override void NextButton_OnClicked()
        {
            // Commit data to Model when advancing to the next page.
            Model.Reader = Reader;
            Model.Sheet = SelectedSheet;
            Next = new HeaderSelectionViewModel(this);
        }

        public override void PrevButton_OnClicked()
        {
            ResetToDefaults();
        }
    }
}
