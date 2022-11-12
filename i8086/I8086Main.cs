using System;
using System.IO;

namespace i8086
{
    public static class I8086Main
    {
        static byte[] RAM = new byte[0xFFFFF];
        static void Main(string[] args)
        {
            string path = args.Length > 0 ? args[0] : "8086.cfg";

            if (File.Exists(path))
                Config.Parse(File.ReadAllText(path));
            else
            {
                Console.WriteLine("Couldn't find 8086.cfg.");
                Environment.Exit(-1);
            }

            var cpu = new I8086();

            // Set up registers
            cpu.AX = Config.Regs["ax"]; cpu.CX = Config.Regs["cx"];
            cpu.DX = Config.Regs["dx"]; cpu.BX = Config.Regs["bx"];
            cpu.SP = Config.Regs["sp"]; cpu.BP = Config.Regs["bp"];
            cpu.SI = Config.Regs["si"]; cpu.DI = Config.Regs["di"];
            cpu.CS = Config.Regs["cs"]; cpu.DS = Config.Regs["ds"];
            cpu.ES = Config.Regs["es"]; cpu.IP = Config.Regs["ip"];

            Func<ulong, byte, bool, byte> r = (addr, value, write) =>
            {
                Console.WriteLine(write);
                if (write)
                {
                    Console.WriteLine($"Access violation at physical address 0x{addr:X5}");
                    Console.WriteLine(cpu.RegDump());
                    Console.ReadKey(true);
                    Environment.Exit(-1);
                    return 0;
                }
                else
                    return RAM[addr];
            };

            Func<ulong, byte, bool, byte> rw = (addr, value, write) =>
            {
                if (write)
                    return RAM[addr] = value;
                else
                    return RAM[addr];
            };

            // Map memory to files
            foreach (var mapping in Config.Mappings)
            {
                if (!File.Exists(mapping.Filename) && mapping.Filename != "empty")
                {
                    Console.WriteLine($"Couldn't find {mapping.Filename}.");
                    Environment.Exit(-1);
                }
                else if (mapping.Filename != "empty")
                {
                    var bytes = File.ReadAllBytes(mapping.Filename);
                    Buffer.BlockCopy(bytes, 0, RAM, 0, bytes.Length);
                }

                cpu.Memory.Map(mapping.Start, mapping.End, mapping.Perms == "r" ? r : rw);
            }

            while (!cpu.Halted)
                cpu.RunInstruction();

            Console.WriteLine(cpu.RegDump());
            Console.WriteLine("Execution halted.");

            if (Config.ReadKey)
                 Console.ReadKey(true);
        }
    }
}
