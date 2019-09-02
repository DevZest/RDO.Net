using System;

namespace DevZest.Data.Annotations.Primitives
{
    /// <summary>
    /// Specifies the implementation attribute type of <see cref="ModelDeclarationAttribute"/>.
    /// </summary>
    /// <remarks>Each concrete <see cref="ModelDeclarationAttribute"/> should have a corresponding implementation.
    /// For example, <see cref="CheckConstraintAttribute"/> has a corresponding <see cref="_CheckConstraintAttribute"/>.
    /// Specify this attribute for both of the classes to identify the relationship between this two classes.
    /// This information will be used by code analyzers and designers.</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CrossReferenceAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CrossReferenceAttribute"/>.
        /// </summary>
        /// <param name="attibuteType">The corresponding attribute type.</param>
        public CrossReferenceAttribute(Type attibuteType)
        {
            AttributeType = attibuteType.VerifyNotNull(nameof(attibuteType));
        }

        /// <summary>
        /// Gets the corresponding attribute type.
        /// </summary>
        public Type AttributeType { get; }
    }
}
