using System;
using System.Collections.Generic;

namespace i8086
{
    // A handy extensible class for memory things.
    public class BaseMemory
    {
        public struct Handler
        {
            public ulong MinAddress;
            public ulong MaxAddress;
            public Func<ulong, byte, bool, byte> Access;
        }
        public List<Handler> Handlers { get; set; } = new List<Handler>();
        public ulong MinAddress { get; set; }
        public ulong MaxAddress { get; set; }
        public BaseMemory(ulong min, ulong max) { MinAddress = min; MaxAddress = max; }
        public void Map(ulong min, ulong max, Func<ulong, byte, bool, byte> access) => Handlers.Add(
            new Handler { MinAddress = min, MaxAddress = max, Access = access }
        );
        public virtual byte Access(ulong address, byte value = 0, bool write = false)
        {
            foreach (Handler h in Handlers)
                if (h.MaxAddress >= address && h.MinAddress <= address)
                    return h.Access(address, value, write);
            Console.WriteLine($"Unmapped memory location 0x{address:X}");
            return 0x00;
        }
        public virtual byte ReadByte(ulong address) => Access(address);
        public virtual byte WriteByte(ulong address, byte value) => Access(address, value, true);
        public virtual byte[] ReadBytes(ulong address, ulong count)
        {
            byte[] bytes = new byte[count];
            for (ulong i = address; address + count > i; i++)
                bytes[i - address] = ReadByte(i);
            return bytes;
        }
        public virtual byte[] WriteBytes(ulong address, byte[] bytes)
        {
            for (ulong i = address; address + (ulong)bytes.LongLength > i; i++)
                WriteByte(i, bytes[i - address]);
            return bytes;
        }
    }
}
