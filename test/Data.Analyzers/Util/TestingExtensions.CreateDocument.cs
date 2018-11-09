using Microsoft.CodeAnalysis;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    static partial class TestingExtensions
    {
        /// <summary>
        /// Create a Document from a string through creating a project that contains it.
        /// </summary>
        /// <param name="source">Classes in the form of a string</param>
        /// <param name="language">The language the source code is in</param>
        /// <returns>A Document created from the source string</returns>
        public static Document CreateDocument(this string source, string language = LanguageNames.CSharp)
        {
            return Enumerable.First(source.CreateProject(language).Documents);
        }
    }
}
