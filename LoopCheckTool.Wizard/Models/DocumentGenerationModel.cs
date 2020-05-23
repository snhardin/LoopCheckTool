using LoopCheckTool.Lib.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopCheckTool.Wizard.Models
{
    public class DocumentGenerationModel
    {
        #region Main Required Options
        public string Header { get; set; }
        public string OutputPath { get; set; }
        public ExcelReader Reader { get; set; }
        public ExcelReader.Worksheet Sheet { get; set; }
        public string TemplatesPath { get; set; }
        #endregion

        #region Optional Options w/ Defaults
        public bool IgnoreErrors { get; set; }
        #endregion

        public DocumentGenerationModel()
        {
            IgnoreErrors = false;
        }
    }
}
