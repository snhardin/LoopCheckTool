using LoopCheckTool.Wizard.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LoopCheckTool.Wizard.Services
{
    public class AboutDialogService : IAboutDialogService
    {
        private const string DEFAULT_VALUE = "unknown";

        public void OpenAboutDialog()
        {
            AboutDialogViewModel vm = new AboutDialogViewModel()
            {
                AssemblyFileVersion = DEFAULT_VALUE,
                AssemblyVersion = DEFAULT_VALUE,
                GitBranch = DEFAULT_VALUE,
                GitHash = DEFAULT_VALUE,
                TaggedVersion = DEFAULT_VALUE,
            };

            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                AssemblyName assemblyName = AssemblyName.GetAssemblyName(assembly.Location);
                IEnumerable<AssemblyMetadataAttribute> customAttributes = assembly.GetCustomAttributes<AssemblyMetadataAttribute>();

                vm.AssemblyFileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
                vm.AssemblyVersion = assemblyName.Version.ToString();
                vm.GitBranch = customAttributes.Where(m => m.Key.Equals("GitBranch")).FirstOrDefault().Value;
                vm.GitHash = customAttributes.Where(m => m.Key.Equals("GitCommit")).FirstOrDefault().Value;
                vm.TaggedVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            }
            catch
            {
                MessageBox.Show("An error occurred while retrieving assembly information.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            AboutWindow window = new AboutWindow
            {
                DataContext = vm,
            };

            window.ShowDialog();
        }
    }
}
