using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class ScalarGridItem : GridItem
    {
        internal ScalarGridItem(Model parentModel)
            : base(parentModel)
        {
        }

        private RepeatMode _repeatMode;
        public RepeatMode RepeatMode
        {
            get { return _repeatMode; }
            set
            {
                VerifyNotSealed();
                _repeatMode = value;
            }
        }
    }
}
