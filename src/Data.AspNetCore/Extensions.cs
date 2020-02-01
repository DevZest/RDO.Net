using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace DevZest.Data.AspNetCore
{
    internal static class Extensions
    {
        public static bool IsScalar(this ModelExpression dataSetFor)
        {
            return dataSetFor.Metadata.IsScalar();
        }

        public static bool IsScalar(this ModelMetadata modelMetadata)
        {
            var validatorMetadata = modelMetadata.ValidatorMetadata;
            return validatorMetadata.OfType<ScalarAttribute>().Any();
        }

        public static bool IsDataSet(this Type type)
        {
            if (type == typeof(DataSet))
                return true;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(DataSet<>))
                return true;

            var baseType = type.BaseType;
            return baseType == null ? false : IsDataSet(baseType);
        }

        private const string IEnumerableOfIFormFileName = nameof(IEnumerable) + "`" + nameof(IFormFile);

        public static IEnumerable<string> GetTypeNames(this Column column, Type fieldType)
        {
            // Not returning type name here for IEnumerable<IFormFile> since we will be returning
            // a more specific name, IEnumerableOfIFormFileName.
            var fieldTypeInfo = fieldType.GetTypeInfo();

            if (typeof(IEnumerable<IFormFile>) != fieldType)
                yield return fieldType.Name;

            if (fieldType == typeof(string))
            {
                // Nothing more to provide
                yield break;
            }
            else if (!column.IsComplexDataType())
            {
                // IsEnum is false for the Enum class itself
                if (fieldTypeInfo.IsEnum)
                {
                    // Same as fieldType.BaseType.Name in this case
                    yield return "Enum";
                }
                else if (fieldType == typeof(DateTimeOffset))
                {
                    yield return "DateTime";
                }

                yield return "String";
                yield break;
            }
            else if (!fieldTypeInfo.IsInterface)
            {
                var type = fieldType;
                while (true)
                {
                    type = type.GetTypeInfo().BaseType;
                    if (type == null || type == typeof(object))
                    {
                        break;
                    }

                    yield return type.Name;
                }
            }

            if (typeof(IEnumerable).IsAssignableFrom(fieldType))
            {
                if (typeof(IEnumerable<IFormFile>).IsAssignableFrom(fieldType))
                {
                    yield return IEnumerableOfIFormFileName;

                    // Specific name has already been returned, now return the generic name.
                    if (typeof(IEnumerable<IFormFile>) == fieldType)
                    {
                        yield return fieldType.Name;
                    }
                }

                yield return "Collection";
            }
            else if (typeof(IFormFile) != fieldType && typeof(IFormFile).IsAssignableFrom(fieldType))
            {
                yield return nameof(IFormFile);
            }

            yield return "Object";
        }

        private static bool IsComplexDataType(this Column column)
        {
            return !TypeDescriptor.GetConverter(column.DataType).CanConvertFrom(typeof(string));
        }

        public static string GetFullHtmlFieldName(this ViewContext viewContext, ModelExpression dataSetFor, Column column, DataRow dataRow)
        {
            if (dataSetFor == null)
                throw new ArgumentNullException(nameof(dataSetFor));

            var dataSet = dataSetFor.Model as DataSet;
            var isScalar = dataSetFor.IsScalar();
            return viewContext.GetFullHtmlFieldName(dataSetFor.Name, dataSet, isScalar, column, dataRow);
        }

        private static string GetFullHtmlFieldName(this ViewContext viewContext, string expression, DataSet dataSet, bool isScalar, Column column, DataRow dataRow)
        {
            if (dataSet == null)
                throw new ArgumentNullException(nameof(dataSet));

            if (column == null)
                throw new ArgumentNullException(nameof(column));

            VerifyDataRow(dataRow, column, dataSet, isScalar);

            var memberName = ResolveMemberName(dataSet, isScalar, column, dataRow);
            var fullDataSetName = NameAndIdProvider.GetFullHtmlFieldName(viewContext, expression);
            return ModelNames.CreatePropertyModelName(fullDataSetName, memberName);
        }

        private static void VerifyDataRow(DataRow dataRow, Column column, DataSet dataSet, bool isScalar)
        {
            if (dataRow == null)
            {
                if (!isScalar)
                    throw new ArgumentNullException(nameof(dataRow));

                return;
            }

            if (dataRow.Model != column.GetParent())
                throw new ArgumentException(DiagnosticMessages.InvalidDataRowForColumn, nameof(dataRow));

            if (!IsValidDataRow(dataRow, dataSet))
                throw new ArgumentException(DiagnosticMessages.InvalidDataRowForDataSet, nameof(dataRow));
        }

        private static bool IsValidDataRow(DataRow dataRow, DataSet dataSet)
        {
            if (dataRow.Model == dataSet.Model)
                return true;

            var parentDataRow = dataRow.ParentDataRow;
            return parentDataRow == null ? false : IsValidDataRow(parentDataRow, dataSet);
        }

        private static string ResolveMemberName(DataSet dataSet, bool isScalar, Column column, DataRow dataRow)
        {
            if (column.GetParent() == dataSet.Model)
                return isScalar ? column.Name : ModelNames.CreatePropertyModelName(ModelNames.CreateIndexModelName(string.Empty, dataRow.Index), column.Name);

            Debug.Assert(dataRow != null);
            return ResolveMemberName(dataSet, isScalar, dataRow, column.Name);
        }

        private static string ResolveMemberName(DataSet dataSet, bool isScalar, DataRow dataRow, string memberName)
        {
            if (dataRow.Model == dataSet.Model)
                return isScalar ? memberName : ModelNames.CreatePropertyModelName(ModelNames.CreateIndexModelName(string.Empty, dataRow.Index), memberName);

            memberName = ModelNames.CreatePropertyModelName(ModelNames.CreateIndexModelName(dataRow.Model.GetName(), dataRow.Index), memberName);
            Debug.Assert(dataRow.ParentDataRow != null);
            return ResolveMemberName(dataSet, isScalar, dataRow.ParentDataRow, memberName);
        }

        public static Type UnderlyingOrDataType(this Column column)
        {
            return column.DataType.UnderlyingOrDataType();
        }

        public static Type UnderlyingOrDataType(this Type dataType)
        {
            return Nullable.GetUnderlyingType(dataType) ?? dataType;
        }

        public static string GetFormattedWeek(this Column column, object value)
        {
            if (value is DateTimeOffset dateTimeOffset)
                value = dateTimeOffset.DateTime;

            if (value is DateTime date)
            {
                var calendar = Thread.CurrentThread.CurrentCulture.Calendar;
                var day = calendar.GetDayOfWeek(date);

                // Get the week number consistent with ISO 8601. See blog post:
                // https://blogs.msdn.microsoft.com/shawnste/2006/01/24/iso-8601-week-of-year-format-in-microsoft-net/
                if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
                {
                    date = date.AddDays(3);
                }

                var week = calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                var year = calendar.GetYear(date);
                var month = calendar.GetMonth(date);

                // Last week (either 52 or 53) includes January dates (1st, 2nd, 3rd) 
                if (week >= 52 && month == 1)
                {
                    year--;
                }

                // First week includes December dates (29th, 30th, 31st)
                if (week == 1 && month == 12)
                {
                    year++;
                }

                return $"{year:0000}-W{week:00}";
            }

            return null;
        }

        public static bool IsEnum(this Column column)
        {
            return column.DataType.IsEnum();
        }

        public static bool IsEnum(this Type type)
        {
            return type.UnderlyingOrDataType().GetTypeInfo().IsEnum;
        }

        public static bool IsFlagsEnum(this Type type)
        {
            return type.IsEnum && type.UnderlyingOrDataType().GetTypeInfo().IsDefined(typeof(FlagsAttribute), inherit: false);
        }

        public static IReadOnlyDictionary<string, string> GetEnumNamesAndValues(this Column column)
        {
            return column.DataType.GetEnumNamesAndValues();
        }

        public static IReadOnlyDictionary<string, string> GetEnumNamesAndValues(this Type type)
        {
            var isEnum = type.IsEnum();
            if (!isEnum)
                return null;

            var result = new Dictionary<string, string>();

            var underlyingType = type.UnderlyingOrDataType();
            var enumFields = Enum.GetNames(underlyingType)
                .Select(name => underlyingType.GetField(name))
                .OrderBy(field => field.GetCustomAttribute<DisplayAttribute>(inherit: false)?.GetOrder() ?? 1000);

            foreach (var field in enumFields)
            {
                var value = ((Enum)field.GetValue(obj: null)).ToString("d");
                result.Add(field.Name, value);
            }

            return result;
        }

        public static Type GetEnumerableElementType(this Type type)
        {
            if (type == typeof(string) || !typeof(IEnumerable).IsAssignableFrom(type))
                return null;

            if (type.IsArray)
                return type.GetElementType();

            var enumerableType = type.ExtractGenericInterface(typeof(IEnumerable<>));
            var result = enumerableType?.GenericTypeArguments[0];

            return result ?? typeof(object);
        }

        private static Type ExtractGenericInterface(this Type queryType, Type interfaceType)
        {
            if (queryType == null)
                throw new ArgumentNullException(nameof(queryType));

            if (interfaceType == null)
                throw new ArgumentNullException(nameof(interfaceType));

            if (IsGenericInstantiation(queryType, interfaceType))
            {
                // queryType matches (i.e. is a closed generic type created from) the open generic type.
                return queryType;
            }

            // Otherwise check all interfaces the type implements for a match.
            // - If multiple different generic instantiations exists, we want the most derived one.
            // - If that doesn't break the tie, then we sort alphabetically so that it's deterministic.
            //
            // We do this by looking at interfaces on the type, and recursing to the base type 
            // if we don't find any matches.
            return GetGenericInstantiation(queryType, interfaceType);
        }

        private static bool IsGenericInstantiation(Type candidate, Type interfaceType)
        {
            return
                candidate.GetTypeInfo().IsGenericType &&
                candidate.GetGenericTypeDefinition() == interfaceType;
        }

        private static Type GetGenericInstantiation(Type queryType, Type interfaceType)
        {
            Type bestMatch = null;
            var interfaces = queryType.GetInterfaces();
            foreach (var @interface in interfaces)
            {
                if (IsGenericInstantiation(@interface, interfaceType))
                {
                    if (bestMatch == null)
                    {
                        bestMatch = @interface;
                    }
                    else if (StringComparer.Ordinal.Compare(@interface.FullName, bestMatch.FullName) < 0)
                    {
                        bestMatch = @interface;
                    }
                    else
                    {
                        // There are two matches at this level of the class hierarchy, but @interface is after
                        // bestMatch in the sort order.
                    }
                }
            }

            if (bestMatch != null)
            {
                return bestMatch;
            }

            // BaseType will be null for object and interfaces, which means we've reached 'bottom'.
            var baseType = queryType?.GetTypeInfo().BaseType;
            if (baseType == null)
            {
                return null;
            }
            else
            {
                return GetGenericInstantiation(baseType, interfaceType);
            }
        }
    }
}
