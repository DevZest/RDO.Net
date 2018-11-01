using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    internal static partial class Extensions
    {
        public static SortDirection? GetSortDirection(this IParameterSymbol parameter, Compilation compilation)
        {
            bool isAsc = false;
            bool isDesc = false;
            var attributes = parameter.GetAttributes();
            for (int i = 0; i < attributes.Length; i++)
            {
                var attributeClass = attributes[i].AttributeClass;
                if (attributeClass.EqualsTo(KnownTypes.AscAttribute, compilation))
                    isAsc = true;
                else if (attributeClass.EqualsTo(KnownTypes.DescAttribute, compilation))
                    isDesc = true;
            }

            if (isAsc && isDesc)
                return null;
            else if (isAsc)
                return SortDirection.Ascending;
            else if (isDesc)
                return SortDirection.Descending;
            else
                return SortDirection.Unspecified;
        }
    }
}
