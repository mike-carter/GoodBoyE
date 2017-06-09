using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBE.Emulation
{
    partial class Z80Processor
    {
        MemoryDispatcher md; // reference to memory unit

        Instruction[] instructionSet; // instructions decode table

        // 16-bit registers
        ushort PC; // program counter
        ushort SP; // stack pointer

        // 8-Bit Registers
        byte RegA; // Accumulator
        int RegFlags; // Flags register
        byte RegB;
        byte RegC;
        byte RegD;
        byte RegE;
        byte RegH;
        byte RegL;

        bool IMEFlag; // Master interrupt flag (IME)
        bool halted;

        public Z80Processor(MemoryDispatcher memory)
        {
            md = memory;
            instructionSet = InitInstructionSet();
        }

        #region Properties

        #region 16-Bit Register Access

        public ushort RegAF
        {
            get { return (ushort)((RegA << 8) + RegFlags); }
            private set
            {
                RegA = (byte)(value >> 8);
                RegFlags = (byte)value;
            }
        }

        public ushort RegBC
        {
            get { return (ushort)((RegB << 8) + RegC); }
            private set
            {
                RegB = (byte)(value >> 8);
                RegC = (byte)value;
            }
        }

        public ushort RegDE
        {
            get { return (ushort)((RegD << 8) + RegE); }
            private set
            {
                RegD = (byte)(value >> 8);
                RegE = (byte)value;
            }
        }

        public ushort RegHL
        {
            get { return (ushort)((RegH << 8) + RegL); }
            private set
            {
                RegH = (byte)(value >> 8);
                RegL = (byte)value;
            }
        }

        public ushort RegPC
        {
            get { return PC; }
        }

        public ushort RegSP
        {
            get { return SP; }
        }

        #endregion 16-Bit Register Access

        #region Flag Accessors

        private const int ZERO_FLAG = 0x80;
        private const int OP_FLAG = 0x40;
        private const int HALF_CARRY_FLAG = 0x20;
        private const int CARRY_FLAG = 0x10;

        /**
         * Gets or sets the Zero flag, which indicates that the result of the last operation was zero.
         */
        protected bool ZeroFlag
        {
            get { return (RegFlags & ZERO_FLAG) != 0; }
            set { RegFlags = value ? (RegFlags | ZERO_FLAG) : (RegFlags & ~ZERO_FLAG); }
        }

        /**
         * Gets or sets the Operation flag, which indicates that the last operation was a 
         * subtraction operation.
         */
        protected bool OpFlag
        {
            get { return (RegFlags & OP_FLAG) != 0; }
            set { RegFlags = value ? (RegFlags | OP_FLAG) : (RegFlags & ~OP_FLAG); }
        }

        /**
         * Gets or sets the Half Carry flag, which indicates an overflow of the lower nibble of the register
         */
        protected bool HalfCarryFlag
        {
            get { return (RegFlags & HALF_CARRY_FLAG) != 0; }
            set { RegFlags = value ? (RegFlags | HALF_CARRY_FLAG) : (RegFlags & ~HALF_CARRY_FLAG); }
        }

        /**
         * Gets or sets the Carry flag, which indicates that the last operation resulted in an overflow.
         */
        protected bool CarryFlag
        {
            get { return (RegFlags & CARRY_FLAG) != 0; }
            set { RegFlags = value ? (RegFlags | CARRY_FLAG) : (RegFlags & ~CARRY_FLAG); }
        }

        #endregion Flag Accessors

        #endregion Properties

        public void Reset()
        {
            PC = 0;
            SP = 0;
            RegAF = 0;
            RegBC = 0;
            RegDE = 0;
            RegHL = 0;

            IMEFlag = false;
            halted = false;
        }
        
        public int Step()
        {
            int ticks;

            if (halted)
            {
                ticks = 1;
            }
            else
            {
#if DEBUG
                switch (PC)
                {
                    case 0x2C4:
                        break;
                    case 0x2C7:
                        break;
                    case 0x2CA:
                        break;
                    case 0x2f0:
                        break;
                }
#endif

                int opcode = md[PC++];

                // Load immediate operand if necessary
                int immediate = 0;
                if (instructionSet[opcode].OperandLength > 0)
                {
                    if (instructionSet[opcode].OperandLength == 1)
                    {
                        immediate = md[PC++];
                    }
                    else
                    {
                        immediate = LoadWord(PC);
                        PC += 2;
                    }
                }
                // Execute instruction
                instructionSet[opcode].Operation?.Invoke(immediate);
                ticks = instructionSet[opcode].Ticks;
            }

            // Handle interrupts
            if ((md.IFlagsReg & md.IEnableReg) != 0)
            {
                // Un-halt the processor if any (enabled) interrupts are signalled,
                // even if the IME flag is reset.
                halted = false;

                if (IMEFlag)
                {
                    IMEFlag = false;
                    if ((md.IFlagsReg & InterruptFlags.VerticalBlank) != 0)
                    {
                        md.IFlagsReg &= ~InterruptFlags.VerticalBlank;
                        Call(0x40);
                    }
                    else if ((md.IFlagsReg & InterruptFlags.LCDCStatus) != 0)
                    {
                        md.IFlagsReg &= ~InterruptFlags.LCDCStatus;
                        Call(0x48);
                    }
                    else if ((md.IFlagsReg & InterruptFlags.TimerOverflow) != 0)
                    {
                        md.IFlagsReg &= ~InterruptFlags.TimerOverflow;
                        Call(0x50);
                    }
                    else if ((md.IFlagsReg & InterruptFlags.SerialTxComplete) != 0)
                    {
                        md.IFlagsReg &= ~InterruptFlags.SerialTxComplete;
                        Call(0x58);
                    }
                    else if ((md.IFlagsReg & InterruptFlags.InputSignalLow) != 0)
                    {
                        md.IFlagsReg &= ~InterruptFlags.InputSignalLow;
                        Call(0x60);
                    }
                    ticks += 4;
                }
            }

            return ticks;
        }

        #region Operation Methods

        private void Stop()
        {
            halted = true;
        }

        private void Halt()
        {
            halted = true;
        }

        private void SetCarryFlag(bool value)
        {
            OpFlag = false;
            HalfCarryFlag = false;
            CarryFlag = value;
        }

        private ushort LoadWord(int address)
        {
            return (ushort)(md[address] | (md[address + 1] << 8));
        }

        private void StoreWord(int address, ushort value)
        {
            md[address] = (byte)value;
            md[address + 1] = (byte)(value >> 8);
        }

        #region Arithmetic Operations

        void AddA(byte value, bool withCarry = false)
        {
            int result = RegA + value;

            if (withCarry) result += CarryFlag ? 1 : 0;

            OpFlag = false;
            CarryFlag = result > 0xFF;
            HalfCarryFlag = ((RegA ^ value ^ result) & 0x10) != 0;
            ZeroFlag = (result &= 0xFF) == 0; // also clears upper bits

            RegA = (byte)result;
        }

        void SubA(byte value, bool withCarry = false)
        {
            int result = RegA - value;

            if (withCarry) result -= CarryFlag ? 1 : 0;

            OpFlag = true;
            CarryFlag = result < 0;
            HalfCarryFlag = (RegA & 15) - (value & 15) < 0;
            ZeroFlag = (result &= 0xFF) == 0;

            RegA = (byte)result;
        }

        void AndA(byte value)
        {
            RegA = (byte)(RegA & value);

            ZeroFlag = RegA == 0;
            OpFlag = false;
            HalfCarryFlag = true;
            CarryFlag = false;
        }

        void OrA(byte value)
        {
            RegA = (byte)(RegA | value);

            ZeroFlag = RegA == 0;
            OpFlag = false;
            HalfCarryFlag = false;
            CarryFlag = false;
        }

        void XOrA(byte value)
        {
            RegA = (byte)(RegA ^ value);

            ZeroFlag = RegA == 0;
            OpFlag = false;
            HalfCarryFlag = false;
            CarryFlag = false;
        }

        void CompareA(byte value)
        {
            int result = RegA - value;

            OpFlag = true;
            CarryFlag = result < 0;
            HalfCarryFlag = (RegA & 15) - (value & 15) < 0;
            ZeroFlag = (result &= 0xFF) == 0;
        }

        private void ComplementA()
        {
            OpFlag = true;
            HalfCarryFlag = true;
            RegA = (byte)(~RegA);
        }

        byte Inc(byte value)
        {
            OpFlag = false;
            HalfCarryFlag = (value & 15) == 15;
            value++;
            ZeroFlag = value == 0;
            return value;
        }

        byte Dec(byte value)
        {
            OpFlag = true;
            HalfCarryFlag = (value & 15) == 0;
            value--;
            ZeroFlag = value == 0;
            return value;
        }

        void AddHL(ushort value)
        {
            int result = RegHL + value;

            OpFlag = false;
            HalfCarryFlag = (value & 0xFF) + (RegHL & 0xFF) > 0xFF;
            CarryFlag = result > 0xFFFF;

            RegHL = (ushort)(result & 0xFFFF);
        }

        private ushort AddSP(sbyte value)
        {
            RegFlags = 0;
            HalfCarryFlag = value >= 0 ? ((SP & 0xFF) + value) > 0xFF : ((SP & 0xFF) + value) < 0;
            CarryFlag = value >= 0 ? SP + value > 0xFFFF : SP + value < 0;
            return (ushort)((SP + value) & 0xFFFF);
        }

        byte SwapNibbles(byte n)
        {
            RegFlags = 0;

            n = (byte)((n << 4) + (n >> 4));
            ZeroFlag = n == 0;
            return n;
        }

        byte RLC(byte n)
        {
            RegFlags = 0;

            if ((n & 0x80) == 0)
            {
                n <<= 1;
                n |= 1;
                CarryFlag = true;
            }
            else
            {
                n <<= 1;
            }
            ZeroFlag = n == 0;
            return n;
        }

        byte RL(byte n)
        {
            OpFlag = false;
            HalfCarryFlag = false;
            
            bool carry = (n & 0x80) != 0;
            n <<= 1;
            n |= (byte)(CarryFlag ? 1 : 0);

            CarryFlag = carry;
            ZeroFlag = n == 0;
            return n;
        }

        byte RRC(byte n)
        {
            RegFlags = 0;

            if ((n & 1) == 0)
            {
                n >>= 1;
                n |= 0x80;
                CarryFlag = true;
            }
            else
            {
                n >>= 1;
            }
            ZeroFlag = n == 0;
            return n;
        }

        byte RR(byte n)
        {
            OpFlag = false;
            HalfCarryFlag = false;

            bool carry = (n & 1) != 0;
            n >>= 1;
            n |= (byte)(CarryFlag ? 0x80 : 0);

            CarryFlag = carry;
            ZeroFlag = n == 0;
            return n;
        }

        byte ShiftLeftArithmetic(byte n)
        {
            RegFlags = 0;

            CarryFlag = (n & 0x80) != 0;
            n <<= 1;
            ZeroFlag = n == 0;
            return n;
        }

        byte ShiftRightArithmetic(byte n)
        {
            RegFlags = 0;

            bool msb = (n & 0x80) != 0;
            CarryFlag = (n & 1) != 0;

            n = (byte)((n >> 1) + (msb ? 0x80 : 0));

            ZeroFlag = n == 0;
            return n;
        }

        byte ShiftRightLogical(byte n)
        {
            RegFlags = 0;

            CarryFlag = (n & 1) != 0;
            n >>= 1;
            ZeroFlag = n == 0;
            return n;
        }

        byte TestBit(int bit, byte val)
        {
            OpFlag = false;
            HalfCarryFlag = true;

            ZeroFlag = ((val >> bit) & 1) == 0;

            return val;
        }

        byte SetBit(int bit, byte val)
        {
            return (byte)(val | (1 << bit));
        }

        private void DecimalAdjustA()
        {
            byte cf = 0;
            if (RegA > 0x99 || CarryFlag)
            {
                CarryFlag = true;
                cf = 0x60;
            }
            else
            {
                CarryFlag = false;
            }

            if ((RegA & 0x0F) > 9 || HalfCarryFlag)
            {
                cf |= 6;
            }

            cf = (byte)(OpFlag ? RegA - cf : RegA + cf);

            HalfCarryFlag = ((RegA ^ cf) & 0x08) != 0;
            RegA = cf;

            ZeroFlag = RegA == 0;
        }

        #endregion Arithmetic Operations

        #region Stack Operations

        void PushWord(ushort word)
        {
            md[SP - 1] = (byte)(word >> 8);
            md[SP - 2] = (byte)word;
            SP -= 2;
        }

        ushort PopWord()
        {
            ushort value = md[SP];
            value = (ushort)(value | (md[SP + 1] << 8));
            SP += 2;
            return value;
        }

        #endregion

        #region Program Flow Operations

        void JumpRelative(byte value, bool condition = true)
        {
            if (condition)
            {
                PC = (ushort)(PC + ((sbyte)value));
            }
        }

        void JumpTo(int address, bool condition = true)
        {
            if (condition)
            {
                PC = (ushort)address;
            }
        }

        void Call(int address, bool condition = true)
        {
            if (condition)
            {
                PushWord(PC);
                PC = (ushort)address;
            }
        }

        void Return(bool condition = true)
        {
            if (condition)
            {
                PC = PopWord();
            }
        }

        #endregion Program Flow Operations

        private void ExtendedOperation(int extOpcode)
        {
            //string instruction, register = string.Empty;
            Func<byte, byte> operation;
            int bit = 0;

            switch (extOpcode & 0xF0)
            {
                case 0:
                default:
                    if ((extOpcode & 15) < 8)
                    {
                        //instruction = "RLC {0}";
                        operation = RLC;
                    }
                    else
                    {
                        //instruction = "RRC {0}";
                        operation = RRC;
                    }
                    break;
                case 0x10:
                    if ((extOpcode & 15) < 8)
                    {
                        //instruction = "RL {0}";
                        operation = RL;
                    }
                    else
                    {
                        //instruction = "RR {0}";
                        operation = RR;
                    }
                    break;
                case 0x20:
                    if ((extOpcode & 15) < 8)
                    {
                        //instruction = "SLA {0}";
                        operation = ShiftLeftArithmetic;
                    }
                    else
                    {
                        //instruction = "SRA {0}";
                        operation = ShiftRightArithmetic;
                    }
                    break;
                case 0x30:
                    if ((extOpcode & 15) < 8)
                    {
                        //instruction = "SWAP {0}";
                        operation = SwapNibbles;
                    }
                    else
                    {
                        //instruction = "SRL {0}";
                        operation = ShiftRightLogical;
                    }
                    break;
                case 0x40:
                case 0x50:
                case 0x60:
                case 0x70:
                    //instruction = "BIT {1}, {0}";
                    bit = (extOpcode - 0x40) / 8;
                    operation = (r) => TestBit(bit, r);
                    break;
                case 0x80:
                case 0x90:
                case 0xA0:
                case 0xB0:
                    //instruction = "RES {1}, {0}";
                    bit = (extOpcode - 0x80) / 8;
                    operation = (r) => (byte)(r & (~(1 << bit)));
                    break;
                case 0xC0:
                case 0xD0:
                case 0xE0:
                case 0xF0:
                    //instruction = "SET {1}, {0}";
                    bit = (extOpcode - 0xC0) / 8;
                    operation = (r) => (byte)(r | (1 << bit));
                    break;
            }


            instructionSet[0xCB].Ticks = 2; // most of these take 2 ticks
            switch (extOpcode & 0x0F)
            {
                case 0:
                case 8:
                    RegB = operation(RegB); /* register = "B"; */ break;
                case 1:
                case 9:
                    RegC = operation(RegC); /* register = "C"; */ break;
                case 2:
                case 10:
                    RegD = operation(RegD); /* register = "D"; */ break;
                case 3:
                case 11:
                    RegE = operation(RegE); /* register = "E"; */ break;
                case 4:
                case 12:
                    RegH = operation(RegH); /* register = "H"; */ break;
                case 5:
                case 13:
                    RegL = operation(RegL); /* register = "L"; */ break;
                case 6:
                case 14:
                    md[RegHL] = operation(md[RegHL]);
                    //register = "(HL)";
                    instructionSet[0xCB].Ticks = 4; // This instruction takes 4 ticks
                    break;
                case 7:
                case 15:
                    RegA = operation(RegA); /* register = "A"; */ break;
            }

            //if (DebugMode)
            //{
            //    instructionSet[0xCB].Disassembly = string.Format(instruction, register, bit);
            //}
        }

        #endregion Operation Methods
    }
}
