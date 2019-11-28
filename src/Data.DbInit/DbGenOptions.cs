using CommandLine;

namespace DevZest.Data.DbInit
{
    [Verb(Commands.DbGen, HelpText = "Generates database.")]
    internal class DbGenOptions : OptionsBase
    {
        [Option('i', "init", Required = false, HelpText = "Full name of DbInitializer type.")]
        public string DbInitializerType { get; set; }
    }
}
