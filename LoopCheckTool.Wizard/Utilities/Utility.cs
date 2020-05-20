using System;
using System.IO;

namespace LoopCheckTool.Wizard.Utilities
{
    public static class Utility
    {
        /// <summary>
        /// Gets the initial directory for the browse modal. If it
        /// can't figure it out, it uses My Documents by default.
        /// </summary>
        /// <param name="directory">The initial directory to try</param>
        /// <returns>The initial directory to set for the browse modal</returns>
        public static string GetInitialDirectory(string directory, bool specified)
        {
            try
            {
                if (specified)
                {
                    return Path.GetDirectoryName(directory);
                }
            }
            catch
            {
                // Swallow the error. We're just going to use the default.
            }

            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
    }
}
