using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    /// <summary>
    /// Represents view element that contains child element(s).
    /// </summary>
    public interface IContainerElement
    {
        /// <summary>
        /// Gets child view element at specified index.
        /// </summary>
        /// <param name="index">The specified index.</param>
        /// <returns>The child view element.</returns>
        UIElement GetChild(int index);
    }
}
