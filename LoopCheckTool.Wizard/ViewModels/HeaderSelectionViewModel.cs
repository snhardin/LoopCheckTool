using LoopCheckTool.Lib.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopCheckTool.Wizard.ViewModels
{
    public class HeaderSelectionViewModel : WizardPageViewModel
    {
        private string _selectedHeader;

        public IEnumerable<string> Headers { get; }
        public ExcelReader Reader { get; }
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

        public HeaderSelectionViewModel(WizardPageViewModel prev) : base(prev)
        {
            if (prev is InputFileViewModel vm)
            {
                Headers = vm.Headers;
            }
        }

        public override bool FinishButton_CanExecute()
        {
            return false;
        }

        public override bool NextButton_CanExecute()
        {
            return !string.IsNullOrEmpty(SelectedHeader);
        }

        public override void NextButton_OnClicked()
        {
            // Commit data to the model
            Model.Header = SelectedHeader;
            Next = new TemplateSelectionViewModel(this);
        }

        public override void PrevButton_OnClicked()
        {
            // Do nothing.
        }
    }
}
