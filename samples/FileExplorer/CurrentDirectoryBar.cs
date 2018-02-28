using DevZest.Data;
using DevZest.Data.Presenters;
using DevZest.Data.Views;

namespace FileExplorer
{
    public sealed class CurrentDirectoryBar : DirectoryPresenter<CurrentDirectoryBar.Item>
    {
        public sealed class Item : Model
        {
        }

        public CurrentDirectoryBar(DataView dataView, DirectoryTree directoryTree)
            : base(directoryTree)
        {
            _currentDirectory = NewScalar<string>();
            CurrentDirectory = DirectoryTree.CurrentPath;

            var dataSet = DataSet<Item>.New();
            Show(dataView, dataSet);
        }

        private readonly Scalar<string> _currentDirectory;
        protected sealed override string CurrentDirectory
        {
            get { return _currentDirectory.GetValue(true); }
            set { _currentDirectory.SetValue(value, true); }
        }

        protected override void BuildTemplate(TemplateBuilder builder)
        {
            builder.GridColumns("*")
                .GridRows("Auto", "0")
                .RowRange(0, 1, 0, 1)
                .AddBinding(0, 0, _currentDirectory.BindToTextBox().Input.AddToInPlaceEditor(_currentDirectory.BindToTextBlock()));
        }
    }
}
