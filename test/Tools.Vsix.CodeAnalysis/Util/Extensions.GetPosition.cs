using Microsoft.CodeAnalysis.Text;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static int GetPosition(this SourceText sourceText, int line, int column)
        {
            var textLine = sourceText.Lines[line - 1];
            return textLine.Start + column - 1;
        }

    }
}
