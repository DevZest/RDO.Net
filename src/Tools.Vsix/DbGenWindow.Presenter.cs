using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DevZest.Data.Tools
{
    partial class DbGenWindow
    {
        private sealed class Presenter : SimplePresenter
        {
            public Presenter(DbGenWindow window, Project project, INamedTypeSymbol dbSessionProviderType, DataSet<DbInitInput> input, EnvDTE.DTE dte)
            {
                _window = window;
                _project = project;
                _dbSessionProviderType = dbSessionProviderType;
                _input = input;
                _dte = dte;

                _showLog = NewScalar<bool>();

                Show(_window._view);
            }

            private static string ValidateNotEmpty(TypeSymbolEntry dbInitType)
            {
                return dbInitType.IsDefault ? UserMessages.Validation_Required : null;
            }

            private readonly DbGenWindow _window;
            private readonly EnvDTE.DTE _dte;
            private readonly Project _project;
            private readonly INamedTypeSymbol _dbSessionProviderType;
            private readonly DataSet<DbInitInput> _input;

            private readonly Scalar<bool> _showLog;

            public bool Execute()
            {
                var showLog = _showLog.Value;
                var runner = DbInitProjectRunner.CreateDbInit(_project, _dbSessionProviderType, _input, null, _showLog.Value, _dte);
                return ConsoleWindow.Run(_window.Title, runner.RunAsync);
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder
                    .AddBinding(_window._checkBoxShowLog, _showLog.BindToCheckBox());
            }
        }
    }
}
