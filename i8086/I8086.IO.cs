using System;
using i8 = System.SByte;
using i16 = System.Int16;
using i32 = System.Int32;
using i64 = System.Int64;
using u8 = System.Byte;
using u16 = System.UInt16;
using u32 = System.UInt32;
using u64 = System.UInt64;
using System.Runtime.CompilerServices;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace i8086
{
    // Intel 8086 microprocessor
    // Instruction set: x86-16
    // Designed by: Intel
    // Release date: 1976-1978
    // Clock rate: 4MHz-16MHz
    public partial class I8086
    {
        public struct PortHandler<T>
        {
            public ushort PortNumber;
            public Func<T, bool, T> Access;
        }
        public bool Halted { get; set; }
        public u16 AX, BX, CX, DX, CS = 0xFFFF, DS, ES, SS, IP, SP, BP, SI, DI;
        public List<PortHandler<u8>> PortMappings8 = new List<PortHandler<u8>>();
        public List<PortHandler<u16>> PortMappings16 = new List<PortHandler<u16>>();
        public bool CF, PF, AF, ZF, SF, TF, IF, DF, OF;
        public u16 Flags
        {
            get => (u16)(CF.Int() << 0  | 
                         PF.Int() << 2  | 
                         AF.Int() << 4  | 
                         ZF.Int() << 6  |
                         SF.Int() << 7  | 
                         TF.Int() << 8  | 
                         IF.Int() << 9  | 
                         DF.Int() << 10 |
                         OF.Int() << 11);

            set
            {
                CF = value.Bit(0);
                PF = value.Bit(2);
                AF = value.Bit(4);
                ZF = value.Bit(6);
                SF = value.Bit(7);
                TF = value.Bit(8);
                IF = value.Bit(9);
                DF = value.Bit(10);
                OF = value.Bit(11);
            }
        }
        public u8 AH { get => (u8)(AX >> 8); set => AX = AX.SetBits(8, value, 8); }
        public u8 AL { get => (u8)AX;        set => AX = AX.SetBits(0, value, 8); }
        public u8 BH { get => (u8)(BX >> 8); set => BX = BX.SetBits(8, value, 8); }
        public u8 BL { get => (u8)BX;        set => BX = BX.SetBits(0, value, 8); }
        public u8 CH { get => (u8)(CX >> 8); set => CX = CX.SetBits(8, value, 8); }
        public u8 CL { get => (u8)CX;        set => CX = CX.SetBits(0, value, 8); }
        public u8 DH { get => (u8)(DX >> 8); set => DX = DX.SetBits(8, value, 8); }
        public u8 DL { get => (u8)DX;        set => DX = DX.SetBits(0, value, 8); }
        public BaseMemory Memory { get; private set; }
        public I8086() => Memory = new BaseMemory(0x00000, 0xFFFFF);
        public byte Peek(u16 segment, u16 offset) => Memory.ReadByte((u64)((segment << 4) + offset) & 0xFFFFF);
        public byte Poke(u16 segment, u16 offset, u8 value) => Memory.WriteByte((u64)((segment << 4) + offset), value);
        public u16 Peek16(u16 segment, u16 offset) 
            => (u16)(Memory.ReadByte((u64)((segment << 4) + offset) & 0xFFFFF)
            | (Memory.ReadByte((u64)((segment << 4) + offset + 1) & 0xFFFFF) << 8));
        public u16 Poke16(u16 segment, u16 offset, u16 value)
        {
            Memory.WriteByte((u64)((segment << 4) + offset)     & 0xFFFFF, (u8)(value & 0xFF));
            Memory.WriteByte((u64)((segment << 4) + offset + 1) & 0xFFFFF, (u8)(value >> 8  ));
            return value;
        }

        public void Int(byte number)
        {
            Push(Flags);
            IF = false; TF = false; AF = false;
            Push(CS);
            Push(IP);
            CS = Peek16(0x0000, (ushort)(number * 4 + 2));
            IP = Peek16(0x0000, (ushort)(number * 4));
        }

        public u16 Push(u16 value) => Poke16(SS, (u16)((SP -= 2) + 2), value); 
        public u16 Pop() => Peek16(SS, SP += 2);

        public void MapPort8(u16 portNumber, Func<u8, bool, u8> access) 
            => PortMappings8.Add(new PortHandler<u8> { Access = access, PortNumber = portNumber });
        public void MapPort16(u16 portNumber, Func<u16, bool, u16> access) 
            => PortMappings16.Add(new PortHandler<u16> { Access = access, PortNumber = portNumber });

        public u8 In8(u16 port)
        {
            var p = PortMappings8.Where(x => x.PortNumber == port);
            if (p == null)
                throw new Exception($"Unknown port 0x{port:X4}");
            return p.ToArray()[0].Access(0, false);
        }
        public u16 In16(u16 port)
        {
            var p = PortMappings16.Where(x => x.PortNumber == port);
            if (p == null)
                throw new Exception($"Unknown port 0x{port:X4}");
            return p.ToArray()[0].Access(0, false);
        }

        public u8 Out8(u16 port, u8 val)
        {
            var p = PortMappings8.Where(x => x.PortNumber == port);
            if (p == null)
                throw new Exception($"Unknown port 0x{port:X4}");
            return p.ToArray()[0].Access(val, true);
        }

        public u16 Out16(u16 port, u16 val)
        {
            var p = PortMappings16.Where(x => x.PortNumber == port);
            if (p == null)
                throw new Exception($"Unknown port 0x{port:X4}");
            return p.ToArray()[0].Access(val, true);
        }


        public string RegDump()
        {
            return $"AX={AX:x4}; BX={BX:x4}\r\n" +
                   $"CX={CX:x4}; DX={DX:x4}\r\n" +
                   $"CS={CS:x4}; DS={DS:x4}\r\n" +
                   $"ES={ES:x4}; IP={IP:x4}\r\n" +
                   $"SP={SP:x4}; BP={BP:x4}\r\n" +
                   $"SI={SI:x4}; DI={DI:x4}\r\n" + 
                   $"SS={SS:x4}; FL={Flags:x4}";
        }

        public string HexDump(u16 segment, u16 addr, int count, int perLine = 16)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("            00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");
            for (int i = 0; count > i; i++)
            {
                if (i % perLine == 0)
                    sb.Append($"{segment:X4}:{addr + i:X4} | ");
                sb.Append(Peek(segment, (u16)(addr + i)).ToString("X2") + " ");
                if (((i + 1) % perLine) == 0 && i != 0)
                {
                    sb.Append("| ");
                    int ad = addr + (i & ~15);
                    for (int j = 0; perLine > j; j++)
                        sb.Append( ((char)Peek(segment, (u16)(ad + j))).ToString()
                            .Select(x => x > 0x0D ? x : '.').ToArray());
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }
    }
}
