using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    public static class FunctionKeys
    {
        public static readonly FunctionKey IsNull = new FunctionKey(typeof(Functions), nameof(IsNull));

        public static readonly FunctionKey IsNotNull = new FunctionKey(typeof(Functions), nameof(IsNotNull));

        public static readonly FunctionKey IfNull = new FunctionKey(typeof(Functions), nameof(IfNull));

        public static readonly FunctionKey GetDate = new FunctionKey(typeof(Functions), nameof(GetDate));

        public static readonly FunctionKey GetUtcDate = new FunctionKey(typeof(Functions), nameof(GetUtcDate));

        public static readonly FunctionKey NewGuid = new FunctionKey(typeof(Functions), nameof(NewGuid));

        public static readonly FunctionKey Sum = new FunctionKey(typeof(Functions), nameof(Sum));

        public static readonly FunctionKey Count = new FunctionKey(typeof(Functions), nameof(Count));

        public static readonly FunctionKey CountRows = new FunctionKey(typeof(Functions), nameof(CountRows));

        public static readonly FunctionKey Average = new FunctionKey(typeof(Functions), nameof(Average));

        public static readonly FunctionKey First = new FunctionKey(typeof(Functions), nameof(First));

        public static readonly FunctionKey Last = new FunctionKey(typeof(Functions), nameof(Last));

        public static readonly FunctionKey Min = new FunctionKey(typeof(Functions), nameof(Min));

        public static readonly FunctionKey Max = new FunctionKey(typeof(Functions), nameof(Max));

        public static readonly FunctionKey Contains = new FunctionKey(typeof(Functions), nameof(Contains));
    }
}
