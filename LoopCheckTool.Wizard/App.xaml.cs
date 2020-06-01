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
            LoadingDialogService loadingService = new LoadingDialogService();
            AboutDialogService aboutService = new AboutDialogService();
            AppViewModel viewModel = new AppViewModel(loadingService, aboutService);
            Resources.Add(nameof(AppViewModel), viewModel);
        }
    }
}
