using LoopCheckTool.Wizard.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LoopCheckTool.Wizard.ViewModels
{
    public class AppViewModel : OnPropertyChangedNotifier
    {
        private WizardPageViewModel _currentView;

        public WizardPageViewModel CurrentView
        {
            get
            {
                return _currentView;
            }
            private set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        public ICommand FinishStep { get; }
        public ICommand NextStep { get; }
        public ICommand PrevStep { get; }

        public AppViewModel()
        {
            _currentView = new IntroPageViewModel();

            FinishStep = new CustomCommand(FinishStep_CanExecute, FinishStep_Execute);
            NextStep = new CustomCommand(NextStep_CanExecute, NextStep_Execute);
            PrevStep = new CustomCommand(PrevStep_CanExecute, PrevStep_Execute);
        }

        private bool FinishStep_CanExecute(object parameters)
        {
            return CurrentView.FinishButton_CanExecute();
        }

        private void FinishStep_Execute(object parameters)
        {
            // TODO: Pull data and generate document.
        }

        private bool NextStep_CanExecute(object parameters)
        {
            return CurrentView.NextButton_CanExecute();
        }

        private void NextStep_Execute(object parameters)
        {
            CurrentView.NextButton_OnClicked();
            CurrentView = CurrentView.Next;
            CurrentView.OnNavigateFromNextButton();
        }

        private bool PrevStep_CanExecute(object parameters)
        {
            return CurrentView.Prev != null;
        }

        private void PrevStep_Execute(object parameters)
        {
            CurrentView.PrevButton_OnClicked();
            CurrentView = CurrentView.Prev;
            CurrentView.OnNavigateFromPrevButton();
        }
    }
}
