using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;

namespace DevZest.Data.CodeAnalysis
{
    public abstract class CodeMapper
    {
        protected virtual void Update(CodeContext context)
        {
            Debug.Assert(!context.IsEmpty);

            Document = context.Document;
            RootSyntaxNode = context.RootSyntaxNode;
            Compilation = context.Compilation;
            SemanticModel = context.SemanticModel;
        }

        protected SyntaxNode RootSyntaxNode { get; private set; }

        protected SyntaxTree SyntaxTree
        {
            get { return RootSyntaxNode.SyntaxTree; }
        }

        public Compilation Compilation { get; private set; }

        protected SemanticModel SemanticModel { get; private set; }

        public Document Document { get; private set; }

        public Project Project
        {
            get { return Document.Project; }
        }

        public Solution Solution
        {
            get { return Project.Solution; }
        }

        public Workspace Workspace
        {
            get { return Solution.Workspace; }
        }

        protected string Language
        {
            get { return Project.Language; }
        }

        public abstract bool RefreshSelectionChanged(TextSpan selectionSpan);
    }
}
