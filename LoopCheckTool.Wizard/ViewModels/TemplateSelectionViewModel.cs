using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            }
        }

        public TemplateSelectionViewModel(WizardPageViewModel prev) : base(prev)
        {
            _templateFolderPath = DEFAULT_INPUT_FOLDER;
            Next = new ExtraOptionsViewModel(this);
        }

        public override bool FinishButton_CanExecute()
        {
            // Use the same logic as the next button.
            return NextButton_CanExecute();
        }

        public override bool NextButton_CanExecute()
        {
            return !string.IsNullOrEmpty(TemplateFolderPath) && !TemplateFolderPath.Equals(DEFAULT_INPUT_FOLDER);
        }

        public override void NextButton_OnClicked()
        {
            Model.TemplatesPath = TemplateFolderPath;
        }

        public override void PrevButton_OnClicked()
        {
            // Do nothing.
        }
    }
}
