using System;
using DevZest.Data.Views;

namespace DevZest.Data.Presenters
{
    public abstract class RowViewPlugin
    {
        protected internal abstract void Setup(RowView rowView);

        protected internal abstract void Refresh(RowView rowView);

        protected internal abstract void Cleanup(RowView rowView);
    }
}
