using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FL4Tool
{
    internal static class BinaryReaderExtensions
    {
        public static string ReadShortString(this BinaryReader reader, Encoding encoding)
        {
            int length = reader.ReadByte();
            var data = reader.ReadBytes(length);
            return encoding.GetString(data);
        }
    }
}
