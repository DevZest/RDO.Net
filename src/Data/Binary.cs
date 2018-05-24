using System;
using System.Diagnostics;
using System.Runtime;
using System.Text;

namespace DevZest.Data
{
    /// <summary>Represents an immutable block of binary data.</summary>
    public sealed class Binary : IEquatable<Binary>
    {
        private byte[] bytes;
        private int? hashCode;

        /// <summary>Gets the length of the binary object.</summary>
        /// <returns>An integer representing the length.</returns>
        public int Length
        {
            get
            {
                return this.bytes.Length;
            }
        }
        
        /// <summary>Initializes a new instance of the <see cref="T:System.Data.Linq.Binary" /> class.</summary>
        /// <param name="value">The bytes representing the binary data.</param>
        public Binary(byte[] value)
        {
            if (value == null)
            {
                this.bytes = new byte[0];
            }
            else
            {
                this.bytes = new byte[value.Length];
                Array.Copy(value, this.bytes, value.Length);
            }
            this.ComputeHash();
        }
        
        /// <summary>Returns an array of bytes that represents the current binary object.</summary>
        /// <returns>A byte array that contains the value of the current binary object.</returns>
        public byte[] ToArray()
        {
            byte[] array = new byte[this.bytes.Length];
            Array.Copy(this.bytes, array, array.Length);
            return array;
        }
        
        /// <summary>Enables arrays of bytes to be implicitly coerced to the <see cref="T:System.Data.Linq.Binary" /> type in a programming language.</summary>
        /// <returns>A <see cref="T:System.Data.Linq.Binary" /> class containing the coerced value.</returns>
        /// <param name="value">The array of bytes to convert into an instance of the <see cref="T:System.Data.Linq.Binary" /> type.</param>
        public static implicit operator Binary(byte[] value)
        {
            return new Binary(value);
        }
        
        /// <summary>Determines whether two binary objects are equal.</summary>
        /// <returns>true if the two binary objects are equal; otherwise, false.</returns>
        /// <param name="other">The <see cref="T:System.Object" /> to which the current object is being compared.</param>
        public bool Equals(Binary other)
        {
            if (object.ReferenceEquals(this, other))
                return true;
            if (object.ReferenceEquals(other, null))
                return false;
            return EqualsTo(other);
        }
        
        /// <summary>Describes the equality relationship between two binary objects.</summary>
        /// <returns>true if the binary objects are equal; otherwise false.</returns>
        /// <param name="binary1">First binary object.</param>
        /// <param name="binary2">Second binary object.</param>
        public static bool operator ==(Binary binary1, Binary binary2)
        {
            return Equals(binary1, binary2);
        }
        
        /// <summary>Describes the inequality relationship between two binary objects.</summary>
        /// <returns>true if the binary objects are not equal; otherwise false.</returns>
        /// <param name="binary1">The first binary object.</param>
        /// <param name="binary2">The second binary object.</param>
        public static bool operator !=(Binary binary1, Binary binary2)
        {
            return !Equals(binary1, binary2);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Binary);
        }

        /// <summary>Gets the hash code of this binary object.</summary>
        /// <returns>The hash code computed based on the binary data.</returns>
        public override int GetHashCode()
        {
            if (!this.hashCode.HasValue)
                this.ComputeHash();

            return this.hashCode.Value;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("\"");
            stringBuilder.Append(Convert.ToBase64String(this.bytes, 0, this.bytes.Length));
            stringBuilder.Append("\"");
            return stringBuilder.ToString();
        }
        
        private static bool Equals(Binary binary1, Binary binary2)
        {
            if (object.ReferenceEquals(binary1, binary2))
                return true;
            if (object.ReferenceEquals(binary1, null) == object.ReferenceEquals(binary2, null))
                return binary1.EqualsTo(binary2);
            else
                return false;
        }

        private bool EqualsTo(Binary binary)
        {
            Debug.Assert(binary != null);
            if (this.bytes.Length != binary.bytes.Length)
                return false;
            if (this.GetHashCode() != binary.GetHashCode())
                return false;
            int i = 0;
            int num = this.bytes.Length;
            while (i < num)
            {
                if (this.bytes[i] != binary.bytes[i])
                    return false;
                i++;
            }
            return true;
        }

        private void ComputeHash()
        {
            int num = 314;
            int num2 = 159;
            this.hashCode = new int?(0);
            for (int i = 0; i < this.bytes.Length; i++)
            {
                this.hashCode = this.hashCode * num + (int)this.bytes[i];
                num *= num2;
            }
        }

        /// <summary>Gets the byte value at specified index.</summary>
        /// <param name="index">The index.</param>
        /// <returns>The byte value at specified index.</returns>
        public byte this[int index]
        {
            get { return bytes[index]; }
        }
    }
}
