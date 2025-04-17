using System.Text;

namespace ScriptTool
{
    internal class Script
    {
        private byte[] _script = [];
        private readonly CommandAssembly _assembly = [];
        private string _disassembly = string.Empty;

        public void Load(string filePath, Encoding encoding)
        {
            _script = File.ReadAllBytes(filePath);
            var stream = new MemoryStream(_script);
            Parse(stream, encoding);
        }

        public void Save(string filePath)
        {
            File.WriteAllBytes(filePath, _script);
        }

        private void Parse(Stream stream, Encoding encoding)
        {
            var reader = new BinaryReader(stream);
            var dis = new StringBuilder(8388608);

            _assembly.Clear();

            while (stream.Position < stream.Length)
            {
                var addr = Convert.ToInt32(stream.Position);
                var code = reader.ReadUInt16();

                switch (code)
                {
                    case 0x0000:
                    {
                        var index = reader.ReadUInt16();
                        var voice = string.Empty;

                        if (reader.ReadUInt16() == 0x565C)
                            voice = reader.ReadSymbolTerminatedString('.', encoding);
                        else
                            stream.Position -= 2;

                        var message = reader.ReadNullTerminatedString(encoding);

                        dis.AppendLine($"{addr:X8} | MSG 0x{index} \"{voice}\" \"{message}\"");

                        break;
                    }
                    case 0x0101:
                    {
                        var lhs = reader.ReadUInt16();
                        var op = reader.ReadByte();
                        var rhs = reader.ReadUInt16();

                        switch (op)
                        {
                            case 0x24:
                            {
                                dis.AppendLine($"{addr:X8} | AND F{lhs} F{rhs}");
                                break;
                            }
                            case 0x2A:
                            {
                                dis.AppendLine($"{addr:X8} | MUL F{lhs} F{rhs}");
                                break;
                            }
                            case 0x2B:
                            {
                                dis.AppendLine($"{addr:X8} | ADD F{lhs} F{rhs}");
                                break;
                            }
                            case 0x2D:
                            {
                                dis.AppendLine($"{addr:X8} | SUB F{lhs} F{rhs}");
                                break;
                            }
                            case 0x2E:
                            {
                                dis.AppendLine($"{addr:X8} | MOD F{lhs} F{rhs}");
                                break;
                            }
                            case 0x2F:
                            {
                                dis.AppendLine($"{addr:X8} | DIV F{lhs} F{rhs}");
                                break;
                            }
                            case 0x3D:
                            {
                                dis.AppendLine($"{addr:X8} | MOV F{lhs} F{rhs}");
                                break;
                            }
                            case 0x7C:
                            {
                                dis.AppendLine($"{addr:X8} | OR F{lhs} F{rhs}");
                                break;
                            }
                            default:
                            {
                                throw new Exception($"Unknow operator {code:X4} at {addr:X8}");
                            }
                        }

                        break;
                    }
                    case 0x0205:
                    {
                        var name = reader.ReadNullTerminatedString(encoding);
                        dis.AppendLine($"{addr:X8} | SCRIPT \"{name}\"");
                        break;
                    }
                    case 0x0300:
                    {
                        var message = reader.ReadNullTerminatedString(encoding);
                        dis.AppendLine($"{addr:X8} | ASK \"{message}\"");
                        break;
                    }
                    case 0x0301:
                    {
                        var message = reader.ReadNullTerminatedString(encoding);
                        var target = reader.ReadInt32();
                        dis.AppendLine($"{addr:X8} | SEL \"{message}\" L{target:X8}");
                        break;
                    }
                    case 0x0302:
                    {
                        var a1 = reader.ReadUInt16();
                        dis.AppendLine($"{addr:X8} | CMD_{code:X4} 0x{a1:X2}");
                        break;
                    }
                    case 0x0304:
                    {
                        dis.AppendLine($"{addr:X8} | CMD_{code:X4}");
                        break;
                    }
                    case 0x0305:
                    {
                        var sb = new StringBuilder();

                        sb.Append("( ");

                        for (var i = 0; i < 4; i++)
                        {
                            if (i != 0)
                            {
                                sb.Append(", ");
                            }

                            var val = reader.ReadUInt16();
                            sb.AppendFormat("{0}", val);
                        }

                        sb.Append(" )");

                        var a3 = reader.ReadUInt16();

                        var target = reader.ReadInt32();

                        dis.AppendLine($"{addr:X8} | CMD_{code:X4} L{target:X8} RECT {sb} 0x{a3:X2}");

                        break;
                    }
                    case 0x0306:
                    {
                        var sb = new StringBuilder();

                        sb.Append("( ");

                        for (var i = 0; i < 4; i++)
                        {
                            if (i != 0)
                            {
                                sb.Append(", ");
                            }

                            var val = reader.ReadUInt16();
                            sb.AppendFormat("{0}", val);
                        }

                        sb.Append(" )");

                        var target = reader.ReadInt32();

                        dis.AppendLine($"{addr:X8} | CMD_{code:X4} L{target:X8} RECT {sb}");

                        break;
                    }
                    case 0x0307:
                    {
                        var a1 = reader.ReadInt32();
                        dis.AppendLine($"{addr:X8} | CMD_{code:X4} 0x{a1:X4}");
                        break;
                    }
                    case 0x0308:
                    {
                        dis.AppendLine($"{addr:X8} | CMD_{code:X4}");
                        break;
                    }
                    case 0x0402:
                    {
                        var duration = reader.ReadUInt16();
                        dis.AppendLine($"{addr:X8} | SLEEP 0x{duration:X2}");
                        break;
                    }
                    case 0x0500:
                    {
                        var a1 = reader.ReadUInt16();
                        var a2 = reader.ReadUInt16();
                        dis.AppendLine($"{addr:X8} | CMD_{code:X4} 0x{a1:X2} 0x{a2:X2}");
                        break;
                    }
                    case 0x0501:
                    {
                        dis.AppendLine($"{addr:X8} | CMD_{code:X4}");
                        break;
                    }
                    case 0x0502:
                    {
                        var a1 = reader.ReadUInt16();
                        dis.AppendLine($"{addr:X8} | CMD_{code:X4} 0x{a1:X2}");
                        break;
                    }
                    case 0x0505:
                    {
                        var flags = reader.ReadUInt16();
                        dis.AppendLine($"{addr:X8} | END 0x{flags:X2}");
                        break;
                    }
                    case 0x0600:
                    {
                        var name = reader.ReadNullTerminatedString(encoding);
                        var flags = reader.ReadUInt16();
                        dis.AppendLine($"{addr:X8} | BGM \"{name}\" 0x{flags:X4}");
                        break;
                    }
                    case 0x0602:
                    {
                        dis.AppendLine($"{addr:X8} | CMD_{code:X4}");
                        break;
                    }
                    case 0x0604:
                    {
                        dis.AppendLine($"{addr:X8} | CMD_{code:X4}");
                        break;
                    }
                    case 0x0605:
                    {
                        var a1 = reader.ReadNullTerminatedString(encoding);
                        dis.AppendLine($"{addr:X8} | CMD_{code:X4} \"{a1}\"");
                        break;
                    }
                    case 0x0609:
                    {
                        var a1 = reader.ReadUInt16();
                        var a2 = reader.ReadNullTerminatedString(encoding);
                        dis.AppendLine($"{addr:X8} | CMD_{code:X4} 0x{a1:X2} \"{a2}\"");
                        break;
                    }
                    case 0x060A:
                    {
                        var a1 = reader.ReadUInt16();
                        dis.AppendLine($"{addr:X8} | CMD_{code:X4} 0x{a1:X2}");
                        break;
                    }
                    case 0x0703:
                    case 0x0704:
                    case 0x0705:
                    {
                        var name = reader.ReadNullTerminatedString(encoding);
                        dis.AppendLine($"{addr:X8} | CG \"{name}\"");
                        break;
                    }
                    case 0x0707:
                    {
                        var a1 = reader.ReadUInt16();
                        var a2 = reader.ReadNullTerminatedString(encoding);
                        dis.AppendLine($"{addr:X8} | CMD_{code:X4} 0x{a1:X2} \"{a2}\"");
                        break;
                    }
                    case 0x0709:
                    {
                        var a1 = reader.ReadUInt16();
                        dis.AppendLine($"{addr:X8} | CMD_{code:X4} 0x{a1:X2}");
                        break;
                    }
                    case 0x070A:
                    {
                        var a1 = reader.ReadUInt16();
                        var a2 = reader.ReadUInt16();
                        dis.AppendLine($"{addr:X8} | CMD_{code:X4} 0x{a1:X2} 0x{a2:X2}");
                        break;
                    }
                    case 0x070C:
                    {
                        dis.AppendLine($"{addr:X8} | CMD_{code:X4}");
                        break;
                    }
                    case 0x070D:
                    {
                        var a1 = reader.ReadUInt16();
                        dis.AppendLine($"{addr:X8} | CMD_{code:X4} 0x{a1:X2}");
                        break;
                    }
                    case 0x0711:
                    {
                        var a1 = reader.ReadUInt16();
                        var a2 = reader.ReadUInt16();
                        var a3 = reader.ReadUInt16();
                        var a4 = reader.ReadNullTerminatedString(encoding);
                        dis.AppendLine($"{addr:X8} | CMD_{code:X4} 0x{a1:X2} 0x{a2:X2} 0x{a3:X2} \"{a4}\"");
                        break;
                    }
                    case 0x8000:
                    {
                        var target = reader.ReadInt32();
                        var count = reader.ReadByte();

                        var sb = new StringBuilder();

                        sb.Append("( ");

                        for (var i = 0; i < count; i++)
                        {
                            var lhs = reader.ReadUInt16();
                            var op = reader.ReadByte();
                            var rhs = reader.ReadUInt16();

                            if (i != 0)
                            {
                                sb.Append(", ");
                            }

                            switch (op)
                            {
                                case 0x21:
                                {
                                    // EQ
                                    sb.AppendFormat("F{0} == F{1}", lhs, rhs);
                                    break;
                                }
                                case 0x26:
                                {
                                    // (L & R) == 0
                                    sb.AppendFormat("F{0} !& F{1}", lhs, rhs);
                                    break;
                                }
                                case 0x3C:
                                case 0x7B:
                                {
                                    // BT
                                    sb.AppendFormat("F{0} > F{1}", lhs, rhs);
                                    break;
                                }
                                case 0x3D:
                                {
                                    // NE
                                    sb.AppendFormat("F{0} != F{1}", lhs, rhs);
                                    break;
                                }
                                case 0x3E:
                                case 0x7D:
                                {
                                    // LT
                                    sb.AppendFormat("F{0} < F{1}", lhs, rhs);
                                    break;
                                }
                                default:
                                {
                                    throw new Exception($"Unknow logical operator {code:X4} at {addr:X8}");
                                }
                            }
                        }

                        sb.Append(" )");

                        dis.AppendLine($"{addr:X8} | CJMP L{target:X8} COND {sb}");

                        break;
                    }
                    case 0x8001:
                    {
                        var target = reader.ReadInt32();
                        dis.AppendLine($"{addr:X8} | JMP L{target:X8}");
                        break;
                    }
                    default:
                    {
                        throw new Exception($"Unknow opcode {code:X4} at {addr:X8}");
                    }
                }

                var size = Convert.ToInt32(stream.Position) - addr;

                _assembly.Add(code, addr, size);
            }

            if (stream.Position != stream.Length)
            {
                Console.WriteLine("WARNING: This script file is not full parsed.");
            }

            reader.Close();

            _disassembly = dis.ToString();
        }

        public void ExportDisasm(string filePath)
        {
            File.WriteAllText(filePath, _disassembly);
        }

        public void ExportText(string filePath, Encoding encoding)
        {
            using var writer = File.CreateText(filePath);

            var stream = new MemoryStream(_script);
            var reader = new BinaryReader(stream);

            foreach (var cmd in _assembly)
            {
                stream.Position = cmd.Addr + 2;

                switch (cmd.Code)
                {
                    case 0x0000:
                    {
                        reader.ReadUInt16();

                        if (reader.ReadUInt16() == 0x565C)
                            reader.ReadSymbolTerminatedString('.', encoding);
                        else
                            stream.Position -= 2;

                        var message = reader.ReadNullTerminatedString(encoding);

                        writer.WriteLine("◇{0:X8}◇{1}", cmd.Addr, message);
                        writer.WriteLine("◆{0:X8}◆{1}", cmd.Addr, message);
                        writer.WriteLine();

                        break;
                    }
                    case 0x0300:
                    {
                        var message = reader.ReadNullTerminatedString(encoding);

                        writer.WriteLine("◇{0:X8}◇{1}", cmd.Addr, message);
                        writer.WriteLine("◆{0:X8}◆{1}", cmd.Addr, message);
                        writer.WriteLine();

                        break;
                    }
                    case 0x0301:
                    {
                        var message = reader.ReadNullTerminatedString(encoding);

                        writer.WriteLine("◇{0:X8}◇{1}", cmd.Addr, message);
                        writer.WriteLine("◆{0:X8}◆{1}", cmd.Addr, message);
                        writer.WriteLine();

                        break;
                    }
                }
            }

            writer.Flush();
            writer.Close();
        }

        public void ImportText(string filePath, Encoding inputEncoding, Encoding outputEncoding)
        {
            var translation = Translation.Load(filePath);

            var orgStream = new MemoryStream(_script);
            var orgReader = new BinaryReader(orgStream);

            var outStream = new MemoryStream();
            var outReader = new BinaryReader(outStream);
            var outWriter = new BinaryWriter(outStream);

            foreach (var cmd in _assembly)
            {
                cmd.NewAddr = Convert.ToInt32(outStream.Position);

                orgStream.Position = cmd.Addr + 2;

                switch (cmd.Code)
                {
                    case 0x0000:
                    {
                        if (translation.TryGetValue(cmd.Addr, out var text))
                        {
                            var index = orgReader.ReadUInt16();
                            var voice = string.Empty;

                            if (orgReader.ReadUInt16() == 0x565C)
                                voice = orgReader.ReadSymbolTerminatedString('.', inputEncoding);
                            else
                                orgStream.Position -= 2;

                            outWriter.Write(Convert.ToUInt16(cmd.Code));
                            outWriter.Write(index);

                            if (!string.IsNullOrEmpty(voice))
                            {
                                outWriter.WriteUInt16(0x565C);
                                outWriter.WriteSymbolTerminatedString(voice, '.', outputEncoding);
                            }

                            outWriter.WriteNullTerminatedString(text, outputEncoding);
                        }
                        else
                        {
                            goto default;
                        }

                        break;
                    }
                    case 0x0300:
                    {
                        if (translation.TryGetValue(cmd.Addr, out var text))
                        {
                            outWriter.Write(Convert.ToUInt16(cmd.Code));
                            outWriter.WriteNullTerminatedString(text, outputEncoding);
                        }
                        else
                        {
                            goto default;
                        }

                        break;
                    }
                    case 0x0301:
                    {
                        if (translation.TryGetValue(cmd.Addr, out var text))
                        {
                            orgReader.ReadNullTerminatedString(inputEncoding);

                            var target = orgReader.ReadInt32();

                            outWriter.Write(Convert.ToUInt16(cmd.Code));
                            outWriter.WriteNullTerminatedString(text, outputEncoding);
                            outWriter.Write(target);
                        }
                        else
                        {
                            goto default;
                        }

                        break;
                    }
                    default:
                    {
                        outWriter.Write(_script, cmd.Addr, cmd.Size);
                        break;
                    }
                }
            }

            var commandMap = _assembly.ToDictionary(x => x.Addr);

            foreach (var cmd in _assembly)
            {
                outStream.Position = cmd.NewAddr + 2;

                switch (cmd.Code)
                {
                    case 0x0301:
                    {
                        outReader.ReadNullTerminatedString(outputEncoding);

                        var target = outReader.ReadInt32();
                        var newTarget = commandMap[target].NewAddr;

                        outStream.Position -= 4;
                        outWriter.Write(newTarget);

                        break;
                    }
                    case 0x0305:
                    {
                        outReader.ReadInt16();
                        outReader.ReadInt16();
                        outReader.ReadInt16();
                        outReader.ReadInt16();
                        outReader.ReadInt16();

                        var target = outReader.ReadInt32();
                        var newTarget = commandMap[target].NewAddr;

                        outStream.Position -= 4;
                        outWriter.Write(newTarget);

                        break;
                    }
                    case 0x0306:
                    {
                        outReader.ReadInt16();
                        outReader.ReadInt16();
                        outReader.ReadInt16();
                        outReader.ReadInt16();

                        var target = outReader.ReadInt32();
                        var newTarget = commandMap[target].NewAddr;

                        outStream.Position -= 4;
                        outWriter.Write(newTarget);

                        break;
                    }
                    case 0x8000:
                    {
                        var target = outReader.ReadInt32();
                        var newTarget = commandMap[target].NewAddr;

                        outStream.Position -= 4;
                        outWriter.Write(newTarget);

                        break;
                    }
                    case 0x8001:
                    {
                        var target = outReader.ReadInt32();
                        var newTarget = commandMap[target].NewAddr;

                        outStream.Position -= 4;
                        outWriter.Write(newTarget);

                        break;
                    }
                }
            }

            _script = outStream.ToArray();
        }
    }
}
