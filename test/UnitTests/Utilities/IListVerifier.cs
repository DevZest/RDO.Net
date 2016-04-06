using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace DevZest
{
    static partial class IReadOnlyListExtensions
    {
        private sealed class ListVerifier<T> : IListVerifier<T>
            where T : class
        {
            internal ListVerifier(IReadOnlyList<T> list)
            {
                Debug.Assert(list != null);
                _list = list;
            }

            private readonly IReadOnlyList<T> _list;
            private int _currentIndex = 0;

            public IListVerifier<T> Verify<TItem>(Action<TItem> assertAction)
                where TItem : class, T
            {
                Debug.Assert(assertAction != null);

                if (_currentIndex >= _list.Count)
                    Assert.Fail(string.Format(CultureInfo.InvariantCulture, "List[{0}] is out of range, List.Count = {1}.", _currentIndex, _list.Count));

                var item = _list[_currentIndex] as TItem;
                if (item == null)
                    Assert.Fail(string.Format(CultureInfo.InvariantCulture, "List[{0}] is null or not convertable to type '{1}'.", _currentIndex, typeof(TItem)));

                if (assertAction != null)
                    assertAction(item);
                _currentIndex++;
                return this;
            }

            public void VerifyEof()
            {
                Assert.IsTrue(_currentIndex == _list.Count,
                    string.Format(CultureInfo.InvariantCulture, "EOF expected at List[{0}]; List.Count = {1}.", _currentIndex, _list.Count));
            }
        }

        public static IListVerifier<T> Verify<T, TItem>(this IReadOnlyList<T> list, Action<TItem> assertAction)
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

    internal interface IListVerifier<T>
        where T : class
    {
        IListVerifier<T> Verify<TItem>(Action<TItem> assertAction = null)
            where TItem : class, T;

        void VerifyEof();
    }
}
