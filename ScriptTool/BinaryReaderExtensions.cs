﻿using System.Runtime.InteropServices;
using System.Text;

namespace ScriptTool
{
    internal static class BinaryReaderExtensions
    {
        public static string ReadNullTerminatedString(this BinaryReader reader, Encoding encoding)
        {
            var buffer = new List<byte>(256);

            for (var b = reader.ReadByte(); b != 0; b = reader.ReadByte())
            {
                buffer.Add(b);
            }

            if (buffer.Count == 0)
            {
                return string.Empty;
            }

            return encoding.GetString(CollectionsMarshal.AsSpan(buffer));
        }

        public static string ReadSymbolTerminatedString(this BinaryReader reader, char symbol, Encoding encoding)
        {
            var buffer = new List<byte>(256);

            for (var b = reader.ReadByte(); b != symbol; b = reader.ReadByte())
            {
                buffer.Add(b);
            }

            if (buffer.Count == 0)
            {
                return string.Empty;
            }

            return encoding.GetString(CollectionsMarshal.AsSpan(buffer));
        }
    }
}
