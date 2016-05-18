using DevZest.Data;

namespace SmoothScroll
{
    public class Foo : Model
    {
        public static readonly Accessor<Foo, _String> TextAccessor = RegisterColumn((Foo x) => x.Text);
        public static readonly Accessor<Foo, _Boolean> IsSectionAccessor = RegisterColumn((Foo x) => x.IsSectionHeader);
        public static readonly Accessor<Foo, _Byte> BackgroundRAccessor = RegisterColumn((Foo x) => x.BackgroundR);
        public static readonly Accessor<Foo, _Byte> BackgroundGAccessor = RegisterColumn((Foo x) => x.BackgroundG);
        public static readonly Accessor<Foo, _Byte> BackgroundBAccessor = RegisterColumn((Foo x) => x.BackgroundB);

        public _String Text { get; private set; }

        public _Boolean IsSectionHeader { get; private set; }

        public _Byte BackgroundR { get; private set; }

        public _Byte BackgroundG { get; private set; }

        public _Byte BackgroundB { get; private set; }
    }
}
