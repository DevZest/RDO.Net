using DevZest.Data.Windows.Primitives;
using System;
using System.Windows;
using System.Windows.Data;

namespace DevZest.Data.Windows.Utilities
{
    internal static partial class DependencyPropertyExtensions
    {
        private static UpdateSourceTrigger GetDefaultUpdateSourceTrigger(this DependencyProperty property, Type type)
        {
            var metaData = property.GetMetadata(type) as FrameworkPropertyMetadata;
            return metaData == null ? UpdateSourceTrigger.PropertyChanged : metaData.DefaultUpdateSourceTrigger;
        }

        internal static Trigger<T> GetUpdateSourceTrigger<T>(this DependencyProperty property, UpdateSourceTrigger updateSourceTrigger)
            where T : UIElement, new()
        {
            switch (updateSourceTrigger)
            {
                case UpdateSourceTrigger.Default:
                    return property.GetUpdateSourceTrigger<T>(property.GetDefaultUpdateSourceTrigger(typeof(T)));
                case UpdateSourceTrigger.Explicit:
                    return new ExplicitTrigger<T>();
                case UpdateSourceTrigger.LostFocus:
                    return new LostFocusTrigger<T>();
                case UpdateSourceTrigger.PropertyChanged:
                    return new PropertyChangedTrigger<T>(property);
            }
            return null;
        }
    }
}
