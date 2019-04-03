using DevZest.Data.Presenters.Primitives;
using System;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    public sealed class TemplateId : ResourceId<ControlTemplate>
    {
        public TemplateId(Type type)
            : base(type)
        {
        }

        protected override string UriSuffix
        {
            get { return "Templates"; }
        }
    }
}
