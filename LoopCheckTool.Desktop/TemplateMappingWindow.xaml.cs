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
using System.Windows.Shapes;

namespace LoopCheckTool.Desktop
{
    /// <summary>
    /// Interaction logic for TemplateMappingWindow.xaml
    /// </summary>
    public partial class TemplateMappingWindow : Window
    {
        public TemplateMappingWindow()
        {
            InitializeComponent();

            List<ConfigItem> list = new List<ConfigItem>();
            list.Add(new ConfigItem() { TemplateValue = "test 1", TemplateName = "name 1" });
            list.Add(new ConfigItem() { TemplateValue = "test 2", TemplateName = "name 2" });
            list.Add(new ConfigItem() { TemplateValue = "test 3", TemplateName = "name 3" });

            kvTemplateConfig.ItemsSource = list;
        }
    }

    public class ConfigItem
    {
        public string TemplateValue { get; set; }
        public string TemplateName { get; set; }
    }
}
