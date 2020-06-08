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
    public class OutputFileViewModel : WizardPageViewModel
    {
        private const string DEFAULT_OUTPUT_FILE = "N/A";
        private string _outputFileName;

        public string OutputFileName
        {
            get
            {
                return _outputFileName;
            }
            set
            {
                _outputFileName = value;
                OnPropertyChanged(nameof(OutputFileName));
            }
        }

        private bool OutputFileSpecified
        {
            get
            {
                return !string.IsNullOrEmpty(_outputFileName) && !_outputFileName.Equals(DEFAULT_OUTPUT_FILE);
            }
        }

        public ICommand btnBrowse_OnClick { get; }

        public OutputFileViewModel(WizardPageViewModel prev) : base(prev)
        {
            _outputFileName = DEFAULT_OUTPUT_FILE;
            btnBrowse_OnClick = new CustomCommand(btnBrowse_CanExecute, btnBrowse_Execute);
        }

        public override bool FinishButton_CanExecute()
        {
            // Use the same logic as the 'Next' button.
            return NextButton_CanExecute();
        }

        public override bool NextButton_CanExecute()
        {
            return OutputFileSpecified;
        }

        public override void NextButton_BeforeClicked()
        {
            Model.OutputPath = OutputFileName;
            if (Next == null)
            {
                Next = new ExtraOptionsViewModel(this);
            }
        }

        public bool btnBrowse_CanExecute(object parameters)
        {
            return true;
        }

        public void btnBrowse_Execute(object parameters)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                CheckFileExists = false,
                CheckPathExists = true,
                Filter = "Microsoft Word Documents (*.docx)|*.docx|All Files (*.*)|*.*",
                InitialDirectory = Utility.GetInitialDirectory(OutputFileName, OutputFileSpecified),
                Multiselect = false,
            };

            if (dialog.ShowDialog() == true)
            {
                if (File.Exists(dialog.FileName))
                {
                    MessageBoxResult result = MessageBox.Show("This file already exists. Do you wish to overwrite it?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }

                OutputFileName = dialog.FileName;
            }
        }
    }
}
