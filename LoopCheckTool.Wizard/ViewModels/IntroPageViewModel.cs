using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopCheckTool.Wizard.ViewModels
{
    public class IntroPageViewModel : WizardPageViewModel
    {
        public IntroPageViewModel() : base() { }

        public override bool FinishButton_CanExecute()
        {
            return false;
        }

        public override bool NextButton_CanExecute()
        {
            return true;
        }

        public override void NextButton_OnClicked()
        {
            Next = new InputFileViewModel(this);
        }

        public override void PrevButton_OnClicked()
        {
            // This cannot happen on this page.
        }
    }
}
