using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopCheckTool.Wizard.ViewModels
{
    public class ExtraOptionsViewModel : WizardPageViewModel
    {
        public bool IgnoreErrors
        {
            get
            {
                return Model.IgnoreErrors;
            }
            set
            {
                Model.IgnoreErrors = value;
                OnPropertyChanged(nameof(IgnoreErrors));
            }
        }

        public ExtraOptionsViewModel(WizardPageViewModel prev) : base(prev) { }

        public override bool FinishButton_CanExecute()
        {
            return true;
        }

        public override bool NextButton_CanExecute()
        {
            return false;
        }
    }
}
