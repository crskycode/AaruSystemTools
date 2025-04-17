using System.Text;

namespace FL4Tool
{
    internal static class BinaryWriterExtensions
    {
        public static void WriteUInt16(this BinaryWriter writer, ushort value)
        {
            writer.Write(value);
        }

        public static void WriteShortString(this BinaryWriter writer, string s, Encoding encoding)
        {
            var data = encoding.GetBytes(s);
            writer.Write(Convert.ToByte(data.Length));
            writer.Write(data);
        }
    }
}
