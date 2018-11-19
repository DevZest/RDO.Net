using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        /// <summary>
        /// Create a Document from a string through creating a project that contains it.
        /// </summary>
        /// <param name="source">Classes in the form of a string</param>
        /// <param name="language">The language the source code is in</param>
        /// <returns>A Document created from the source string</returns>
        public static Document CreateDocument(this string source, IEnumerable<MetadataReference> additionalReferences, string language = LanguageNames.CSharp)
        {
            return Enumerable.First(source.CreateProject(additionalReferences, language).Documents);
        }

        public static Document CreateDocument(this string source, string language = LanguageNames.CSharp)
        {
            return source.CreateDocument(Array.Empty<MetadataReference>(), language);
        }

        public static Document CreateDocument(this string source, MetadataReference metadataReference, string language = LanguageNames.CSharp)
        {
            return source.CreateDocument(new MetadataReference[] { metadataReference }, language);
        }
    }
}
