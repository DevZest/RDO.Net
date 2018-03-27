using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Views
{
    /// <summary>
    /// Interaction logic for PasteAppendWindow.xaml
    /// </summary>
    internal partial class PasteAppendWindow : Window
    {
        private sealed class Presenter : DataPresenter<TabularText>
        {
            public Presenter()
            {

            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                var textColumns = _.TextColumns;

                builder.GridColumns(textColumns.Select(x => "Auto;Max:200").ToArray())
                    .GridRows("Auto")
                    .Layout(Orientation.Vertical);

                for (int i = 0; i < textColumns.Count; i++)
                    builder.AddBinding(i, 0, textColumns[i].BindToTextBlock().AddToGridCell());

            }
        }

        public PasteAppendWindow()
        {
            InitializeComponent();
        }

        private Presenter _presenter;
        public IReadOnlyList<ColumnValueBag> Show(IReadOnlyList<Column> columns)
        {
            var tabularText = TabularText.PasteFromClipboard();
            _presenter = new Presenter();
            _presenter.Show(_dataView, tabularText);
            ShowDialog();
            return null;
        }
    }
}
