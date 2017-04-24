using DevZest.Data;

namespace SmoothScroll.Models
{
    public class Foo : Model
    {
        public Column<string> Text { get; private set; }

        public Column<bool> IsSectionHeader { get; private set; }

        public Column<byte> BackgroundR { get; private set; }

        public Column<byte> BackgroundG { get; private set; }

        public Column<byte> BackgroundB { get; private set; }

        protected override void OnInitializing()
        {
            Text = DataSetContainer.CreateLocalColumn<string>(this);
            IsSectionHeader = DataSetContainer.CreateLocalColumn<bool>(this);
            BackgroundR = DataSetContainer.CreateLocalColumn<byte>(this);
            BackgroundG = DataSetContainer.CreateLocalColumn<byte>(this);
            BackgroundB = DataSetContainer.CreateLocalColumn<byte>(this);
            base.OnInitializing();
        }
    }
}
