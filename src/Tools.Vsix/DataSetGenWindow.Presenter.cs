using DevZest.Data.CodeAnalysis;
using DevZest.Data.DbInit;
using DevZest.Data.Presenters;
using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DevZest.Data.Tools
{
    partial class DataSetGenWindow
    {
        private sealed class Presenter : DataPresenter<DbInitMapper.DataSetEntry>
        {
            public Presenter(DataSetGenWindow window, CodeContext codeContext, EnvDTE.DTE dte)
            {
                _window = window;
                _codeContext = codeContext;
                _dte = dte;

                _dbSessionProvider = NewScalar<TypeSymbolEntry>().AddValidator(ValidateNotEmpty);
                _showDbLog = NewScalar<bool>();

                _dbSessionProviderTypes = GetDbSessionProviderTypes().ToArray();
                if (_dbSessionProviderTypes.Length == 1)
                    _dbSessionProvider.Value = _dbSessionProviderTypes[0];

                var dataSet = DbInitMapper.GetDataSetEntries(codeContext);
                Show(_window._dataView, dataSet);
            }

            private static string ValidateNotEmpty(TypeSymbolEntry dbSessionProviderType)
            {
                return dbSessionProviderType.IsDefault ? UserMessages.Validation_Required : null;
            }

            private readonly DataSetGenWindow _window;
            private readonly CodeContext _codeContext;
            private readonly EnvDTE.DTE _dte;
            private readonly TypeSymbolEntry[] _dbSessionProviderTypes;

            private Project Project
            {
                get { return _codeContext.Project; }
            }

            private readonly Scalar<TypeSymbolEntry> _dbSessionProvider;
            private readonly Scalar<bool> _showDbLog;

            private IEnumerable<TypeSymbolEntry> GetDbSessionProviderTypes()
            {
                var dbInitializerType = _codeContext.GetDbInitializerType();
                return Project.ResolveDbSessionProviderTypes(dbInitializerType, isEmptyDb: false).Select(x => new TypeSymbolEntry(Project, x));
            }

            private IEnumerable DbSessionProviderTypeSelection
            {
                get { return _dbSessionProviderTypes.Select(x => new { Value = x, Display = x.TypeSymbol.Name }); }
            }

            public bool Execute()
            {
                return ConsoleWindow.Run(_window.Title, RunAsync);
            }

            private async Task<int> RunAsync(IProgress<string> progress, CancellationToken ct)
            {
                var project = _dbSessionProvider.Value.Project;
                var dbSessionProvider = _dbSessionProvider.Value.TypeSymbol;
                var input = dbSessionProvider.GetDbInitInput(_codeContext.Compilation);
                var showLog = _showDbLog.Value;

                var tableNames = new string[DataSet.Count];
                for (int i = 0; i < DataSet.Count; i++)
                    tableNames[i] = _.DbTableProperty[i].Name;
                var outputDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Directory.CreateDirectory(outputDir);

                var runner = DbInitProjectRunner.CreateDataSetGen(project, dbSessionProvider, input, _showDbLog.Value, tableNames, outputDir, _dte);
                var result = await runner.RunAsync(progress, ct);
                if (result != ExitCodes.Succeeded)
                {
                    DeleteDirectoryIfExists(outputDir);
                    return result;
                }

                for (int i = 0; i < DataSet.Count; i++)
                {
                    var dbTable = _.DbTableProperty[i].Name;
                    _.ReferencedTypes[i] = File.ReadAllText(Path.Combine(outputDir, dbTable + ".types"));
                    _.DataSetMethodBody[i] = File.ReadAllText(Path.Combine(outputDir, dbTable + ".statements"));
                }

                DeleteDirectoryIfExists(outputDir);

                var document = await DbInitMapper.GenerateDataSetsAsync(_codeContext, DataSet, ct);
                var workspace = project.Solution.Workspace;
                workspace.TryApplyChanges(document.Project.Solution);

                return ExitCodes.Succeeded;
            }

            private static void DeleteDirectoryIfExists(string dir)
            {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder
                    .GridRows("Auto", "Auto")
                    .GridColumns("5*", "4*")
                    .GridLineX(new GridPoint(0, 2), 1)
                    .Layout(Orientation.Vertical)
                    .WithFrozenTop(1)
                    .AddBinding(_window._comboBoxDbSessionProvider, _dbSessionProvider.BindToComboBox(DbSessionProviderTypeSelection))
                    .AddBinding(_window._checkBoxShowDbLog, _showDbLog.BindToCheckBox())
                    .AddBinding(0, 0, _.DataSetMethod.BindToColumnHeader())
                    .AddBinding(1, 0, _.DbTableProperty.BindToColumnHeader())
                    .AddBinding(0, 1, _.DataSetMethod.BindToTextBlock(p => string.Format("{0}()", p.GetValue(_.DataSetMethod).Name)))
                    .AddBinding(1, 1, _.DbTableProperty.BindToComboBox(_.DbTablePropertySelection));
            }
        }
    }
}
