using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopCheckTool.Wizard.Services
{
    public interface ILoadingDialogService
    {
        void CloseLoadingDialog();
        void OpenLoadingDialog();
        void SetLoadingText(string text);
    }
}
