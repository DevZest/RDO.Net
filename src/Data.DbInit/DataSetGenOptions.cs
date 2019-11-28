using System.Collections.Generic;
using CommandLine;

namespace DevZest.Data.DbInit
{
    [Verb(Commands.DataSetGen, HelpText = "Generates dataset(s) from database.")]
    internal class DataSetGenOptions : OptionsBase
    {
        [Option('t', "tables", Required = true, HelpText = "Name of database table(s).")]
        public IEnumerable<string> Tables { get; set; }

        [Option('l', "lang", Required = true, HelpText = "Programming language.")]
        public string Language { get; set; }

        [Option('o', "out", Required = true, HelpText = "Output directory.")]
        public string OutputDirectory { get; set; }
    }
}
