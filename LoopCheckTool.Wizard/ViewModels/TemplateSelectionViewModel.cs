using LoopCheckTool.Wizard.Utilities;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LoopCheckTool.Wizard.ViewModels
{
    public class TemplateSelectionViewModel : WizardPageViewModel
    {
        #region Constants
        /// <summary>
        /// Default value for TemplateFolderPath when the user first opens this page
        /// of the wizard
        /// </summary>
        private const string DEFAULT_INPUT_FOLDER = "N/A";
        #endregion

        private string _templateFolderPath;
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
                OnPropertyChanged(nameof(TemplateFolderSpecified));
            }
        }

        /// <summary>
        /// Helper property for determining if the input
        /// file path has been specified or not
        /// </summary>
        private bool TemplateFolderSpecified
        {
            get
            {
                return !string.IsNullOrEmpty(_templateFolderPath) && !_templateFolderPath.Equals(DEFAULT_INPUT_FOLDER);
            }
        }

        public ICommand btnBrowse_OnClick { get; }

        public TemplateSelectionViewModel(WizardPageViewModel prev) : base(prev)
        {
            btnBrowse_OnClick = new CustomCommand(btnBrowse_CanExecute, btnBrowse_Execute);
            _templateFolderPath = DEFAULT_INPUT_FOLDER;
        }

        public override bool FinishButton_CanExecute()
        {
            return false;
        }

        public override bool NextButton_CanExecute()
        {
            return !string.IsNullOrEmpty(TemplateFolderPath) && !TemplateFolderPath.Equals(DEFAULT_INPUT_FOLDER);
        }

        public override void NextButton_BeforeClicked()
        {
            Model.TemplatesPath = TemplateFolderPath;
            if (Next == null)
            {
                Next = new OutputFileViewModel(this);
            }
        }

        public bool btnBrowse_CanExecute(object parameters)
        {
            return true;
        }

        public void btnBrowse_Execute(object parameters)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                InitialDirectory = Utility.GetInitialDirectory(TemplateFolderPath, TemplateFolderSpecified),
                IsFolderPicker = true,
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                TemplateFolderPath = dialog.FileName;
            }
        }
    }
}
