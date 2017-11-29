using DevZest.Data.Annotations.Primitives;
using System;
using System.Linq;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class CreditCardAttribute : GeneralValidationColumnAttribute
    {
        protected override bool IsValid(Column column, DataRow dataRow)
        {
            var stringColumn = column as Column<string>;
            return stringColumn == null ? false : IsValid(stringColumn[dataRow]);
        }

        protected override string GetDefaultMessage(Column column, DataRow dataRow)
        {
            return Strings.CreditCardAttribute_DefaultErrorMessage;
        }

        private static bool IsValid(string text)
        {
            if (text == null)
                return false;

            text = text.Replace("-", "");
            text = text.Replace(" ", "");
            int num = 0;
            bool flag = false;
            foreach (char current in text.Reverse())
            {
                if (current < '0' || current > '9')
                    return false;
                int i = (int)((current - '0') * (flag ? '\u0002' : '\u0001'));
                flag = !flag;
                while (i > 0)
                {
                    num += i % 10;
                    i /= 10;
                }
            }
            return num % 10 == 0;
        }
    }
}
