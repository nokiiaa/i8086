using System;
using i8 = System.SByte;
using i16 = System.Int16;
using i32 = System.Int32;
using i64 = System.Int64;
using u8 = System.Byte;
using u16 = System.UInt16;
using u32 = System.UInt32;
using u64 = System.UInt64;
using System.Linq;

namespace i8086
{
    // Intel 8086 microprocessor
    // Instruction set: x86-16
    // Designed by: Intel
    // Release date: 1976-1978
    public partial class I8086
    {
        struct ModRM
        {
            public u8 Mode, Register, RM;
            public static implicit operator ModRM(u8 b) => new ModRM { Mode = b.GetBits(6, 2), Register = b.GetBits(3, 3), RM = b.GetBits(0, 3) };
        }
     
        u8 Fetch()    => Peek(CS, IP++);
        u16 Fetch16() { IP += 2; return Peek16(CS, (u16)(IP - 2)); }


        /// <summary>
        /// Returns a certain segment register, given an index.
        /// </summary>
        /// <param name="num">The register index.</param>
        u16 SegRegister(int index)
        {
            if (index > 3)
                throw new Exception($"Invalid segment register index {index}");
            return new u16[] { ES, CS, SS, DS }[index];
        }

        /// <summary>
        /// Writes to a certain segment register, given an index and a value.
        /// </summary>
        /// <param name="num">The register index.</param>
        /// <param name="value">The value to write to the register.</param>
        void SegRegisterWrite(int index, u16 val)
        {
            switch (index)
            {
                case 0: ES = val; break;
                case 1: CS = val; break;
                case 2: SS = val; break;
                case 3: DS = val; break;
                default: throw new Exception($"Invalid segment register index {index}");
            }
        }


        /// <summary>
        /// Returns a certain 8/16-bit register, given an index into the regs/regs16 array and the bit count.
        /// </summary>
        /// <param name="num">The register index.</param>
        /// <param name="u16">Whether to pick from the regs or regs16 array.</param>
        u16 Register(int num, bool u16 = false) 
        {
            var regs16 = new u16[] { AX, CX, DX, BX, SP, BP, SI, DI };
            var regs   = new u8 [] { AL, CL, DL, BL, AH, CH, DH, BH };
            if (num > 7)
                throw new Exception($"Invalid register number {num}");
            else
                return u16 ? regs16[num] : regs[num];
        }

        /// <summary>
        /// Writes to a certain 8/16-bit register, given an index into the regs/regs16 array, bit count and the value.
        /// </summary>
        /// <param name="num">The register index.</param>
        /// <param name="u16">Whether to pick from the regs or regs16 array.</param>
        /// <param name="value">The value to write to the register.</param>
        void RegisterWrite(int num, u16 value, bool u16 = false)
        {
            if (value > 0xFF && !u16)
                throw new Exception("Invalid value for 8-bit register write.");
            switch (num)
            {
                case 0: if (u16) AX = value; else AL = (u8)value; break;
                case 1: if (u16) CX = value; else CL = (u8)value; break; 
                case 2: if (u16) DX = value; else DL = (u8)value; break; 
                case 3: if (u16) BX = value; else BL = (u8)value; break; 
                case 4: if (u16) SP = value; else AH = (u8)value; break; 
                case 5: if (u16) BP = value; else CH = (u8)value; break; 
                case 6: if (u16) SI = value; else DH = (u8)value; break; 
                case 7: if (u16) DI = value; else BH = (u8)value; break;
                default: throw new Exception($"Invalid register number {num}");
            }
        }

        u16 AddrMode(ModRM modrm, u16 segreg, u16 val = 0, bool postFetchValue = false, bool write = false, bool u16 = false, bool lea = false, bool incIP = true)
        {
            Func<u16, u16, u16> access = (offset, value) =>
            {
                if (lea)
                    return offset; // Don't read value at offset
                if (u16)
                {
                    if (write)
                        return Poke16(segreg, offset, value);
                    else
                        return Peek16(segreg, offset);
                }
                else
                {
                    if (write)
                        return Poke(segreg, offset, (u8)value);
                    else
                        return Peek(segreg, offset);
                }
            };
            if (modrm.Mode == 0)
            {
                switch (modrm.RM)
                {
                    case 0: return access((u16)(BX + SI), val);
                    case 1: return access((u16)(BX + DI), val);
                    case 2: return access((u16)(BP + SI), val);
                    case 3: return access((u16)(BP + DI), val);
                    case 4: return access(SI, val);
                    case 5: return access(DI, val);
                    case 6: u16 v = access(Fetch16(), postFetchValue ? (u16 ? Fetch16() : Fetch()) : val);
                            if (!incIP) IP -= 2; // Revert IP increment
                            return v;
                    case 7: return access(BX, val);
                }
            }
            else if (modrm.Mode == 1 || modrm.Mode == 2)
            {
                u16 disp = 0;
                if (modrm.Mode == 1)
                {
                    disp = Fetch();
                    if (disp.Bit(7)) disp |= 0xFF00; // Sign-extend
                }
                else
                    disp = Fetch16();

                if (postFetchValue)
                {
                    val = u16 ? Fetch16() : Fetch();
                    if (!incIP) IP -= u16 ? (u16)2 : (u16)1; // Revert IP increment
                }

                switch (modrm.RM)
                {
                    case 0: return access((u16)(BX + SI + disp), val);
                    case 1: return access((u16)(BX + DI + disp), val);
                    case 2: return access((u16)(BP + SI + disp), val);
                    case 3: return access((u16)(BP + DI + disp), val);
                    case 4: return access((u16)(SI + disp), val);
                    case 5: return access((u16)(DI + disp), val);
                    case 6: return access((u16)(BP + disp), val);
                    case 7: return access((u16)(BX + disp), val);
                }
            }
            else if (modrm.Mode == 3) // Two-register instruction.
            {
                if (!write)
                    return Register(modrm.RM, u16);
                else
                {
                    RegisterWrite(modrm.RM, val, u16);
                    return val;
                }
            }
            throw new Exception($"Unknown addressing mode {modrm.Mode}");
        }

        u8[] stringOps = new u8[] { 0xA4, 0xA5, 0xA6, 0xA7, 0xAA, 0xAB, 0xAC, 0xAD, 0xAE, 0xAF };

        // Main instruction parser and executor.
        public void RunInstruction()
        {
            try
            {
                ModRM rm = new ModRM();
                u8 opcode = Fetch();

                u16 segOverride = DS;

                // Get prefix chain
                u8 repPrefix = 0; // Either 0 (no REP prefix), 1 (REPZ) or (REPNZ)
                while ((opcode & 0b11100111) == 0b00100110 || opcode == 0xF2 || opcode == 0xF3 || opcode == 0xF0)
                {
                    // Segment override prefix.
                    if ((opcode & 0b11100111) == 0b00100110) // Pick index from bits 3-4 of the prefix byte to set segment override.
                        segOverride = SegRegister(opcode.GetBits(3, 2));
                    // REP prefixes
                    else if (opcode == 0xF2) // REPNZ/REPNE
                        repPrefix = 2;
                    else if (opcode == 0xF3) // REPZ/REPE/REP
                        repPrefix = 1;

                    opcode = Fetch(); // Refetch opcode
                }

                bool lsb = opcode.LSB();

                if (opcode.GetBits(3, 5) == 0b01000 ||
                    opcode.GetBits(3, 5) == 0b01011 ||
                    opcode.GetBits(3, 5) == 0b01001 ||
                    opcode.GetBits(3, 5) == 0b01010 ||
                    opcode.GetBits(3, 5) == 0b10010)
                    OpsRegNum(opcode);
                else if (opcode.GetBits(3, 5) == 0x16) // MOV Ib to 8-bit register
                    RegisterWrite(opcode.GetBits(0, 3), Fetch());
                else if ((opcode & 0xF0) == 0x70) // Conditional relative jump
                    RelJump(opcode & 0xF);
                else if ((opcode & 0xF0) < 0x40 && ((opcode & 7) < 6)) // GRP1 instructions with predefined addressing modes
                    OpsGRP1(opcode, segOverride, lsb);
                else if (opcode.GetBits(3, 5) == 0x17) // MOV Iv to 16-bit register
                    RegisterWrite(opcode.GetBits(0, 3), Fetch16(), true);
                else if (stringOps.Contains(opcode))
                    StringInstruction(opcode, repPrefix);
                else switch (opcode)
                    {
                        case 0x86:
                        case 0x87: // XCHG Gb/v, Eb/v
                            rm = Fetch();
                            u16 r = Register(rm.Register, lsb);
                            RegisterWrite(rm.Register, AddrMode(rm, segOverride, u16: lsb, incIP: false), u16: lsb);
                            AddrMode(rm, segOverride, r, write: true, u16: lsb);
                            break;
                        case 0x8E: // MOV Sw, Ew
                            rm = Fetch();
                            SegRegisterWrite(rm.Register, AddrMode(rm, segOverride, u16: true));
                            break;
                        case 0x8C: // MOV Ew, Sw
                            rm = Fetch();
                            AddrMode(rm, segOverride, SegRegister(rm.Register), u16: true, write: true);
                            break;
                        case 0x9C: Push(Flags); break; // PUSHF
                        case 0x9D: Flags = Pop(); break; // POPF
                        case 0xE6: Out8(Fetch(), AL); break; // OUT Ib, AL
                        case 0xE7: Out16(Fetch(), AX); break; // OUT Ib, AX
                        case 0xEE: Out8(DX, AL); break; // OUT DX, AL
                        case 0xEF: Out16(DX, AX); break; // OUT DX, AX
                        case 0xE4: AL = In8(Fetch()); break; // IN AL, Ib
                        case 0xE5: AX = In16(Fetch()); break; // IN AX, Ib
                        case 0xEC: AL = In8(DX); break; // IN AL, DX
                        case 0xED: AX = In16(DX); break; // IN AX, DX
                        case 0xCC: Int(3); break; // Short for CD 03
                        case 0xCD: Int(Fetch()); break; // INT xx
                        case 0xCF: IRET(); break; // IRET
                        case 0x8A: // MOV Gb, Eb
                            rm = Fetch();
                            RegisterWrite(rm.Register, AddrMode(rm, segOverride));
                            break;
                        case 0x8B: // MOV Gv, Ev
                            rm = Fetch();
                            RegisterWrite(rm.Register, AddrMode(rm, segOverride, u16: true), u16: true);
                            break;
                        case 0x88: // MOV Eb, Gb
                            rm = Fetch();
                            AddrMode(rm, segOverride, Register(rm.Register), write: true);
                            break;
                        case 0x89: // MOV Ev, Gv
                            rm = Fetch();
                            AddrMode(rm, segOverride, Register(rm.Register, u16: true), write: true, u16: true);
                            break;
                        case 0xC6:
                        case 0xC7: // MOV Eb/v, Ib/v
                            rm = Fetch();
                            AddrMode(rm, segOverride, postFetchValue: true, write: true, u16: lsb);
                            break;
                        case 0x8D: // LEA Gv, M
                            rm = Fetch();
                            RegisterWrite(rm.Register, AddrMode(rm, segOverride, u16: true, lea: true), u16: true);
                            break;
                        case 0x80:
                        case 0x81:
                        case 0x82:
                        case 0x83: // GRP1 Eb/v, Ib/v
                            rm = Fetch();
                            u16 dest = AddrMode(rm, segOverride, u16: lsb);
                            u16 value = lsb && !(opcode == 0x83) ? Fetch16() : Fetch();
                            if (opcode == 0x83) value = value.Extend();
                            u16 borrow = IPBorrow(rm, lsb ? 2 : 1);
                            AddrMode(rm, segOverride, GRP1(rm.Register, dest, value, lsb), write: true, u16: lsb, incIP: false);
                            IP += borrow;
                            break;
                        case 0xF4:
                            Halted = true; break;
                        case 0xFE: // INC/DEC Eb
                            u8 b = 1;
                            rm = Fetch();
                            if (rm.Register == 0)
                                b = 1;
                            else if (rm.Register == 1)
                                b = 0xFF; // -1 in 8-bit hexadecimal
                            else
                                throw new Exception($"Illegal mode opcode 0xFE (must be INC or DEC)");
                            u8 a = (u8)AddrMode(rm, segOverride, incIP: false);
                            u8 res = (u8)(a + b);
                            ZF = Zero(res);
                            PF = Parity(res);
                            SF = Sign(res);
                            OF = Overflow(a, b, res);
                            AF = Adjust(a, b);
                            AddrMode(rm, segOverride, res, write: true);
                            break;
                        case 0x06: Push(ES); break; // PUSH ES
                        case 0x07: ES = Pop(); break; // POP ES
                        case 0x16: Push(SS); break; // PUSH SS
                        case 0x17: SS = Pop(); break; // POP SS (would corrupt memory on actual hardware)
                        case 0x0E: Push(CS); break; // PUSH CS
                        case 0x0F: CS = Pop(); break; // POP CS (why would you ever do that anyway)
                        case 0x1E: Push(DS); break; // PUSH DS
                        case 0x1F: DS = Pop(); break; // POP DS
                        case 0x8F: // POP Ev
                            rm = Fetch();
                            AddrMode(rm, segOverride, Pop(), write: true, u16: true);
                            break;
                        case 0x9E: Flags = Flags.SetBits(0, AH, 8); break; // SAHF
                        case 0x9F: AH = (u8)Flags.GetBits(0, 8); break; // LAHF
                        case 0xC3: RET(); break; // Near RET
                        case 0xCB: RETF(); break; // Far RET
                        case 0xC2: // Near RET with POP Iw bytes
                            u16 bytes = Fetch16();
                            RET();
                            SP += bytes;
                            break;
                        case 0xCA: // Fat RET with POP Iw bytes
                            bytes = Fetch16();
                            RETF();
                            SP += bytes;
                            break;
                        case 0xCE: INTO(); break; // INTO
                        case 0xE9: RelJump(u16: true); break; // JMP Jw
                        case 0xEB: RelJump(); break; // JMP Jb
                        case 0xE8: // CALL Jw
                            Push(CS);
                            Push((u16)(IP + 2));
                            RelJump(u16: true);
                            break;
                        case 0x9A: // CALL Ap
                            Push(CS);
                            Push((u16)(IP + 4));
                            IP = Fetch16();
                            CS = Fetch16();
                            break;
                        case 0xEA: // JMP Ap
                            IP = Fetch16();
                            CS = Fetch16();
                            break;
                        case 0xC4: // LES/LDS Gv, Mp
                        case 0xC5:
                            rm = Fetch();
                            u16 pointer = Fetch16();
                            RegisterWrite(rm.Register, Peek16(segOverride, pointer), u16: true);
                            SegRegisterWrite(lsb ? 3 : 0 /* Write to ES or DS depending on the opcode's first bit */,
                                             Peek16(segOverride, (u16)(pointer + 2)));
                            break;
                        case 0xF8: CF = false; break; // CLC
                        case 0xF9: CF = true;  break; // STC
                        case 0xFA: IF = false; break; // CLI
                        case 0xFB: IF = true;  break; // STI
                        case 0xFC: DF = false; break; // CLD
                        case 0xFD: DF = true;  break; // STD

                        case 0xD7: AL = Peek(segOverride, (u16)(BX + AL)); break; // XLAT

                        case 0xD4: AAM(); break; // AAM
                        case 0xD5: AAD(); break; // AAD
                        case 0x2F: DAS(); break; // DAS
                        case 0x3F: AAS(); break; // AAS
                        case 0x37: AAA(); break; // AAA
                        case 0x27: DAA(); break; // DAA

                        case 0xD0: // GRP2 Eb, 1
                            rm = Fetch();
                            AddrMode(rm, segOverride, GRP2(rm.Register, AddrMode(rm, segOverride, incIP: false), 1), write: true);
                            break;
                        case 0xD1: // GRP2 Ev, 1
                            rm = Fetch();
                            AddrMode(rm, segOverride, GRP2(rm.Register, AddrMode(rm, segOverride, incIP: false, u16: true), 1, u16: true),
                                    write: true, u16: true);
                            break;
                        case 0xD2: // GRP2 Eb, CL
                            rm = Fetch();
                            AddrMode(rm, segOverride, GRP2(rm.Register, AddrMode(rm, segOverride, incIP: false), CL),
                                     write: true);
                            break;
                        case 0xD3: // GRP2 Ev, CL
                            rm = Fetch();
                            AddrMode(rm, segOverride, GRP2(rm.Register, AddrMode(rm, segOverride, incIP: false, u16: true), CL, u16: true),
                                     write: true, u16: true);
                            break;
                        case 0x98: // CBW, sign extend AL to AX
                            AX = AX.Extend();
                            break;
                        case 0x99: // CWD, sign extend AX to DX
                            DX = (u16)(AX.MSB() ? 0xFFFF : 0);
                            break;
                        case 0x84: // TEST Eb, Gb
                            rm = Fetch();
                            Test(Register(rm.Register), AddrMode(rm, segOverride));
                            break;
                        case 0x85: // TEST Ev, Gv
                            rm = Fetch();
                            Test(Register(rm.Register, u16: true), AddrMode(rm, segOverride, u16: true), u16: true);
                            break;
                        case 0xA8: Test(AL, Fetch()); break; // TEST AL, Ib
                        case 0xA9: Test(AX, Fetch16(), u16: true); break; // TEST AX, Iv
                        case 0xF5: CF = !CF; break; // CMC
                        case 0xA0: AL = Peek(segOverride, Fetch16()); break; // MOV AL Ob
                        case 0xA1: AX = Peek16(segOverride, Fetch16()); break; // MOV AX Ov
                        case 0xA2: Poke(segOverride, Fetch16(), AL); break; // MOV Ob AL
                        case 0xA3: Poke16(segOverride, Fetch16(), AX); break; // MOV Ov AX
                        case 0xE3: if (CX == 0) RelJump(); break; // JCXZ
                        case 0xE2: if (--CX != 0 && !ZF) RelJump(); break; // LOOPNZ
                        case 0xE1: if (--CX != 0 && ZF) RelJump(); break; // LOOPZ
                        case 0xE0: if (--CX != 0) RelJump(); break; // LOOP
                        case 0xFF: GRP5(segOverride); break;                         // GRP5 Ev
                        case 0xF6: GRP3(segOverride); break; // GRP3
                        case 0xF7: GRP3(segOverride, u16: true); break; // GRP4
                        default: throw new Exception($"Illegal opcode 0x{opcode:x2}");
                    }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Sorry, but an exception has occured while executing this instruction:\r\n{ex}");
                Halted = true;
            }
        } 

        void RelJump(int cond = -1, bool u16 = false)
        {
            u16 disp;
            if (!u16)
            {
                disp = Fetch();
                if (disp.Bit(7)) disp |= 0xFF00; // Sign-extend
            }
            else
                disp = Fetch16();
            bool condition = false;
            switch (cond)
            {
                case 0x0: condition = OF; break;
                case 0x1: condition = !OF; break;
                case 0x2: condition = CF; break;
                case 0x3: condition = !CF; break;
                case 0x4: condition = ZF; break;
                case 0x5: condition = !ZF; break;
                case 0x6: condition = CF || ZF; break;
                case 0x7: condition = !CF && !ZF; break;
                case 0x8: condition = SF; break;
                case 0x9: condition = !SF; break;
                case 0xA: condition = PF; break;
                case 0xB: condition = !PF; break;
                case 0xC: condition = OF != SF; break;
                case 0xD: condition = OF == SF; break;
                case 0xE: condition = ZF || SF != OF; break;
                case 0xF: condition = !ZF && SF == OF; break;
                case -1:  condition = true; break;
                default: throw new Exception("Invalid condition for relative jump");
            }
            if (condition)
                IP += disp;
        }

        // Adjust After Addition
        void AAA() 
        {
            if (((AL & 0xF) > 9) || AF)
            {
                AX += 0x106;
                AF = true;
                CF = true;
            }
            else
            {
                AF = false;
                CF = false;
            }
            AL &= 0xF;
        }

        // Decimal Adjust After Addition
        void DAA()
        {
            if (((AL & 0xF) > 9) || AF)
            {
                AL += 6;
                AF = true;
            }
            else
                AF = false;

            if ((AL > 0x9F) || CF)
            {
                AL += 0x60;
                CF = true;
            }
            else
                CF = false;
        }

        // Adjust After Multiplication
        void AAM()
        {
            u8 val = Fetch();
            AH = (u8)(AL / val);
            AL %= val;
            SF = Sign(AL);
            PF = Parity(AL);
            ZF = Zero(AL);
        }

        // Adjust After Division
        void AAD()
        {
            u8 val = Fetch();
            u8 al = AL, ah = AH;
            AL = (u8)(al + (ah * val));
            AH = 0;
            SF = Sign(AL);
            PF = Parity(AL);
            ZF = Zero(AL);
        }

        void DAS()
        {
            u8 oAL = AL;
            bool oCF = CF;
            CF = false;

            if ((AL & 0x0F) > 9 || AF)
            {
                CF = oCF || ((AL - 6) >> 8).Bit();
                AL -= 6;
                AF = true;
            }
            else 
                 AF = false;

            if ((oAL > 0x99) || oCF)
            {
                AL -= 0x60;
                CF = true;
            }
        }

        void AAS()
        {
            if ((AL & 0x0F) > 9 || AF)
            {
                AX -= 6;
                AH--;
                AF = true;
                CF = true;
            }
            else 
            {
                CF = false;
                AF = false;
            }
            AL &= 0x0F;
        }

        void IRET()
        {
            IP = Pop();
            CS = Pop();
            Flags = Pop();
        }

        void INTO()
        {
            if (OF)
                Int(4);
        }

        void RETF()
        {
            IP = Pop();
            CS = Pop();
        }

        void RET() => IP = Pop();

        void OpsGRP1(u8 opcode, u16 segment, bool u16 = false)
        {
            ModRM rm = new ModRM();
            int column = opcode >> 4;
            column *= 2;
            if (opcode.Bit(3)) column++;
            if ((opcode & 7) < 4)
                rm = Fetch();
            u16 reg = Register(rm.Register, u16: u16);
            switch (opcode & 7) // Addressing mode
            {
                case 0: AddrMode(rm, segment, GRP1(column, AddrMode(rm, segment, write: true, incIP: false), reg), write: true); break;
                case 1:
                    AddrMode(rm, segment, GRP1(column,
                    AddrMode(rm, segment, u16: true, incIP: false), reg, true), write: true, u16: true); break;
                case 2: RegisterWrite(rm.Register, GRP1(column, reg, AddrMode(rm, segment))); break;
                case 3: RegisterWrite(rm.Register, GRP1(column, reg, AddrMode(rm, segment, u16: true), true), true); break;
                case 4: AL = (u8)GRP1(column, AL, Fetch()); break;
                case 5: AX = GRP1(column, AX, Fetch16(), true); break;
            }
        }

        void OpsRegNum(u8 opcode)
        {
            int regnum = opcode.GetBits(0, 3);
            int op = opcode.GetBits(3, 5);

            if (op == 0x0A) // PUSH
                Push(Register(regnum, true));
            else if (op == 0x0B) // POP
                RegisterWrite(regnum, Pop(), true);
            else if (op == 0x08 || op == 0x09) // INC/DEC
            {
                u16 a = Register(regnum, true);
                u16 b = (u16)(op == 0x08 ? 1 : 0xFFFF);
                u16 res = (u16)(Register(regnum, true) + b);
                RegisterWrite(regnum, res, true);
                ZF = Zero(res);
                PF = Parity(res);
                SF = Sign(res, true);
                OF = Overflow(a, b, res, true);
                AF = Adjust(a, b);
            }
            else if (op == 0x12) // XCHG reg, AX
            {
                u16 olAX = AX;
                AX = Register(regnum, true);
                RegisterWrite(regnum, olAX, true);
            }
        }

        u16 GRP1(int column, u16 dest, u16 src, bool u16 = false)
        {
            u16 res = 0;

            switch (column)
            {
                case 0: res = (u16)(dest + src);                                                 break; // ADD
                case 1: res = (u16)(dest | src);                                                 break; // OR 
                case 2: res = GRP1(0, dest, (u16)(src + (CF ? 1 : 0)), u16);                     break; // ADC (Add With Carry)
                case 3: res = GRP1(5, dest, (u16)(src + (CF ? 1 : 0)), u16);                     break; // SBB (Subtract With Borrow)
                case 4: res = (u16)(dest & src);                                                 break; // AND
                case 5: res = GRP1(0, dest, (u16)(u16 ? (~src + 1) : ((~src + 1) & 0xFF)), u16); break; // SUB
                case 6: res = (u16)(dest ^ src);                                                 break; // XOR
                case 7: res = dest; GRP1(5, dest, src, u16);                                     break; // CMP
            }

            if (!u16)
                res &= 0xFF;

            if (column == 6 || column == 4 || column == 1)
            {
                OF = false;
                CF = false;
                SF = Sign(res, u16);
                PF = Parity(res);
                ZF = Zero(res);
            }
            else if (column == 0)
            {
                ZF = Zero(res);
                SF = Sign(res, u16);
                PF = Parity(res);
                AF = Adjust(dest, src);
                CF = Carry((u32)(dest + src), u16);
                OF = Overflow(dest, src, res, u16);
            }

            return res;
        }

        u16 GRP2(int column, u16 dest, u16 count, bool u16 = false)
        {
            u16 tempDest = dest;
            if (column < 4) // Rotate
            {
                switch (column)
                {
                    case 0: // ROL
                        for (int i = 0; count > i; i++)
                            dest = (u16)((dest << 1) | dest.Bit(u16 ? 15 : 7).Int());
                        CF = dest.LSB();
                        if (count == 1)
                            OF = dest.Bit(u16 ? 15 : 7) ^ CF;
                        break;
                    case 1: // ROR
                        for (int i = 0; count > i; i++)
                            dest = (u16)((dest >> 1) | dest.LSB().Int());
                        CF = dest.Bit(u16 ? 15 : 7);
                        if (count == 1) OF = dest.Bit(u16 ? 15 : 7) ^ dest.Bit(u16 ? 14 : 6);
                        break;
                    case 2: // RCL
                        for (int i = 0; count > i; i++)
                        {
                            bool cf = dest.Bit(u16 ? 15 : 7);
                            dest = (u16)((dest << 1) | CF.Int());
                            CF = cf;
                        }
                        if (count == 1) OF = dest.Bit(u16 ? 15 : 7) ^ CF;
                        break;
                    case 3: // RCR
                        if (count == 1) OF = dest.Bit(u16 ? 15 : 7) ^ CF;
                        for (int i = 0; count > i; i++)
                        {
                            bool cf = dest.LSB();
                            dest = (u16)((dest >> 1) | (CF.Int() << (u16 ? 15 : 7)));
                            CF = cf;
                        }
                        break;
                }
            }
            else // Shift
            {
                bool shl = column == 4;
                bool shr = column == 5;
                bool sar = column == 7;
                for (int i = 0; count > i; i++)
                {
                    if (shl)
                    {
                        CF = dest.Bit(u16 ? 15 : 7);
                        dest <<= 1;
                    }
                    else
                    {
                        CF = dest.LSB();
                        if (shr)
                            dest >>= 1;
                        else
                            dest = (u16)((i16)dest / 2);
                    }
                }

                if (count == 1)
                {
                    if (shl) OF = dest.Bit(u16 ? 15 : 7) ^ CF;
                    else if (sar) OF = false;
                    else OF = tempDest.Bit(u16 ? 15 : 7);
                }

                ZF = Zero(dest);
                SF = Sign(dest, u16);
                PF = Parity(dest);
            }

            if (!u16)
                dest &= 0xFF;

            return dest;
        }

        void GRP5(u16 segment)
        {
            ModRM rm = Fetch();
            switch (rm.Register)
            {
                case 2: // CALL
                case 3:
                case 4: // JMP
                case 5:
                    if (rm.Register < 4) // CALL
                    {
                        Push(CS);
                        Push((u16)(IP + IPBorrow(rm)));
                    }
                    if (rm.Register == 2 || rm.Register == 4)
                        IP = AddrMode(rm, segment, u16: true);
                    else
                    {
                        u16 ip = Fetch16();
                        u16 seg = Fetch16();
                        IP = Peek16(seg, ip);
                        CS = Peek16(seg, (u16)(ip + 2));
                    }
                    break;
                case 6: // PUSH
                    Push(AddrMode(rm, segment, u16: true));
                    break;
                case 0:
                case 1:
                    u16 value = (u16)(rm.Register == 0 ? 1 : 0xFFFF); // INC/DEC
                    u16 mem = AddrMode(rm, segment, u16: true, incIP: false);
                    u16 result = (u16)(mem + value);
                    AddrMode(rm, segment, result, write: true, u16: true);
                    ZF = Zero(result);
                    PF = Parity(result);
                    SF = Sign(result);
                    OF = Overflow(mem, value, result);
                    AF = Adjust(mem, value);
                    break;
            }
        }

        void GRP3(u16 segment, bool u16 = false)
        {
            ModRM rm = Fetch();
            int mode = rm.Register;
            switch (mode)
            {
            case 0: // TEST Eb/v, Ib/v
                    u16 src = AddrMode(rm, segment, u16: u16);
                    u16 dest = u16 ? Fetch16() : Fetch();
                    Test(src, dest, u16);
                    break;
            case 2: // NOT/NEG
            case 3: 
                    u16 inv = (u16)~AddrMode(rm, segment, u16: u16, incIP: false);
                    if (mode == 3) inv++;
                    AddrMode(rm, segment, inv, write: true, u16: u16);
                    break;
                case 4: // MUL
                case 5: // IMUL
                    u16 value = AddrMode(rm, segment, u16: u16);
                    if (!u16)
                    {
                        u16 res = (u16)(AL * value);
                        if (mode == 4)
                            AX = (u16)(AL * value);
                        else
                            AX = (u16)(i16)((i8)AL * (i8)value);
                        CF = Carry(res);
                        OF = CF;
                    }
                    else
                    {
                        if (mode == 4)
                        {
                            u32 res = (u32)(AX * value);
                            DX = (u16)(res >> 16);
                            AX = (u16)res;
                        }
                        else
                        {
                            u32 res = (u32)((i16)AX * (i16)value);
                            DX = (u16)((i32)res >> 16);
                            AX = (u16)res;
                        }
                        CF = !Zero(DX);
                        OF = CF;
                    }
                    break; 
                case 6: // DIV
                case 7: // IDIV
                    value = AddrMode(rm, segment, u16: u16);
                    u16 quo = (u16)(AX / value);
                    u16 rem = (u16)(AX % value);
                    if (!u16)
                    {
                        if (mode == 6)
                        {
                            quo = (u8)(AX / value);
                            rem = (u8)(AX % value);
                        }
                        else
                        {
                            quo = (u8)((i16)AX / (i16)value);
                            rem = (u8)((i16)AX % (i16)value);
                        }
                        AL = (u8)quo;
                        AH = (u8)rem;
                    }
                    else
                    {
                        if (mode == 6)
                        {
                            quo = (u16)((u32)(DX << 16 | AX) / value);
                            rem = (u16)((u32)(DX << 16 | AX) % value);
                        }
                        else
                        {
                            quo = (u16)(((i16)DX << 16 | (i16)AX) / (i16)value);
                            rem = (u16)(((i16)DX << 16 | (i16)AX) % (i16)value);
                        }
                        AX = quo;
                        DX = rem;
                    }
                    break;
                default: throw new Exception($"Unknown GRP3 mode {rm.Register}");
            }
        }

        void Test(u16 a, u16 b, bool u16 = false)
        {
            CF = false;
            OF = false;
            ZF = Zero((u16)(a & b));
            SF = Sign((u16)(a & b), u16);
        }

        void StringInstruction(u8 opcode, int repPrefix)
        {
            while (true)
            {
                switch (opcode)
                {
                    case 0xA4: // MOVSB
                        Poke(ES, DI, Peek(DS, SI));
                        if (DF)
                            { SI--; DI--; }
                        else
                            { SI++; DI++; }
                        break;
                    case 0xA5: // MOVSW
                        Poke16(ES, DI, Peek16(DS, SI));
                        if (DF)
                            { SI -= 2; DI -= 2; }
                        else
                            { SI += 2; DI += 2; }
                        break;
                    case 0xA6: // CMPSB
                        GRP1(7, Peek(ES, DI), Peek(DS, SI));
                        if (DF)
                            { SI--; DI--; }
                        else
                            { SI++; DI++; }
                        break;
                    case 0xA7: // CMPSW
                        GRP1(7, Peek(ES, DI), Peek(DS, SI), u16: true);
                        if (DF)
                            { SI -= 2; DI -= 2; }
                        else
                            { SI += 2; DI += 2; }
                        break;
                    case 0xAA: Poke(DS, SI, AL); if (DF) SI--; else SI++; break; // STOSB
                    case 0xAB: Poke16(DS, SI, AX); if (DF) SI -= 2; else SI += 2; break; // STOSW
                    case 0xAC: AL = Peek(DS, SI); if (DF) SI--; else SI++; break; // LODSB
                    case 0xAD: AX = Peek16(DS, SI); if (DF) SI -= 2; else SI += 2; break; // LODSW
                    case 0xAE: GRP1(7, AL, Peek(DS, SI)); if (DF) SI--; else SI++; break; // SCASB
                    case 0xAF: GRP1(7, AX, Peek16(DS, SI)); if (DF) SI -= 2; else SI += 2; break; // SCASW
                }
                if (opcode == 0xA6 || opcode == 0xA7
                 || opcode == 0xAE || opcode == 0xAF)
                {
                    if (repPrefix == 2 && ZF) // REPNZ
                        break;
                    else if (repPrefix == 1 && !ZF)
                        break;
                }

                if (repPrefix == 0)
                    break;
                else if (--CX == 0)
                    break;
            }
        }

        // Flags.

        bool Overflow(u16 a, u16 b, u16 res, bool u16 = false) => (((a ^ res) & (b ^ res)) >> (u16 ? 15 : 7)).Bit();
        bool Carry(u32 res, bool u16 = false) => (res >> (u16 ? 16 : 8)).Bit();
        bool Sign(u16 res, bool u16 = false) => (res >> (u16 ? 15 : 7)).Bit();
        bool Zero(u16 res) => res == 0;
        bool Parity(u16 res)
        {
            bool p = false;
            p ^= res.Bit(0);
            p ^= res.Bit(1);
            p ^= res.Bit(2);
            p ^= res.Bit(3);
            p ^= res.Bit(4);
            p ^= res.Bit(5);
            p ^= res.Bit(6);
            p ^= res.Bit(7);
            return !p;
        }
        bool Adjust(u16 a, u16 b) => ((a & 0xF) + (b & 0xF) >> 4).Bit();

        // WARNING: this is a hack.
        u16 IPBorrow(ModRM rm, int immsize = 0)
        {
            if (rm.Mode < 3)
            {
                if (rm.Mode == 0 && rm.RM == 6)
                    return (u16)(immsize + 2);
                else if (rm.Mode == 1)
                    return (u16)(immsize + 1);
                else if (rm.Mode == 2)
                    return (u16)(immsize + 2);
            }
            return 0;
        }
    }
}
