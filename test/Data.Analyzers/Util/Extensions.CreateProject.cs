using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        private static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        private static readonly MetadataReference SystemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
        private static readonly MetadataReference DataReference = MetadataReference.CreateFromFile(typeof(Model).Assembly.Location);
        private static readonly MetadataReference NetStandardReference = MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.0.0.0").Location);
        private static readonly MetadataReference SystemRuntimeReference = MetadataReference.CreateFromFile(Assembly.Load("System.Runtime, Version=0.0.0.0").Location);
        private static readonly MetadataReference SystemLinqExpressionsReference = MetadataReference.CreateFromFile(Assembly.Load("System.Linq.Expressions").Location);

        private const string DefaultFilePathPrefix = "Test";
        private const string CSharpDefaultFileExt = "cs";
        private const string VisualBasicDefaultExt = "vb";
        private const string TestProjectName = "TestProject";

        public static Project CreateProject(this string source, string language = LanguageNames.CSharp)
        {
            return new string[] { source }.CreateProject(language);
        }

        public static Project CreateProject(this string source, MetadataReference additionalReference, string language = LanguageNames.CSharp)
        {
            return new string[] { source }.CreateProject(additionalReference, language);
        }

        public static Project CreateProject(this string source, IEnumerable<MetadataReference> additionalReferences, string language = LanguageNames.CSharp)
        {
            return new string[] { source }.CreateProject(additionalReferences, language);
        }

        public static Project CreateProject(this string[] sources, string language = LanguageNames.CSharp)
        {
            return sources.CreateProject(Array.Empty<MetadataReference>(), language);
        }

        public static Project CreateProject(this string[] sources, MetadataReference additionalReference, string language = LanguageNames.CSharp)
        {
            return sources.CreateProject(new MetadataReference[] { additionalReference }, language);
        }

        /// <summary>
        /// Create a project using the inputted strings as sources.
        /// </summary>
        /// <param name="sources">Classes in the form of strings</param>
        /// <param name="language">The language the source code is in</param>
        /// <returns>A Project created out of the Documents created from the source strings</returns>
        public static Project CreateProject(this string[] sources, IEnumerable<MetadataReference> additionalReferences, string language = LanguageNames.CSharp)
        {
            string fileNamePrefix = DefaultFilePathPrefix;
            string fileExt = language == LanguageNames.CSharp ? CSharpDefaultFileExt : VisualBasicDefaultExt;

            var projectId = ProjectId.CreateNewId(debugName: TestProjectName);

            var solution = new AdhocWorkspace()
                .CurrentSolution
                .AddProject(projectId, TestProjectName, TestProjectName, language)
                .AddMetadataReference(projectId, CorlibReference)
                .AddMetadataReference(projectId, SystemCoreReference)
                .AddMetadataReference(projectId, DataReference)
                .AddMetadataReference(projectId, NetStandardReference)
                .AddMetadataReference(projectId, SystemRuntimeReference)
                .AddMetadataReference(projectId, SystemLinqExpressionsReference);

            if (additionalReferences != null)
            {
                foreach (var additionalReference in additionalReferences)
                    solution = solution.AddMetadataReferences(projectId, additionalReferences);
            }

            int count = 0;
            foreach (var source in sources)
            {
                var newFileName = fileNamePrefix + count + "." + fileExt;
                var documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
                solution = solution.AddDocument(documentId, newFileName, SourceText.From(source));
                count++;
            }
            return solution.GetProject(projectId);
        }
    }
}
