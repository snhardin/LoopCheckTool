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

namespace LoopCheckTool.Wizard.UserControls
{
    /// <summary>
    /// Interaction logic for FileBrowser.xaml
    /// </summary>
    public partial class FileBrowser : UserControl
    {
        /// <summary>
        /// File path set
        /// </summary>
        public string FilePath
        {
            get
            {
                return (string)GetValue(FilePathProperty);
            }
            set
            {
                SetValue(FilePathProperty, value);
            }
        }

        /// <summary>
        /// Text to display left of the displayed file path
        /// </summary>
        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        /// <summary>
        /// Dependency property for FilePath property
        /// </summary>
        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register(nameof(FilePath), typeof(string), typeof(FileBrowser), new PropertyMetadata(""));

        /// <summary>
        /// Dependency property for Text property
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(FileBrowser), new PropertyMetadata("Input:"));

        public FileBrowser()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles when the Browse button is clicked
        /// </summary>
        /// <param name="sender">The object handling the event</param>
        /// <param name="e">Event args</param>
        public void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                Filter = "All Files (*.*)|*.*",
                Multiselect = false,
            };

            if (dialog.ShowDialog() == true)
            {
                FilePath = dialog.FileName;
            }
        }
    }
}
