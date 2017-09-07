using System;
using DevZest.Data.Views;

namespace DevZest.Data.Presenters.Plugins
{
    public abstract class RowViewPlugin : IRowViewPlugin
    {
        protected abstract void Setup(RowView rowView);

        protected abstract void Refresh(RowView rowView);

        protected abstract void Cleanup(RowView rowView);

        void IRowViewPlugin.Setup(RowView rowView)
        {
            Setup(rowView);
        }

        void IRowViewPlugin.Refresh(RowView rowView)
        {
            Refresh(rowView);
        }

        void IRowViewPlugin.Cleanup(RowView rowView)
        {
            Cleanup(rowView);
        }
    }
}
