using System;

namespace DevZest.Data.Primitives
{
    /// <summary>Represents operation of binary expression.</summary>
    public enum BinaryExpressionKind
    {
        /// <summary>Logical AND.</summary>
        And,
        /// <summary>Logical OR.</summary>
        Or,
        /// <summary>Equality comparison.</summary>
        Equal,
        /// <summary>Inequality comparison.</summary>
        NotEqual,
        /// <summary>Arithmetic addition.</summary>
        Add,
        /// <summary>Arithmetic substract.</summary>
        Substract,
        /// <summary>Arithmetic multiply.</summary>
        Multiply,
        /// <summary>Arithmetic divide.</summary>
        Divide,
        /// <summary>Arithmetic modulo.</summary>
        Modulo,
        /// <summary>Bitwise AND.</summary>
        BitwiseAnd,
        /// <summary>Bitwise OR.</summary>
        BitwiseOr,
        /// <summary>Bitwise XOR.</summary>
        BitwiseXor,
        /// <summary>Less than comparison.</summary>
        LessThan,
        /// <summary>Less than or equal comparison.</summary>
        LessThanOrEqual,
        /// <summary>Greater than comparison.</summary>
        GreaterThan,
        /// <summary>Greater than or equal comparison.</summary>
        GreaterThanOrEqual
    }
}
