using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public class BlockElementPanel : FrameworkElement
    {
        private BlockView BlockView
        {
            get { return TemplatedParent as BlockView; }
        }

        private IReadOnlyList<UIElement> Elements
        {
            get
            {
                var blockView = BlockView;
                if (blockView == null)
                    return Array<UIElement>.Empty;

                if (blockView.ElementCollection == null || blockView.ElementCollection.Parent != this)
                    blockView.SetElementsPanel(this);

                Debug.Assert(blockView.ElementCollection.Parent == this);
                return blockView.ElementCollection;
            }
        }
    }
}
