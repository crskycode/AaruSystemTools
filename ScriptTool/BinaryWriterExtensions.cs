using System.Text;

namespace ScriptTool
{
    internal static class BinaryWriterExtensions
    {
        public static void WriteUInt16(this BinaryWriter writer, ushort value)
        {
            writer.Write(value);
        }

        public static void WriteSymbolTerminatedString(this BinaryWriter writer, string s, char symbol, Encoding encoding)
        {
            var data = encoding.GetBytes(s);
            writer.Write(data);
            writer.Write(Convert.ToByte(symbol));
        }

        public static void WriteNullTerminatedString(this BinaryWriter writer, string s, Encoding encoding)
        {
            var data = encoding.GetBytes(s);
            writer.Write(data);
            writer.Write((byte)0);
        }
    }
}
