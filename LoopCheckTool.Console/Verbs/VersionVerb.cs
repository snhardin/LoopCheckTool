using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LoopCheckTool.Console.Verbs
{
    /// <summary>
    /// Custom version verb implementation for getting additional information about the tool
    /// for troubleshooting.
    /// </summary>
    [Verb("version", HelpText = "Displays version information about the application.")]
    class VersionVerb : IVerb
    {
        /// <summary>
        /// Default value if a certain assembly attribute cannot be found.
        /// </summary>
        private const string DEFAULT_VALUE = "Unknown";

        /// <summary>
        /// Pulls assembly information, formats it, and prints it to screen.
        /// </summary>
        /// <returns></returns>
        public int Execute()
        {
            string assemblyFileVersion = DEFAULT_VALUE;
            string assemblyVersion = DEFAULT_VALUE;
            string gitBranch = DEFAULT_VALUE;
            string gitHash = DEFAULT_VALUE;
            string taggedVersion = DEFAULT_VALUE;

            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                AssemblyName assemblyName = AssemblyName.GetAssemblyName(assembly.Location);
                IEnumerable<AssemblyMetadataAttribute> customAttributes = assembly.GetCustomAttributes<AssemblyMetadataAttribute>();

                assemblyFileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
                assemblyVersion = assemblyName.Version.ToString();
                gitBranch = customAttributes.Where(m => m.Key.Equals("GitBranch")).FirstOrDefault().Value;
                gitHash = customAttributes.Where(m => m.Key.Equals("GitCommit")).FirstOrDefault().Value;
                taggedVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            }
            catch
            {
                System.Console.WriteLine("An error occurred while retrieving assembly information.");

                return 1;
            }

            System.Console.WriteLine(string.Format("{0,23} {1}", "Tagged Version:", taggedVersion));
            System.Console.WriteLine(string.Format("{0,23} {1}", "Assembly Version:", assemblyVersion));
            System.Console.WriteLine(string.Format("{0,23} {1}", "Assembly File Version:", assemblyFileVersion));
            System.Console.WriteLine(string.Format("{0,23} {1}", "Git Hash:", gitHash));
            System.Console.WriteLine(string.Format("{0,23} {1}", "Git Branch:", gitBranch));

            return 0;
        }
    }
}
