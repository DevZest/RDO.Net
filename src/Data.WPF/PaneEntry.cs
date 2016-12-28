using DevZest.Data.Windows.Primitives;

namespace DevZest.Data.Windows
{
    internal struct PaneEntry<T>
        where T : Binding
    {
        public readonly T Binding; 
        public readonly string Name;

        public PaneEntry(T binding, string name)
        {
            Binding = binding;
            Name = name;
        }
    }
}
