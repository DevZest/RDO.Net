using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace DevZest
{
    static partial class IReadOnlyListExtensions
    {
        private sealed class ListVerifier<T> : IReadOnlyListVerifier<T>
            where T : class
        {
            internal ListVerifier(IReadOnlyList<T> list)
            {
                Debug.Assert(list != null);
                _list = list;
            }

            private readonly IReadOnlyList<T> _list;
            private int _currentIndex = 0;

            public IReadOnlyListVerifier<T> Verify<TItem>(Action<TItem> assertAction)
                where TItem : class, T
            {
                _list.Verify(_currentIndex++, assertAction);
                return this;
            }

            public void VerifyEof()
            {
                _list.VerifyEof(_currentIndex);
            }
        }

        public static IReadOnlyList<T> Verify<T, TItem>(this IReadOnlyList<T> list, int index, Action<TItem> assertAction)
            where T : class
            where TItem : class, T
        {
            Debug.Assert(list != null);
            Debug.Assert(assertAction != null);

            if (index >= list.Count)
                Assert.Fail(string.Format(CultureInfo.InvariantCulture, "List[{0}] is out of range, List.Count = {1}.", index, list.Count));

            var item = list[index] as TItem;
            if (item == null)
                Assert.Fail(string.Format(CultureInfo.InvariantCulture, "List[{0}] is null or not convertable to type '{1}'.", index, typeof(TItem)));

            if (assertAction != null)
                assertAction(item);
            index++;
            return list;
        }

        public static void VerifyEof<T>(this IReadOnlyList<T> list, int index)
        {
            Assert.IsTrue(index == list.Count, string.Format(CultureInfo.InvariantCulture, "EOF expected at List[{0}]; List.Count = {1}.", index, list.Count));
        }

        public static IReadOnlyListVerifier<T> Verify<T, TItem>(this IReadOnlyList<T> list, Action<TItem> assertAction)
            where T : class
            where TItem : class, T
        {
            var listItem = new ListVerifier<T>(list);
            return listItem.Verify(assertAction);
        }

        public static void VerifyEof<T>(this IReadOnlyList<T> list)
            where T : class
        {
            new ListVerifier<T>(list).VerifyEof();
        }
    }

    internal interface IReadOnlyListVerifier<T>
        where T : class
    {
        IReadOnlyListVerifier<T> Verify<TItem>(Action<TItem> assertAction = null)
            where TItem : class, T;

        void VerifyEof();
    }
}
