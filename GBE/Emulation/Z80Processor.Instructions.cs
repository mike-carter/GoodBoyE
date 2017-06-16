using System;

namespace GBE.Emulation
{
    partial class Z80Processor
    {
        internal struct Instruction
        {
            public Instruction(string dis, ushort oplen = 0, ushort ticks = 1, Action<int> op = null)
            {
                Disassembly = dis;
                OperandLength = oplen;
                Ticks = ticks;
                Operation = op;
            }

            public ushort OperandLength;
            public ushort Ticks;
            public string Disassembly;
            public Action<int> Operation;
        }

        /**
         * Creates the instruction lookup table
         */
        private Instruction[] InitInstructionSet()
        {
            return new Instruction[]
            {
                // 0x00
                new Instruction("NOP"),
                new Instruction("LD BC, {0:X4}", 2, 3, (n) => RegBC = (ushort)n),
                new Instruction("LD (BC), A", 0, 2, (n) => md[RegBC] = RegA),
                new Instruction("INC BC", 0, 2, (n) => RegBC++),
                new Instruction("INC B", 0, 1, (n) => RegB = Inc(RegB)),
                new Instruction("DEC B", 0, 1, (n) => RegB = Dec(RegB)),
                new Instruction("LD B, {0:X2}", 1, 2, (n) => RegB = (byte)n),
                new Instruction("RLCA", 0, 1, (n) => { RegA = RLC(RegA); ZeroFlag = false; }),
                new Instruction("LD ({0:X4}), SP", 2, 4, (n) => StoreWord(n, SP)),
                new Instruction("ADD HL, BC", 0, 2, (n) => AddHL(RegBC)),
                new Instruction("LD A, (BC)", 0, 2, (n) => RegA = md[RegBC]),
                new Instruction("DEC BC", 0, 2, (n) => RegBC--),
                new Instruction("INC C", 0, 1, (n) => RegC = Inc(RegC)),
                new Instruction("DEC C", 0, 1, (n) => RegC = Dec(RegC)),
                new Instruction("LD C, {0:X2}", 1, 2, (n) => RegC = (byte)n),
                new Instruction("RRCA", 0, 1, (n) => { RegA = RRC(RegA); ZeroFlag = false; }),

                // 0x10
                new Instruction("STOP", 0, 1, (n) => Stop()),
                new Instruction("LD DE, {0:X4}", 2, 3, (n) => RegDE = (ushort)n),
                new Instruction("LD (DE), A", 0, 2, (n) => md[RegDE] = RegA),
                new Instruction("INC DE", 0, 2, (n) => RegDE++),
                new Instruction("INC D", 0, 1, (n) => RegD = Inc(RegD)),
                new Instruction("DEC D", 0, 1, (n) => RegD = Dec(RegD)),
                new Instruction("LD D, {0:X2}", 1, 2, (n) => RegD = (byte)n),
                new Instruction("RLA", 0, 1, (n) => {  RegA = RL(RegA); ZeroFlag = false; }),
                new Instruction("JR {0:X2}", 1, 2, (n) => JumpRelative((byte)n)),
                new Instruction("ADD HL, DE", 0, 2, (n) => AddHL(RegDE)),
                new Instruction("LD A, (DE)", 0, 2, (n) => RegA = md[RegDE]),
                new Instruction("DEC DE", 0, 2, (n) => RegDE--),
                new Instruction("INC E", 0, 1, (n) => RegE = Inc(RegE)),
                new Instruction("DEC E", 0, 1, (n) => RegE = Dec(RegE)),
                new Instruction("LD E, {0:X2}", 1, 2, (n) => RegE = (byte)n),
                new Instruction("RRA", 0, 1, (n) => { RegA = RR(RegA); ZeroFlag = false; }),
                
                // 0x20
                new Instruction("JR NZ, {0:X2}", 1, 2, (n) => JumpRelative((byte)n, !ZeroFlag)),
                new Instruction("LD HL, {0:X4}", 2, 3, (n) => RegHL = (ushort)n),
                new Instruction("LDI (HL), A", 0, 2, (n) => md[RegHL++] = RegA),
                new Instruction("INC HL", 0, 2, (n) => RegHL++),
                new Instruction("INC H", 0, 1, (n) => RegH = Inc(RegH)),
                new Instruction("DEC H", 0, 1, (n) => RegH = Dec(RegH)),
                new Instruction("LD H, {0:X2}", 1, 2, (n) => RegH = (byte)n),
                new Instruction("DAA", 0, 1, (n) => DecimalAdjustA()),
                new Instruction("JR Z, {0:X2}", 1, 2, (n) => JumpRelative((byte)n, ZeroFlag)),
                new Instruction("ADD HL, HL", 0, 2, (n) => AddHL(RegHL)),
                new Instruction("LDI A, (HL)", 0, 2, (n) => RegA = md[RegHL++]),
                new Instruction("DEC HL", 0, 2, (n) => RegHL--),
                new Instruction("INC L", 0, 1, (n) => RegL = Inc(RegL)),
                new Instruction("DEC L", 0, 1, (n) => RegL = Dec(RegL)),
                new Instruction("LD L, {0:X2}", 1, 2, (n) => RegL = (byte)n),
                new Instruction("CPL", 0, 1, (n) => ComplementA()),
                
                // 0x30
                new Instruction("JR NC, {0:X2}", 1, 2, (n) => JumpRelative((byte)n, !CarryFlag)),
                new Instruction("LD SP, {0:X4}", 2, 3, (n) => SP = (ushort)n),
                new Instruction("LDD (HL), A", 0, 2, (n) => md[RegHL--] = RegA),
                new Instruction("INC SP", 0, 2, (n) => SP++),
                new Instruction("INC (HL)", 0, 3, (n) => md[RegHL] = Inc(md[RegHL])),
                new Instruction("DEC (HL)", 0, 3, (n) => md[RegHL] = Dec(md[RegHL])),
                new Instruction("LD (HL), {0:X2}", 1, 3, (n) => md[RegHL] = (byte)n),
                new Instruction("SCF", 0, 1, (n) => SetCarryFlag(true)),
                new Instruction("JR C, n", 1, 2, (n) => JumpRelative((byte)n, CarryFlag)),
                new Instruction("ADD HL, SP", 0, 2, (n) => AddHL(SP)),
                new Instruction("LDD A, (HL)", 0, 2, (n) => RegA = md[RegHL--]),
                new Instruction("DEC SP", 0, 2, (n) => SP--),
                new Instruction("INC A", 0, 1, (n) => RegA = Inc(RegA)),
                new Instruction("DEC A", 0, 1, (n) => RegA = Dec(RegA)),
                new Instruction("LD A, {0:X2}", 1, 2, (n) => RegA = (byte)n),
                new Instruction("CCF", 0, 1, (n) => SetCarryFlag(!CarryFlag)),
                
                // 0x40
                new Instruction("LD B, B"),
                new Instruction("LD B, C", 0, 1, (n) => RegB = RegC),
                new Instruction("LD B, D", 0, 1, (n) => RegB = RegD),
                new Instruction("LD B, E", 0, 1, (n) => RegB = RegE),
                new Instruction("LD B, H", 0, 1, (n) => RegB = RegH),
                new Instruction("LD B, L", 0, 1, (n) => RegB = RegL),
                new Instruction("LD B, (HL)", 0, 2, (n) => RegB = md[RegHL]),
                new Instruction("LD B, A", 0, 1, (n) => RegB = RegA),
                new Instruction("LD C, B", 0, 1, (n) => RegC = RegB),
                new Instruction("LD C, C"),
                new Instruction("LD C, D", 0, 1, (n) => RegC = RegD),
                new Instruction("LD C, E", 0, 1, (n) => RegC = RegE),
                new Instruction("LD C, H", 0, 1, (n) => RegC = RegH),
                new Instruction("LD C, L", 0, 1, (n) => RegC = RegL),
                new Instruction("LD C, (HL)", 0, 2, (n) => RegC = md[RegHL]),
                new Instruction("LD C, A", 0, 1, (n) => RegC = RegA),

                // 0x50
                new Instruction("LD D, B", 0, 1, (n) => RegD = RegB),
                new Instruction("LD D, C", 0, 1, (n) => RegD = RegC),
                new Instruction("LD D, D"),
                new Instruction("LD D, E", 0, 1, (n) => RegD = RegE),
                new Instruction("LD D, H", 0, 1, (n) => RegD = RegH),
                new Instruction("LD D, L", 0, 1, (n) => RegD = RegL),
                new Instruction("LD D, (HL)", 0, 2, (n) => RegD = md[RegHL]),
                new Instruction("LD D, A", 0, 1, (n) => RegD = RegA),
                new Instruction("LD E, B", 0, 1, (n) => RegE = RegB),
                new Instruction("LD E, C", 0, 1, (n) => RegE = RegC),
                new Instruction("LD E, D", 0, 1, (n) => RegE = RegD),
                new Instruction("LD E, E"),
                new Instruction("LD E, H", 0, 1, (n) => RegE = RegH),
                new Instruction("LD E, L", 0, 1, (n) => RegE = RegL),
                new Instruction("LD E, (HL)", 0, 2, (n) => RegE = md[RegHL]),
                new Instruction("LD E, A", 0, 1, (n) => RegE = RegA),

                // 0x60
                new Instruction("LD H, B", 0, 1, (n) => RegH = RegB),
                new Instruction("LD H, C", 0, 1, (n) => RegH = RegC),
                new Instruction("LD H, D", 0, 1, (n) => RegH = RegD),
                new Instruction("LD H, E", 0, 1, (n) => RegH = RegE),
                new Instruction("LD H, H"),
                new Instruction("LD H, L", 0, 1, (n) => RegH = RegL),
                new Instruction("LD H, (HL)", 0, 2, (n) => RegH = md[RegHL]),
                new Instruction("LD H, A", 0, 1, (n) => RegH = RegA),
                new Instruction("LD L, B", 0, 1, (n) => RegL = RegB),
                new Instruction("LD L, C", 0, 1, (n) => RegL = RegC),
                new Instruction("LD L, D", 0, 1, (n) => RegL = RegD),
                new Instruction("LD L, E", 0, 1, (n) => RegL = RegE),
                new Instruction("LD L, H", 0, 1, (n) => RegL = RegH),
                new Instruction("LD L, L"),
                new Instruction("LD L, (HL)", 0, 2, (n) => RegL = md[RegHL]),
                new Instruction("LD L, A", 0, 1, (n) => RegL = RegA),

                // 0x70
                new Instruction("LD (HL), B", 0, 2, (n) => md[RegHL] = RegB),
                new Instruction("LD (HL), C", 0, 2, (n) => md[RegHL] = RegC),
                new Instruction("LD (HL), D", 0, 2, (n) => md[RegHL] = RegD),
                new Instruction("LD (HL), E", 0, 2, (n) => md[RegHL] = RegE),
                new Instruction("LD (HL), H", 0, 2, (n) => md[RegHL] = RegH),
                new Instruction("LD (HL), L", 0, 2, (n) => md[RegHL] = RegL),
                new Instruction("HALT", 0, 2, (n) => Halt()),
                new Instruction("LD (HL), A", 0, 2, (n) => md[RegHL] = RegA),
                new Instruction("LD A, B", 0, 1, (n) => RegA = RegB),
                new Instruction("LD A, C", 0, 1, (n) => RegA = RegC),
                new Instruction("LD A, D", 0, 1, (n) => RegA = RegD),
                new Instruction("LD A, E", 0, 1, (n) => RegA = RegE),
                new Instruction("LD A, H", 0, 1, (n) => RegA = RegH),
                new Instruction("LD A, L", 0, 1, (n) => RegA = RegL),
                new Instruction("LD A, (HL)", 0, 2, (n) => RegA = md[RegHL]),
                new Instruction("LD A, A"),

                // 0x80
                new Instruction("ADD A, B", 0, 1, (n) => AddA(RegB)),
                new Instruction("ADD A, C", 0, 1, (n) => AddA(RegC)),
                new Instruction("ADD A, D", 0, 1, (n) => AddA(RegD)),
                new Instruction("ADD A, E", 0, 1, (n) => AddA(RegE)),
                new Instruction("ADD A, H", 0, 1, (n) => AddA(RegH)),
                new Instruction("ADD A, L", 0, 1, (n) => AddA(RegL)),
                new Instruction("ADD A, (HL)", 0, 2, (n) => AddA(md[RegHL])),
                new Instruction("ADD A, A", 0, 1, (n) => AddA(RegA)),
                new Instruction("ADC A, B", 0, 1, (n) => AddA(RegB, true)),
                new Instruction("ADC A, C", 0, 1, (n) => AddA(RegC, true)),
                new Instruction("ADC A, D", 0, 1, (n) => AddA(RegD, true)),
                new Instruction("ADC A, E", 0, 1, (n) => AddA(RegE, true)),
                new Instruction("ADC A, H", 0, 1, (n) => AddA(RegH, true)),
                new Instruction("ADC A, L", 0, 1, (n) => AddA(RegL, true)),
                new Instruction("ADC A, (HL)", 0, 2, (n) => AddA(md[RegHL], true)),
                new Instruction("ADC A, A", 0, 1, (n) => AddA(RegA, true)),
                
                // 0x90
                new Instruction("SUB B", 0, 1, (n) => SubA(RegB)),
                new Instruction("SUB C", 0, 1, (n) => SubA(RegC)),
                new Instruction("SUB D", 0, 1, (n) => SubA(RegD)),
                new Instruction("SUB E", 0, 1, (n) => SubA(RegE)),
                new Instruction("SUB H", 0, 1, (n) => SubA(RegH)),
                new Instruction("SUB L", 0, 1, (n) => SubA(RegL)),
                new Instruction("SUB (HL)", 0, 2, (n) => SubA(md[RegHL])),
                new Instruction("SUB A", 0, 1, (n) => SubA(RegA)),
                new Instruction("SBC A, B", 0, 1, (n) => SubA(RegB, true)),
                new Instruction("SBC A, C", 0, 1, (n) => SubA(RegC, true)),
                new Instruction("SBC A, D", 0, 1, (n) => SubA(RegD, true)),
                new Instruction("SBC A, E", 0, 1, (n) => SubA(RegE, true)),
                new Instruction("SBC A, H", 0, 1, (n) => SubA(RegH, true)),
                new Instruction("SBC A, L", 0, 1, (n) => SubA(RegL, true)),
                new Instruction("SBC A, (HL)", 0, 2, (n) => SubA(md[RegHL], true)),
                new Instruction("SBC A, A", 0, 1, (n) => SubA(RegA, true)),

                // 0xA0
                new Instruction("AND B", 0, 1, (n) => AndA(RegB)),
                new Instruction("AND C", 0, 1, (n) => AndA(RegC)),
                new Instruction("AND D", 0, 1, (n) => AndA(RegD)),
                new Instruction("AND E", 0, 1, (n) => AndA(RegE)),
                new Instruction("AND H", 0, 1, (n) => AndA(RegH)),
                new Instruction("AND L", 0, 1, (n) => AndA(RegL)),
                new Instruction("AND (HL)", 0, 2, (n) => AndA(md[RegHL])),
                new Instruction("AND A", 0, 1, (n) => AndA(RegA)),
                new Instruction("XOR B", 0, 1, (n) => XOrA(RegB)),
                new Instruction("XOR C", 0, 1, (n) => XOrA(RegC)),
                new Instruction("XOR D", 0, 1, (n) => XOrA(RegD)),
                new Instruction("XOR E", 0, 1, (n) => XOrA(RegE)),
                new Instruction("XOR H", 0, 1, (n) => XOrA(RegH)),
                new Instruction("XOR L", 0, 1, (n) => XOrA(RegL)),
                new Instruction("XOR (HL)", 0, 2, (n) => XOrA(md[RegHL])),
                new Instruction("XOR A", 0, 1, (n) => XOrA(RegA)),

                // 0xB0
                new Instruction("OR B", 0, 1, (n) => OrA(RegB)),
                new Instruction("OR C", 0, 1, (n) => OrA(RegC)),
                new Instruction("OR D", 0, 1, (n) => OrA(RegD)),
                new Instruction("OR E", 0, 1, (n) => OrA(RegE)),
                new Instruction("OR H", 0, 1, (n) => OrA(RegH)),
                new Instruction("OR L", 0, 1, (n) => OrA(RegL)),
                new Instruction("OR (HL)", 0, 2, (n) => OrA(md[RegHL])),
                new Instruction("OR A", 0, 1, (n) => OrA(RegA)),
                new Instruction("CP B", 0, 1, (n) => CompareA(RegB)),
                new Instruction("CP C", 0, 1, (n) => CompareA(RegC)),
                new Instruction("CP D", 0, 1, (n) => CompareA(RegD)),
                new Instruction("CP E", 0, 1, (n) => CompareA(RegE)),
                new Instruction("CP H", 0, 1, (n) => CompareA(RegH)),
                new Instruction("CP L", 0, 1, (n) => CompareA(RegL)),
                new Instruction("CP (HL)", 0, 2, (n) => CompareA(md[RegHL])),
                new Instruction("CP A", 0, 1, (n) => CompareA(RegA)),

                // 0xC0
                new Instruction("RET NZ", 0, 2, (n) => Return(!ZeroFlag)),
                new Instruction("POP BC", 0, 3, (n) => RegBC = PopWord()),
                new Instruction("JP NZ, {0:X4}", 2, 3, (n) => JumpTo(n, !ZeroFlag)),
                new Instruction("JP {0:X4}", 2, 4, (n) => JumpTo(n)),
                new Instruction("CALL NZ {0:X4}", 2, 3, (n) => Call(n, !ZeroFlag)),
                new Instruction("PUSH BC", 0, 4, (n) => PushWord(RegBC)),
                new Instruction("ADD A, {0:X2}", 1, 2, (n) => AddA((byte)n)),
                new Instruction("RST 0", 0, 4, (n) => Call(0)),
                new Instruction("RET Z", 0, 2, (n) => Return(ZeroFlag)),
                new Instruction("RET", 0, 2, (n) => Return()),
                new Instruction("JP Z, {0:X4}", 2, 3, (n) => JumpTo(n, ZeroFlag)),
                new Instruction("", 1, 2, (n) => ExtendedOperation(n)), // Extended Operation
                new Instruction("CALL Z, {0:X4}", 2, 3, (n) => Call(n, ZeroFlag)),
                new Instruction("CALL {0:X4}", 2, 3, (n) => Call(n)),
                new Instruction("ADC A, {0:X2}", 1, 2, (n) => AddA((byte)n, true)),
                new Instruction("RST 8", 0, 4, (n) => Call(8)),
                
                // 0xD0
                new Instruction("RET NC", 0, 2, (n) => Return(!CarryFlag)),
                new Instruction("POP DE", 0, 3, (n) => RegDE = PopWord()),
                new Instruction("JP NC, {0:X4}", 2, 3, (n) => JumpTo(n, !CarryFlag)),
                new Instruction("undefined"),
                new Instruction("CALL NC {0:X4}", 2, 3, (n) => Call(n, !CarryFlag)),
                new Instruction("PUSH DE", 0, 4, (n) => PushWord(RegDE)),
                new Instruction("SUB {0:X2}", 1, 2, (n) => SubA((byte)n)),
                new Instruction("RST 10", 0, 4, (n) => Call(0x10)),
                new Instruction("RET C", 0, 2, (n) => Return(CarryFlag)),
                new Instruction("RETI", 0, 2, (n) => { Return(); IMEFlag = true; } ),
                new Instruction("JP C, {0:X4}", 2, 3, (n) => JumpTo(n, CarryFlag)),
                new Instruction("undefined"),
                new Instruction("CALL C, {0:X4}", 2, 3, (n) => Call(n, CarryFlag)),
                new Instruction("undefined"),
                new Instruction("SBC A, {0:X2}", 1, 2, (n) => SubA((byte)n, true)),
                new Instruction("RST 18", 0, 4, (n) => Call(0x18)),
                
                // 0xE0
                new Instruction("LDH (FF00+{0:X2}), A", 1, 3, (n) => md[0xFF00 + n] = RegA),
                new Instruction("POP HL", 0, 3, (n) => RegHL = PopWord()),
                new Instruction("LD (FF00+C), A", 0, 2, (n) => md[0xFF00 + RegC] = RegA),
                new Instruction("undefined"),
                new Instruction("undefined"),
                new Instruction("PUSH HL", 0, 4, (n) => PushWord(RegHL)),
                new Instruction("AND {0:X2}", 1, 2, (n) => AndA((byte)n)),
                new Instruction("RST 20", 0, 4, (n) => Call(0x20)),
                new Instruction("ADD SP, {0:X2}", 1, 4, (n) => SP = AddSP((sbyte)n)),
                new Instruction("JP (HL)", 0, 1, (n) => JumpTo(RegHL)),
                new Instruction("LD {0:X4}, A", 2, 4, (n) => md[n] = RegA),
                new Instruction("undefined"),
                new Instruction("undefined"),
                new Instruction("undefined"),
                new Instruction("XOR {0:X2}", 1, 2, (n) => XOrA((byte)n)),
                new Instruction("RST 28", 0, 4, (n) => Call(0x28)),

                // 0xF0
                new Instruction("LDH A, (FF00+{0:X2})", 1, 3, (n) => RegA = md[0xFF00 + n]),
                new Instruction("POP AF", 0, 3, (n) => RegAF = PopWord()),
                new Instruction("LD A, (FF00+C)", 0, 2, (n) => RegA = md[0xFF00 + RegC]),
                new Instruction("DI", 0, 1, (n) => IMEFlag = false),
                new Instruction("undefined"),
                new Instruction("PUSH AF", 0, 4, (n) => PushWord(RegAF)),
                new Instruction("OR {0:X2}", 1, 2, (n) => OrA((byte)n)),
                new Instruction("RST 30", 0, 4, (n) => Call(0x30)),
                new Instruction("LDHL SP, {0:X2}", 1, 4, (n) => RegHL = AddSP((sbyte)n)),
                new Instruction("LD SP, HL", 0, 1, (n) => SP = RegHL),
                new Instruction("LD A, {0:X4}", 2, 4, (n) => RegA = md[n]),
                new Instruction("EI", 0, 1, (n) => IMEFlag = true),
                new Instruction("undefined"),
                new Instruction("undefined"),
                new Instruction("CP {0:X2}", 1, 2, (n) => CompareA((byte)n)),
                new Instruction("RST 38", 0, 4, (n) => Call(0x38)),
            };
        }
    }
}
