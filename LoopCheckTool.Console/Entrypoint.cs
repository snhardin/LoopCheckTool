using CommandLine;
using CommandLine.Text;
using LoopCheckTool.Console.Verbs;

namespace LoopCheckTool.Console
{
    static class Entrypoint
    {
        /// <summary>
        /// Main method of console application.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Return code</returns>
        public static int Main(string[] args)
        {
            // See https://github.com/commandlineparser/commandline/wiki/How-To#q1
            Parser parser = new Parser(config =>
            {
                config.AutoVersion = false;
                config.HelpWriter = null;
            });

            int returnCode = 1; // default to unsuccessful return code.
            ParserResult<object> parserResult = parser.ParseArguments<ListSheetsVerb, GenerateVerb, VersionVerb>(args);
            parserResult.WithParsed<IVerb>(verb => returnCode = verb.Execute());
            parserResult.WithNotParsed(errors =>
            {
                HelpText helpText = HelpText.AutoBuild(parserResult, h =>
                {
                    h.AutoVersion = false;
                    return HelpText.DefaultParsingErrorsHandler(parserResult, h);
                },
                example => example);

                System.Console.WriteLine(helpText);
                returnCode = 1;
            });

            return returnCode;
        }
    }
}
