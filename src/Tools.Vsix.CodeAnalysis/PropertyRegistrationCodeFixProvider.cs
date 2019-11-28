using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.CodeAnalysis
{
    public abstract class PropertyRegistrationCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIds.MissingRegistration); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        internal static void AddRegisterPropertyCodeFix(CodeFixContext context, string propertyName, PropertyRegistrationType type, Diagnostic diagnostic,
            Func<string, bool, CancellationToken, Task<Solution>> generatePropertyRegistration)
        {
            string methodName;
            if (type == PropertyRegistrationType.ModelColumn)
                methodName = "RegisterColumn";
            else if (type == PropertyRegistrationType.LocalColumn)
                methodName = "RegisterLocalColumn";
            else if (type == PropertyRegistrationType.Projection)
                methodName = "RegisterProjection";
            else if (type == PropertyRegistrationType.ColumnList)
                methodName = "RegisterColumnList";
            else
                return;

            if (type == PropertyRegistrationType.ModelColumn)
            {
                var title = string.Format("Add Mounter _{0}", propertyName);
                context.RegisterCodeFix(CodeAction.Create(
                    title: title,
                    createChangedSolution: ct => generatePropertyRegistration(methodName, true, ct),
                    equivalenceKey: title),
                    diagnostic);
            }

            context.RegisterCodeFix(CodeAction.Create(
                    title: methodName,
                    createChangedSolution: ct => generatePropertyRegistration(methodName, false, ct),
                    equivalenceKey: methodName),
                    diagnostic);
        }

        protected static void AddRegisterChildModelCodeFix(CodeFixContext context, SemanticModel semanticModel, IPropertySymbol propertySymbol, Diagnostic diagnostic,
            Func<string, bool, CancellationToken, Task<Solution>> generatePropoertyRegistration,
            Func<IPropertySymbol, CancellationToken, Task<Solution>> generateChildModelRegistration)
        {
            var compilation = semanticModel.Compilation;
            var methodName = "RegisterChildModel";
            var pkType = propertySymbol.ContainingType.GetPkType(compilation);
            var childModelType = propertySymbol.Type;
            var resolvedChildModelType = ResolveChildModelType(childModelType, compilation);
            IPropertySymbol[] foreignKeys = pkType == null || childModelType == null ? null : GetForeignKeys(resolvedChildModelType, pkType).ToArray();

            if (foreignKeys == null || foreignKeys.Length == 0)
                context.RegisterCodeFix(CodeAction.Create(
                                    title: methodName,
                                    createChangedSolution: ct => generatePropoertyRegistration(methodName, false, ct),
                                    equivalenceKey: methodName),
                                    diagnostic);
            else
            {
                for (int i = 0; i < foreignKeys.Length; i++)
                {
                    var foreignKey = foreignKeys[i];
                    var title = string.Format("{0} ({1}.{2})", methodName, childModelType.Name, foreignKey.Name);
                    context.RegisterCodeFix(CodeAction.Create(
                                        title: title,
                                        createChangedSolution: ct => generateChildModelRegistration(foreignKey, ct),
                                        equivalenceKey: title),
                                        diagnostic);
                }
            }
        }

        private static ITypeSymbol ResolveChildModelType(ITypeSymbol childModelType, Compilation compilation)
        {
            if (childModelType is ITypeParameterSymbol typeParameterSymbol)
            {
                var constraintTypes = typeParameterSymbol.ConstraintTypes;
                for (int i = 0; i < constraintTypes.Length; i++)
                {
                    var constraintType = constraintTypes[i];
                    if (constraintType.IsDerivedFrom(KnownTypes.Model, compilation))
                        return constraintType;
                }

                return null;
            }

            return childModelType;
        }

        private static IEnumerable<IPropertySymbol> GetForeignKeys(ITypeSymbol childModelType, INamedTypeSymbol pkType)
        {
            var members = childModelType.GetMembers();
            for (int i = 0; i < members.Length; i++)
            {
                if (members[i] is IPropertySymbol result)
                {
                    if (result.Type.Equals(pkType))
                        yield return result;
                }
            }
        }
    }
}
