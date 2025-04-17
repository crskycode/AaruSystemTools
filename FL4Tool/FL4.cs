using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FL4Tool
{
    internal class FL4
    {
        private readonly static byte[] Signature = Encoding.UTF8.GetBytes("FL4.0\0\0\0");
        private readonly static int NumBuckets = 512;

        public static void Extract(string filePath, string targetPath, Encoding encoding)
        {
            using var fileReader = new BinaryReader(File.OpenRead(filePath));

            var signature = fileReader.ReadBytes(8);

            if (!signature.SequenceEqual(Signature))
            {
                throw new Exception("This is not a valid FL4.0 file.");
            }

            var dataPosition = fileReader.ReadUInt16();
            var indexLength = fileReader.ReadInt32();
            var indexPosition = fileReader.ReadInt32();
            var numEntries = fileReader.ReadInt32();

            fileReader.BaseStream.Position = indexPosition;

            var indexBuffer = fileReader.ReadBytes(indexLength);
            var indexReader = new BinaryReader(new MemoryStream(indexBuffer));

            var buckets = new Bucket[NumBuckets];

            for (var i = 0; i < buckets.Length; i++)
            {
                buckets[i].Position = indexReader.ReadInt32();
                buckets[i].Size = indexReader.ReadUInt16();
            }

            var entries = new List<Entry>(numEntries);

            foreach (var bucket in buckets)
            {
                indexReader.BaseStream.Position = bucket.Position;

                for (var i = 0; i < bucket.Size; i++)
                {
                    var entry = new Entry
                    {
                        Position = indexReader.ReadInt32(),
                        Length = indexReader.ReadInt32(),
                        Name = indexReader.ReadShortString(encoding),
                    };

                    entries.Add(entry);
                }
            }

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];

                Console.WriteLine("Extract {0} [{1}/{2}]", entry.Name, i, entries.Count);

                var entryPath = Path.Combine(targetPath, entry.Name);
                var dirPath = Path.GetDirectoryName(entryPath)!;

                Directory.CreateDirectory(dirPath);

                fileReader.BaseStream.Position = dataPosition + entry.Position;
                var data = fileReader.ReadBytes(entry.Length);

                // Decompress PD2A
                if (BitConverter.ToInt32(data) == 0x41324450)
                {
                    var lz = new LZSS();
                    data = lz.Decompress(data, 0x10, data.Length - 0x10);
                }

                File.WriteAllBytes(entryPath, data);
            }

            fileReader.Close();
        }

        private struct Bucket
        {
            public int Position;
            public int Size;
        }

        private struct Entry
        {
            public int Position;
            public int Length;
            public string Name;
        }

        private readonly static HashSet<string> CompressedExtensions = [".BM2"];

        public static void Create(string filePath, string rootPath, Encoding encoding)
        {
            // Find file to create

            var files = Directory.GetFiles(rootPath, "*", SearchOption.AllDirectories);

            // Create entries

            var entries = new List<PackEntry>(files.Length);

            foreach (var path in files)
            {
                var entry = new PackEntry
                {
                    Path = path,
                    Name = Path.GetRelativePath(rootPath, path)
                };

                if (entry.Name.Length > 255)
                {
                    throw new Exception($"The file name [{entry.Name}] is too long.");
                }

                entries.Add(entry);
            }

            // Create

            using var fileWriter = new BinaryWriter(File.Create(filePath));

            // Write entry data

            fileWriter.BaseStream.Position = 0x1C;

            var dataPosition = Convert.ToUInt16(fileWriter.BaseStream.Position);

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];

                Console.WriteLine("Add {0} [{1}/{2}]", entry.Name, i, entries.Count);

                var data = File.ReadAllBytes(entry.Path);

                // Compress PD2A
                if (CompressedExtensions.Contains(Path.GetExtension(entry.Name).ToUpper()))
                {
                    var lz = new LZSS();
                    var buffer = lz.Compress(data, 0, data.Length);

                    var ms = new MemoryStream();
                    var bw = new BinaryWriter(ms);

                    bw.Write(0x41324450);
                    bw.Write(0);
                    bw.Write(buffer.Length);
                    bw.Write(data.Length);
                    bw.Write(buffer);

                    data = ms.ToArray();
                }

                entry.Position = Convert.ToInt32(fileWriter.BaseStream.Position) - dataPosition;
                entry.Length = data.Length;

                fileWriter.Write(data);
            }

            Console.WriteLine("Create index");

            var indexPosition = Convert.ToInt32(fileWriter.BaseStream.Position);

            // Write bucket index

            var buckets = new List<PackBucket>(NumBuckets);

            for (var i = 0; i < NumBuckets; i++)
            {
                fileWriter.Write(0);
                fileWriter.WriteUInt16(0);

                var bucket = new PackBucket();
                buckets.Add(bucket);
            }

            var entryPosition = Convert.ToInt32(fileWriter.BaseStream.Position);

            // Put entries into bucket

            foreach (var entry in entries)
            {
                var name = Path.GetFileName(entry.Path)
                    .ToUpper();

                var hash = ComputeHash(encoding.GetBytes(name));
                var bucket = buckets[hash];

                if (bucket.Entries.Count > ushort.MaxValue)
                {
                    throw new Exception("Too many entries in bucket.");
                }

                bucket.Entries.Add(entry);
            }

            // Write entry index

            foreach (var bucket in buckets)
            {
                if (bucket.Entries.Count != 0)
                {
                    bucket.Entries = bucket.Entries
                        .OrderBy(entry => entry.Name)
                        .ToList();

                    bucket.Position = Convert.ToInt32(fileWriter.BaseStream.Position) - indexPosition;

                    foreach (var entry in bucket.Entries)
                    {
                        fileWriter.Write(entry.Position);
                        fileWriter.Write(entry.Length);
                        fileWriter.WriteShortString(entry.Name, encoding);
                    }
                }
                else
                {
                    bucket.Position = entryPosition - indexPosition;
                }
            }

            var indexLength = Convert.ToInt32(fileWriter.BaseStream.Position) - indexPosition;

            // Update bucket index

            fileWriter.BaseStream.Position = indexPosition;

            foreach (var bucket in buckets)
            {
                fileWriter.Write(bucket.Position);
                fileWriter.WriteUInt16(Convert.ToUInt16(bucket.Entries.Count));
            }

            // Write header

            fileWriter.BaseStream.Position = 0;

            fileWriter.Write(Signature);
            fileWriter.WriteUInt16(dataPosition);
            fileWriter.Write(indexLength);
            fileWriter.Write(indexPosition);
            fileWriter.Write(entries.Count);
            fileWriter.Write(0);
            fileWriter.WriteUInt16(0);

            // Finish

            fileWriter.Flush();
            fileWriter.Close();

            Console.WriteLine("Finish");
        }

        private static int ComputeHash(byte[] input)
        {
            uint hash = 0;

            for (var i = 0; i < input.Length; i++)
            {
                hash = ((hash << 8) + input[i]) % 0x1FD;
            }

            return (int)hash;
        }

        private class PackEntry
        {
            public string Path = string.Empty;
            public string Name = string.Empty;
            public int Position;
            public int Length;
        }

        private class PackBucket
        {
            public int Position;
            public List<PackEntry> Entries = [];
        }
    }
}
