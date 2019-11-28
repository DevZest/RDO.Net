using CommandLine;

namespace DevZest.Data.DbInit
{
    internal abstract class OptionsBase
    {
        [Option('s', "session", Required = true, HelpText = "Full name of DbSessionProvider type.")]
        public string DbSessionProviderType { get; set; }

        [Option('p', "proj", Required = false, HelpText = "Path of the project.")]
        public string ProjectPath { get; set; }

        [Option('v', Default = false, Required = false, HelpText = "Displays executed database commands.")]
        public bool Verbose { get; set; }

        [Option('c', "cancel", Required = false, HelpText = "Named pipe for cancellation.")]
        public string CancellationPipeName { get; set; }
    }
}
