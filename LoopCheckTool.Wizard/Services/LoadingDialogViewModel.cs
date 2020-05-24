using LoopCheckTool.Wizard.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopCheckTool.Wizard.Services
{
    public class LoadingDialogViewModel : OnPropertyChangedNotifier
    {
        private string _loadingText;

        public string LoadingText
        {
            get => _loadingText;
            set
            {
                _loadingText = value;
                OnPropertyChanged(nameof(LoadingText));
            }
        }

        public LoadingDialogViewModel()
        {
            _loadingText = "";
        }
    }
}
