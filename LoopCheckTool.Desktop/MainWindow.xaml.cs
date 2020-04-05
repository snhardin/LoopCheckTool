using LoopCheckTool.Desktop.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LoopCheckTool.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnTemplateMap_Click(object sender, RoutedEventArgs e)
        {
            TemplateMappingWindow mappings = new TemplateMappingWindow();
            if (mappings.ShowDialog() == true)
            {
                // Do nothing...
            }
        }

        private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            LoopCheckViewModel viewModel = DataContext as LoopCheckViewModel;
            viewModel.SelectInputFile.Execute(null);
        }

        private void OpenCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            LoopCheckViewModel viewModel = DataContext as LoopCheckViewModel;
            e.CanExecute = viewModel.SelectInputFile.CanExecute(null);
        }

        private void cmbSheets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoopCheckViewModel viewModel = DataContext as LoopCheckViewModel;
            viewModel.LoadNewHeaders();
        }
    }
}
