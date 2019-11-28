using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.Tools
{
    partial class DbInitWindow
    {
        private sealed class Presenter : SimplePresenter
        {
            public Presenter(DbInitWindow window, Project project, INamedTypeSymbol dbInitializerType, EnvDTE.DTE dte)
            {
                _window = window;
                _project = project;
                _dbInitializerType = dbInitializerType;
                _dte = dte;

                _dbSessionProvider = NewScalar<TypeSymbolEntry>().AddValidator(ValidateNotEmpty);
                _showLog = NewScalar<bool>();

                _dbSessionProviderTypes = GetDbSessionProviderTypes().ToArray();
                if (_dbSessionProviderTypes.Length == 1)
                    _dbSessionProvider.Value = _dbSessionProviderTypes[0];
                Show(_window._view);
            }

            private static string ValidateNotEmpty(TypeSymbolEntry dbInitType)
            {
                return dbInitType.IsDefault ? UserMessages.Validation_Required : null;
            }

            private readonly DbInitWindow _window;
            private readonly EnvDTE.DTE _dte;
            private readonly Project _project;
            private readonly INamedTypeSymbol _dbInitializerType;
            private readonly TypeSymbolEntry[] _dbSessionProviderTypes;

            private readonly Scalar<TypeSymbolEntry> _dbSessionProvider;
            private readonly Scalar<bool> _showLog;

            private IEnumerable<TypeSymbolEntry> GetDbSessionProviderTypes()
            {
                return _project.ResolveDbSessionProviderTypes(_dbInitializerType, isEmptyDb: true).Select(x => new TypeSymbolEntry(_project, x));
            }

            private IEnumerable DbSessionProviderTypeSelection
            {
                get { return _dbSessionProviderTypes.Select(x => new { Value = x, Display = x.TypeSymbol.Name }); }
            }

            public bool Execute()
            {
                var project = _dbSessionProvider.Value.Project;
                var dbSessionProvider = _dbSessionProvider.Value.TypeSymbol;
                project.TryGetCompilation(out var compilation);
                Debug.Assert(compilation != null);
                var input = dbSessionProvider.GetDbInitInput(compilation);
                var showLog = _showLog.Value;
                var runner = DbInitProjectRunner.CreateDbInit(project, dbSessionProvider, input, _dbInitializerType, _showLog.Value, _dte);
                return ConsoleWindow.Run(_window.Title, runner.RunAsync);
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder
                    .AddBinding(_window._comboBoxDbSessionProvider, _dbSessionProvider.BindToComboBox(DbSessionProviderTypeSelection))
                    .AddBinding(_window._checkBoxShowLog, _showLog.BindToCheckBox());
            }
        }
    }
}
