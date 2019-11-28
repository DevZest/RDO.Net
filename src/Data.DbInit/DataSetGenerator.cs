using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevZest.Data.DbInit
{
    internal sealed class DataSetGenerator : IDisposable
    {
        public DataSetGenerator(DataSet dataSet, string language)
        {
            DataSet = dataSet;
            Language = language;
            Workspace = new AdhocWorkspace();
            Generator = SyntaxGenerator.GetGenerator(Workspace, language);
            ReferencedTypes = new HashSet<Type>();
            Statements = new List<SyntaxNode>();
            Generate();
        }

        public void Dispose()
        {
            Workspace.Dispose();
        }

        public DataSet DataSet { get; }
        public string Language { get; }
        private Workspace Workspace { get; }
        public SyntaxGenerator Generator { get; }
        private HashSet<Type> ReferencedTypes { get; }
        private List<SyntaxNode> Statements { get; }

        public void AddReferencedType(Type type)
        {
            ReferencedTypes.Add(type);
        }

        private void AddStatement(SyntaxNode syntaxNode)
        {
            Statements.Add(syntaxNode.NormalizeWhitespace());
        }

        public IEnumerable<string> GetReferencedTypes()
        {
            return ReferencedTypes.Select(x => x.FullName).OrderBy(x => x);
        }

        public IEnumerable<string> GetStatements()
        {
            if (Language == LanguageNames.CSharp)
                yield return "{";

            foreach (var result in Statements.Select(x => x.ToString()))
            {
                if (Language == LanguageNames.CSharp && !result.EndsWith(";"))
                    yield return result + ";";
                else
                    yield return result;
            }

            if (Language == LanguageNames.CSharp)
                yield return "}";
        }

        private Type ModelType
        {
            get { return DataSet.Model.GetType(); }
        }

        private void Generate()
        {
            AddReferencedType(typeof(DataSet<>));
            AddReferencedType(ModelType);
            AddReferencedType(typeof(DbInitExtensions));

            var g = Generator;
            AddStatement(g.DataSetCreation(ModelType, DataSet.Count));
            if (DataSet.Count > 0)
                GenerateColumnValues();
            AddStatement(g.ReturnResult());
        }

        public string ModelVariableName
        {
            get { return Language == LanguageNames.VisualBasic ? "x" : "_"; }
        }

        private void GenerateColumnValues()
        {
            var g = Generator;
            AddStatement(g.ModelVariableDeclaration(ModelVariableName, ModelType, Language));
            AddReferencedType(typeof(Primitives.ModelExtensions));
            AddStatement(g.SuspendIdentity(ModelVariableName));
            foreach (var column in DataSet.Model.GetColumns())
            {
                if (column.IsSerializable)
                    GenerateColumnValues(column);
            }
            AddStatement(g.ResumeIdentity(ModelVariableName));
        }

        private void GenerateColumnValues(Column column)
        {
            var g = ColumnValueGenerator.Get(column);
            g.Initialize(this, column);
            for (int i = 0; i < DataSet.Count; i++)
                AddStatement(g.Generate(i));
        }
    }
}
