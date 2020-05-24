using LoopCheckTool.Wizard.Services;
using LoopCheckTool.Wizard.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LoopCheckTool.Wizard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App() : base()
        {
            var service = new LoadingDialogService();
            var viewModel = new AppViewModel(service);
            Resources.Add(nameof(AppViewModel), viewModel);
        }
    }
}
