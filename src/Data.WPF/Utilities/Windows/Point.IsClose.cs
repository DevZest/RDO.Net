using System.Windows;

namespace DevZest.Windows
{
    static partial class Extension
    {
        public static bool IsClose(this Point point1, Point point2)
        {
            return point1.X.IsClose(point2.X) && point1.Y.IsClose(point2.Y);
        }
    }
}
