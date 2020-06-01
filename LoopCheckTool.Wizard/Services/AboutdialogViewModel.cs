using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopCheckTool.Wizard.Services
{
    public class AboutDialogViewModel
    {
        public string AssemblyFileVersion { get; set; }
        public string AssemblyVersion { get; set; }
        public string GitBranch { get; set; }
        public string GitHash { get; set; }
        public string TaggedVersion { get; set; }
    }
}
