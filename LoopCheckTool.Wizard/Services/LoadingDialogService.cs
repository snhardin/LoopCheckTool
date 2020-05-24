using LoopCheckTool.Wizard.ViewModels;
using LoopCheckTool.Wizard.Views;

namespace LoopCheckTool.Wizard.Services
{
    public class LoadingDialogService : ILoadingDialogService
    {
        private LoadingDialogWindow _dialog;
        private LoadingDialogViewModel _vm;

        public void OpenLoadingDialog()
        {
            if (_dialog == null)
            {
                _vm = new LoadingDialogViewModel();
                _dialog = new LoadingDialogWindow() { DataContext = _vm };
                _dialog.ShowDialog();
            }
        }

        public void CloseLoadingDialog()
        {
            if (_dialog != null)
            {
                _dialog.Close();
                _dialog = null;
                _vm = null;
            }
        }

        public void SetLoadingText(string text)
        {
            if (_vm != null && _dialog != null)
            {
                _vm.LoadingText = text;
            }
        }
    }
}
