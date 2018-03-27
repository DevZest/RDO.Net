using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
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
            public Presenter(PasteAppendWindow window)
            {

            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                var textColumns = _.TextColumns;

                builder.GridRows(textColumns.Select(x => "Auto;Max:200").ToArray())
                    .GridColumns("Auto")
                    .Layout(Orientation.Vertical);

                for (int i = 0; i < textColumns.Count; i++)
                    builder.AddBinding(i, 0, textColumns[i].BindToTextBlock().AddToGridCell());

            }
        }

        public PasteAppendWindow()
        {
            InitializeComponent();
        }
    }
}
