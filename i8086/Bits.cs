using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace i8086
{
    // WARNING: I claim no responsibility for any moral, physical or other type of damage caused by the devilish code that is followed by this disclaimer.
    // Be cautious.
























































    public static class Bits
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetBits(this ulong value, int start, int count) => (value >> start) & ((1UL << count) - 1);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetBits(this long value, int start, int count) => (value >> start) & ((1L << count) - 1);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetBits(this uint value, int start, int count) => (value >> start) & ((1U << count) - 1);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBits(this int value, int start, int count) => (value >> start) & ((1 << count) - 1);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort GetBits(this ushort value, int start, int count) => (ushort)((value >> start) & ((1 << count) - 1));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short GetBits(this short value, int start, int count) => (short)((value >> start) & ((1 << count) - 1));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte GetBits(this byte value, int start, int count) => (byte)((value >> start) & ((1 << count) - 1));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte GetBits(this sbyte value, int start, int count) => (sbyte)((value >> start) & ((1 << count) - 1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong SetBit(this ulong value, int index, int bit) => value ^ (((ulong)-bit ^ value) & (1UL << index));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long SetBit(this long value, int index, int bit) => value ^ ((-bit ^ value) & (1L << index));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint SetBit(this uint value, int index, int bit) => value ^ (((uint)-bit ^ value) & (1U << index));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SetBit(this int value, int index, int bit) => value ^ ((-bit ^ value) & (1 << index));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort SetBit(this ushort value, int index, int bit) => (ushort)(value ^ (((ushort)-bit ^ value) & (1 << index)));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short SetBit(this short value, int index, int bit) => (short)(value ^ (((short)-bit ^ value) & (1 << index)));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte SetBit(this byte value, int index, int bit) => (byte)(value ^ (((byte)-bit ^ value) & (1 << index)));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte SetBit(this sbyte value, int index, int bit) => (sbyte)(value ^ (((byte)-bit ^ value) & (1 << index)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong SetBits(this ulong value, int index, int bits, int count) => (value & (~((1UL << count) - 1) << index))
                                                                                     | ((ulong)bits << index);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long SetBits(this long value, int index, int bits, int count) => (value & (~((1L << count) - 1) << index))
                                                                                     | ((long)bits << index);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint SetBits(this uint value, int index, int bits, int count) => (value & (~((1U << count) - 1U) << index))
                                                                                     | (uint)(bits << index);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SetBits(this int value, int index, int bits, int count) => (value & (~(1 << count - 1) << index))
                                                                                     | (bits << index);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort SetBits(this ushort value, int index, int bits, int count) => (ushort)((value & ~((ushort)((1 << count) - 1) << index))
                                                                                                 | (bits << index));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short SetBits(this short value, int index, int bits, int count) => (short)(value & (~(((short)(1 << count) - 1) << index))
                                                                                              | (bits << index));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte SetBits(this byte value, int index, int bits, int count) => (byte)(value & (~((1 << count - 1) << index))
                                                                                           | (bits << index));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte SetBits(this sbyte value, int index, int bits, int count) => (sbyte)(value & (~((1 << count - 1) << index))
                                                                                              | (bits << index));


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint EndianSwap(this uint value) => ((value & 0x000000FF) << 24) |
                                                          ((value & 0x0000FF00) << 8) |
                                                          ((value & 0x00FF0000) >> 8) |
                                                          ((value & 0xFF000000) >> 24);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int EndianSwap(this int value) => (int)((uint)((value & 0x000000FF) << 24) |
                                                             ((uint)(value & 0x0000FF00) << 8) |
                                                             ((uint)(value & 0x00FF0000) >> 8) |
                                                             ((uint)(value & 0xFF000000) >> 24));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort EndianSwap(this ushort value) => (ushort)(((value & 0xFF) << 8) | (value >> 8));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short EndianSwap(this short value) => (short)(((value & 0xFF) << 8) | (value >> 8));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Int(this bool value) => value ? 1 : 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Bit(this ulong d) => d != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Bit(this long d) => d != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Bit(this uint d) => d != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Bit(this int d) => d != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Bit(this ushort d) => d != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Bit(this short d) => d != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Bit(this byte d) => d != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Bit(this sbyte d) => d != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MSB(this ulong d) => d.Bit(63);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MSB(this long d) => d.Bit(63);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MSB(this uint d) => d.Bit(31);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MSB(this int d) => d.Bit(31);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MSB(this ushort d) => d.Bit(15);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MSB(this short d) => d.Bit(15);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MSB(this byte d) => d.Bit(7);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MSB(this sbyte d) => d.Bit(7);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LSB(this ulong d) => d.Bit(0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LSB(this long d) => d.Bit(0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LSB(this uint d) => d.Bit(0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LSB(this int d) => d.Bit(0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LSB(this ushort d) => d.Bit(0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LSB(this short d) => d.Bit(0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LSB(this byte d) => d.Bit(0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LSB(this sbyte d) => d.Bit(0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Extend(this ushort d) => (ushort)(d.Bit(7) ? d | 0xFF00 : d);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short Extend(this short d) => (short)(d.Bit(7) ? d | 0xFF00 : d);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Bit(this ulong d, int n) => ((d >> n) & 1).Bit();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Bit(this long d, int n) => ((d >> n) & 1).Bit();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Bit(this uint d, int n) => ((d >> n) & 1).Bit();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Bit(this int d, int n) => ((d >> n) & 1).Bit();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Bit(this ushort d, int n) => ((d >> n) & 1).Bit();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Bit(this short d, int n) => ((d >> n) & 1).Bit();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Bit(this byte d, int n) => ((d >> n) & 1).Bit();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Bit(this sbyte d, int n) => ((d >> n) & 1).Bit();

        public static T Make<T>(params (int, T)[] args)
        {
            ulong final = 0;
            foreach ((int, T) tuple in args)
                final |= (ulong)Convert.ChangeType(tuple.Item2, typeof(ulong)) << tuple.Item1;
            return (T)Convert.ChangeType(final, typeof(T));
        }

        public static T Make<T>(params bool[] bits)
        {
            ulong final = 0;
            int size = Marshal.SizeOf(typeof(T)) * 8;
            for (int i = bits.Length, j = 0; 0 < i; i--, j++)
                final |= (bits[j] ? 1ul : 0ul) << (i - 1);
            return (T)Convert.ChangeType(final, typeof(T));
        }

        public static T Make<T>(params int[] bits)
        {
            ulong final = 0;
            int size = Marshal.SizeOf(typeof(T)) * 8;
            for (int i = bits.Length, j = 0; 0 < i; i--, j++)
                final |= (ulong)(bits[j] & 1) << (i - 1);
            return (T)Convert.ChangeType(final, typeof(T));
        }
    }
}