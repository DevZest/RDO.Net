using Microsoft.CodeAnalysis;
using System;

namespace DevZest.Data.CodeAnalysis
{
    public struct TypeSymbolEntry
    {
        public TypeSymbolEntry(Project project, INamedTypeSymbol typeSymbol)
        {
            Project = project ?? throw new ArgumentNullException(nameof(project));
            TypeSymbol = typeSymbol ?? throw new ArgumentNullException(nameof(typeSymbol));
        }

        public Project Project { get; }
        public INamedTypeSymbol TypeSymbol { get; }

        public bool IsDefault
        {
            get { return Project == null && TypeSymbol == null; }
        }

        public TypeSymbolEntry Refresh()
        {
            if (IsDefault)
                return this;

            var solution = Project.Solution;
            var workspace = solution.Workspace;
            if (workspace.CurrentSolution == solution)
                return this;

            var project = workspace.CurrentSolution.GetProject(Project.Id);
            if (project == null)
                return this;
            var typeSymbol = project.GetNamedTypeSymbol(TypeSymbol);
            if (typeSymbol == null)
                return this;
            return new TypeSymbolEntry(project, typeSymbol);
        }
    }
}
