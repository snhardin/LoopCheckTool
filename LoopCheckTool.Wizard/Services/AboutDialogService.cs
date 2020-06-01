using LoopCheckTool.Wizard.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LoopCheckTool.Wizard.Services
{
    public class AboutDialogService : IAboutDialogService
    {
        public void OpenAboutDialog()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            //FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            AssemblyName assemblyName = AssemblyName.GetAssemblyName(assembly.Location);
            var customAttributes = assembly.GetCustomAttributes<AssemblyMetadataAttribute>();
            var assemblyFileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
            var assemblyVersion = assemblyName.Version.ToString();
            var gitBranch = customAttributes.Where(m => m.Key.Equals("GitBranch")).FirstOrDefault()?.Value ?? "";
            var gitHash = customAttributes.Where(m => m.Key.Equals("GitCommit")).FirstOrDefault()?.Value ?? "";
            var taggedVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            AboutDialogViewModel vm = new AboutDialogViewModel()
            {
                AssemblyFileVersion = assemblyFileVersion,
                AssemblyVersion = assemblyVersion,
                GitBranch = gitBranch,
                GitHash = gitHash,
                TaggedVersion = taggedVersion,
            };

            AboutWindow window = new AboutWindow
            {
                DataContext = vm,
            };

            window.ShowDialog();
        }
    }
}
