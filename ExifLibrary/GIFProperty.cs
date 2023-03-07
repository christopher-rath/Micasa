using System;
using System.Text;

namespace ExifLibrary
{
#pragma warning disable CA1036 // Override methods on comparable types
#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable CA1305 // Specify IFormatProvider
#pragma warning disable CA1507 // Use nameof to express symbol names
#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable CA1710 // Identifiers should have correct suffix
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
#pragma warning disable CA1715 // Identifiers should have correct prefix
#pragma warning disable CA1720 // Identifier contains type name
#pragma warning disable CA1725 // Parameter names should match base declaration
#pragma warning disable CA1805 // Do not initialize unnecessarily
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable CA1825 // Avoid zero-length array allocations
#pragma warning disable CA1854 // Prefer the 'IDictionary.TryGetValue(TKey, out TValue)' method
#pragma warning disable CA2251 // Use 'string.Equals'
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE1006 // Naming Styles
    /// <summary>
    /// Represents a 7-bit ASCII encoded comment string. (GIF Specification: Comment Extension)
    /// </summary>
    public class GIFComment : ExifProperty
    {
        protected string mValue;
        protected override object _Value { get { return Value; } set { Value = (string)value; } }
        public new string Value { get { return mValue; } set { mValue = value; } }
        protected internal GIFBlock InsertBefore { get; private set; }

        static public implicit operator string(GIFComment obj) { return obj.mValue; }

        public override string ToString() { return mValue; }

        public GIFComment(ExifTag tag, string value, GIFBlock insertBefore = null) : base(tag)
        {
            mValue = value;
            InsertBefore = insertBefore;
        }

        public override ExifInterOperability Interoperability
        {
            get
            {
                byte[] data = Encoding.ASCII.GetBytes(mValue);

                return new ExifInterOperability((ushort)mTag, InterOpType.ASCII, (uint)data.Length, data);
            }
        }
    }
}
