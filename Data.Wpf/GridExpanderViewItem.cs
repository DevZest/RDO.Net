using System;

namespace DevZest.Data.Wpf
{
    public class GridExpanderViewItem<T> : ModelViewItem<T>
        where T : GridExpander, new()
    {
        public GridExpanderViewItem(Model model, Action<T> initializer, params GridDefinition[] gridDefinitions)
            : base(model, initializer)
        {
            if (gridDefinitions == null || gridDefinitions.Length == 0)
                throw new ArgumentNullException(nameof(gridDefinitions));

            _gridDefinitions = gridDefinitions;
        }

        private GridDefinition[] _gridDefinitions;
    }
}
