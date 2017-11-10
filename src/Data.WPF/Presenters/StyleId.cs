using DevZest.Data.Presenters.Primitives;
using System;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public sealed class StyleId : ResourceId<Style>
    {
        public StyleId(Type type)
            : base(type)
        {
        }

        protected override string UriSuffix
        {
            get { return "Styles"; }
        }
    }
}
