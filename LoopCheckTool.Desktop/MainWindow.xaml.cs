using LoopCheckTool.ViewModels;
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

namespace LoopCheckTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private ExcelLoopCheckReader reader;
        public WorksheetViewModel WorksheetViewModel { get; }

        public MainWindow()
        {
            InitializeComponent();
            WorksheetViewModel = new WorksheetViewModel();
        }

        private void SettingsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow();
            if (settings.ShowDialog() == true)
            {
                // Done.
            }
        }

        private void SettingsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ExitCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = "All Files (*.*)|*.*",
                Multiselect = false,
            };

            if (dialog.ShowDialog() == true)
            {
                //try
                //{
                //    reader = new ExcelLoopCheckReader(dialog.FileName);
                //    ddlWorksheet.ItemsSource = reader.GetWorksheets().Select(s => new ComboBoxItem { Content = s });
                //    ddlWorksheet.SelectedIndex = 0;
                //    ddlWorksheet.IsEnabled = true;
                //}
                //catch (Exception ex)
                //{
                //    reader.Dispose();
                //    reader = null;
                //    ddlWorksheet.IsEnabled = false;

                //    MessageBox.Show(ex.Message);
                //}
            }
        }

        private void OpenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
    }
}
