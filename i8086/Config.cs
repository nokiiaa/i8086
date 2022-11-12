using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace i8086
{
    // A simple config file system, using which you can set the initial values of registers,
    // mappings of memory to files (you have only 1MiB of address space, of course) and privilege levels,
    // and other various settings.
    public static class Config
    {
        public struct Mapping
        {
            public uint Start;
            public uint End;
            public string Filename;
            public string Perms;
        }

        public static bool ReadKey;
        public static Dictionary<string, ushort> Regs = new Dictionary<string, ushort>()
        {
            { "ax", 0 }, { "cx", 0 }, { "dx", 0 }, { "bx", 0 },
            { "sp", 0 }, { "bp", 0 }, { "cs", 0 }, { "ds", 0 },
            { "es", 0 }, { "ip", 0 }, { "si", 0 }, { "di", 0 },
        };

        public static List<Mapping> Mappings = new List<Mapping>(); 

        public static void Parse(string config)
        {
            string[] lines = config.Split('\n');
            for (int i = 0; lines.Length > i; i++)
            {
                string[] spl = lines[i].Split('=');
                // I mean, what else would you use for such a simple thing?
                if (Regex.IsMatch(lines[i], @"^(ax|bx|cx|dx|es|ds|cs|ip|bp|sp|si|di)=[0-9a-f]{0,4}", RegexOptions.IgnoreCase))
                    Regs[spl[0].ToLower()] = ushort.Parse(spl[1].ToLower(), NumberStyles.HexNumber);
                else if (Regex.IsMatch(lines[i], @"[0-9a-f]{0,5}-[0-9a-f]{0,5}=(.+\..+|empty) (rw|r)\b", RegexOptions.IgnoreCase))
                {
                    var start = uint.Parse(spl[0].Split('-')[0].ToLower(), NumberStyles.HexNumber);
                    var end = uint.Parse(spl[0].Split('-')[1].ToLower(), NumberStyles.HexNumber);
                    var filename = spl[1].Split(' ')[0];
                    var perms = spl[1].Split(' ')[1];
                    Mappings.Add(new Mapping { Start = start, End = end, Filename = filename, Perms = perms });
                }
                else if (lines[i].ToLower() == "readkey")
                    ReadKey = true;
            }
        }
    }
}
