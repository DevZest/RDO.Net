using DevZest.Data.Views;

namespace DevZest.Data.Presenters.Plugins
{
    public interface IRowViewPlugin
    {
        void Setup(RowView rowView);
        void Refresh(RowView rowView);
        void Cleanup(RowView rowView);
    }
}
