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

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("The application has encountered an unrecoverable critical error. Please leave a bug report with any logs present " +
                "in the LoopCheckTool install folder.", "Critical", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
