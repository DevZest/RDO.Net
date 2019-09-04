using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    /// <summary>
    /// Provides keys to identify functions.
    /// </summary>
    public static class FunctionKeys
    {
        /// <summary>
        /// Gets the key to identify [IsNull](xref:DevZest.Data.Functions.IsNull*) function.
        /// </summary>
        public static readonly FunctionKey IsNull = new FunctionKey(typeof(Functions), nameof(IsNull));

        /// <summary>
        /// Gets the key to identify [IsNotNull](xref:DevZest.Data.Functions.IsNotNull*) function.
        /// </summary>
        public static readonly FunctionKey IsNotNull = new FunctionKey(typeof(Functions), nameof(IsNotNull));

        /// <summary>
        /// Gets the key to identify [IfNull](xref:DevZest.Data.Functions.IfNull*) function.
        /// </summary>
        public static readonly FunctionKey IfNull = new FunctionKey(typeof(Functions), nameof(IfNull));

        /// <summary>
        /// Gets the key to identify [Now](xref:DevZest.Data._DateTime.Now*) function.
        /// </summary>
        public static readonly FunctionKey Now = new FunctionKey(typeof(Functions), nameof(Now));

        /// <summary>
        /// Gets the key to identify [UtcNow](xref:DevZest.Data._DateTime.UtcNow*) function.
        /// </summary>
        public static readonly FunctionKey UtcNow = new FunctionKey(typeof(Functions), nameof(UtcNow));

        /// <summary>
        /// Gets the key to identify [NewGuid](xref:DevZest.Data._Guid.NewGuid*) function.
        /// </summary>
        public static readonly FunctionKey NewGuid = new FunctionKey(typeof(Functions), nameof(NewGuid));

        /// <summary>
        /// Gets the key to identify [Sum](xref:DevZest.Data.Functions.Sum*) function.
        /// </summary>
        public static readonly FunctionKey Sum = new FunctionKey(typeof(Functions), nameof(Sum));

        /// <summary>
        /// Gets the key to identify [Count](xref:DevZest.Data.Functions.Count*) function.
        /// </summary>
        public static readonly FunctionKey Count = new FunctionKey(typeof(Functions), nameof(Count));

        /// <summary>
        /// Gets the key to identify [CountRows](xref:DevZest.Data.Functions.CountRows*) function.
        /// </summary>
        public static readonly FunctionKey CountRows = new FunctionKey(typeof(Functions), nameof(CountRows));

        /// <summary>
        /// Gets the key to identify [Average](xref:DevZest.Data.Functions.Average*) function.
        /// </summary>
        public static readonly FunctionKey Average = new FunctionKey(typeof(Functions), nameof(Average));

        /// <summary>
        /// Gets the key to identify [First](xref:DevZest.Data.Functions.First*) function.
        /// </summary>
        public static readonly FunctionKey First = new FunctionKey(typeof(Functions), nameof(First));

        /// <summary>
        /// Gets the key to identify [Last](xref:DevZest.Data.Functions.Last*) function.
        /// </summary>
        public static readonly FunctionKey Last = new FunctionKey(typeof(Functions), nameof(Last));

        /// <summary>
        /// Gets the key to identify [Min](xref:DevZest.Data.Functions.Min*) function.
        /// </summary>
        public static readonly FunctionKey Min = new FunctionKey(typeof(Functions), nameof(Min));

        /// <summary>
        /// Gets the key to identify [Max](xref:DevZest.Data.Functions.Max*) function.
        /// </summary>
        public static readonly FunctionKey Max = new FunctionKey(typeof(Functions), nameof(Max));

        /// <summary>
        /// Gets the key to identify [Contains](xref:DevZest.Data.Functions.Contains*) function.
        /// </summary>
        public static readonly FunctionKey Contains = new FunctionKey(typeof(Functions), nameof(Contains));
    }
}
