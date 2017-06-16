using System.Threading.Tasks;

namespace GBE.Emulation
{
    [System.Flags]
    internal enum InterruptFlags : byte
    {
        None = 0,
        VerticalBlank = 1,
        LCDCStatus = 2,
        TimerOverflow = 4,
        SerialTxComplete = 8,
        InputSignalLow = 16
    }

    /// <summary>
    /// Component used by the CPU to access memory.
    /// </summary>
    class MemoryDispatcher
    {
        /// <summary>
        /// Gameboy BIOS. 
        /// </summary>
        private static readonly byte[] BIOS =
        {
            0x31, 0xFE, 0xFF, 0xAF, 0x21, 0xFF, 0x9F, 0x32, 0xCB, 0x7C, 0x20, 0xFB, 0x21, 0x26, 0xFF, 0x0E,
            0x11, 0x3E, 0x80, 0x32, 0xE2, 0x0C, 0x3E, 0xF3, 0xE2, 0x32, 0x3E, 0x77, 0x77, 0x3E, 0xFC, 0xE0,
            0x47, 0x11, 0x04, 0x01, 0x21, 0x10, 0x80, 0x1A, 0xCD, 0x95, 0x00, 0xCD, 0x96, 0x00, 0x13, 0x7B,
            0xFE, 0x34, 0x20, 0xF3, 0x11, 0xD8, 0x00, 0x06, 0x08, 0x1A, 0x13, 0x22, 0x23, 0x05, 0x20, 0xF9,
            0x3E, 0x19, 0xEA, 0x10, 0x99, 0x21, 0x2F, 0x99, 0x0E, 0x0C, 0x3D, 0x28, 0x08, 0x32, 0x0D, 0x20,
            0xF9, 0x2E, 0x0F, 0x18, 0xF3, 0x67, 0x3E, 0x64, 0x57, 0xE0, 0x42, 0x3E, 0x91, 0xE0, 0x40, 0x04,
            0x1E, 0x02, 0x0E, 0x0C, 0xF0, 0x44, 0xFE, 0x90, 0x20, 0xFA, 0x0D, 0x20, 0xF7, 0x1D, 0x20, 0xF2,
            0x0E, 0x13, 0x24, 0x7C, 0x1E, 0x83, 0xFE, 0x62, 0x28, 0x06, 0x1E, 0xC1, 0xFE, 0x64, 0x20, 0x06,
            0x7B, 0xE2, 0x0C, 0x3E, 0x87, 0xF2, 0xF0, 0x42, 0x90, 0xE0, 0x42, 0x15, 0x20, 0xD2, 0x05, 0x20,
            0x4F, 0x16, 0x20, 0x18, 0xCB, 0x4F, 0x06, 0x04, 0xC5, 0xCB, 0x11, 0x17, 0xC1, 0xCB, 0x11, 0x17,
            0x05, 0x20, 0xF5, 0x22, 0x23, 0x22, 0x23, 0xC9, 0xCE, 0xED, 0x66, 0x66, 0xCC, 0x0D, 0x00, 0x0B,
            0x03, 0x73, 0x00, 0x83, 0x00, 0x0C, 0x00, 0x0D, 0x00, 0x08, 0x11, 0x1F, 0x88, 0x89, 0x00, 0x0E,
            0xDC, 0xCC, 0x6E, 0xE6, 0xDD, 0xDD, 0xD9, 0x99, 0xBB, 0xBB, 0x67, 0x63, 0x6E, 0x0E, 0xEC, 0xCC,
            0xDD, 0xDC, 0x99, 0x9F, 0xBB, 0xB9, 0x33, 0x3E, 0x3c, 0x42, 0xB9, 0xA5, 0xB9, 0xA5, 0x42, 0x4C,
            0x21, 0x04, 0x01, 0x11, 0xA8, 0x00, 0x1A, 0x13, 0xBE, 0x20, 0xFE, 0x23, 0x7D, 0xFE, 0x34, 0x20,
            0xF5, 0x06, 0x19, 0x78, 0x86, 0x23, 0x05, 0x20, 0xFB, 0x86, 0x20, 0xFE, 0x3E, 0x01, 0xE0, 0x50
        };

        public Cartridge Cartridge = null;

        internal bool BIOSLoaded = false;

        private byte[] InternalRAM = new byte[0x2000]; // Internal gameboy RAM

        private byte[] HRAM = new byte[0x7F]; // HRAM, a.k.a. Zero-Page RAM, also internal.

        // Addressable Registers
        internal byte SerialTxReg; // Serial communications register
        internal byte SerialControlReg;
        internal InterruptFlags IFlagsReg; // Interrupts register
        internal InterruptFlags IEnableReg;

        // System Components
        public TimerUnit Timer;
        public LCDController GraphicsUnit;
        public SoundUnit SoundUnit;
        public Joypad Joypad;

        // DMA Transfer
        private byte DMAStart; // DMA Register
        private int DMACurrentOffset;
        private int DMAClock;
        private bool DMAInProcess = false;


        public MemoryDispatcher(LCDController graphics, SoundUnit sound, TimerUnit timer, Joypad joypad)
        {
            Timer = timer;
            Timer.TimerOverflow += Timer_TimerOverflow;

            GraphicsUnit = graphics;
            GraphicsUnit.VBlankInterrupt += Gu_VBlankInterrupt;
            GraphicsUnit.LCDCInterrupt += Gu_LCDCInterrupt;

            SoundUnit = sound;

            Joypad = joypad;
            Joypad.JoypadInterrupt += Joypad_JoypadInterrupt;
        }

        public byte this[int address]
        {
            get
            {
                address &= 0xFFFF; // Mostly just a sanity check

                if (address >= 0xFF80 && address < 0xFFFF) // HRAM
                {
                    return HRAM[address & 0x7F];
                }

                // During DMA, only HRAM is accessable
                else if (DMAInProcess)
                {
                    return 0;
                }

                if (address >= 0xF000)
                {
                    if (address >= 0xFF00) // IO Registers
                    {
                        switch (address & 0xF0)
                        {
                            default: return 0;

                            case 0:
                                switch (address & 0x0F)
                                {
                                    case 0: return Joypad.P1Register;
                                    case 1: return SerialTxReg;
                                    case 2: return SerialControlReg;
                                    //   3: unused
                                    case 4: return Timer.DivisionRegister;
                                    case 5: return Timer.CountRegister;
                                    case 6: return Timer.ModuloRegister;
                                    case 7: return Timer.ControlRegister;
                                    //   8 - 14 unused
                                    case 15: return (byte)IFlagsReg;
                                }
                                return 0;

                            case 0x10: // Sound Registers
                            case 0x20:
                            case 0x30:
                                return 0; //su.ReadByte(address);

                            case 0x40: // LCD registers
                                switch (address & 0x0F)
                                {
                                    case 0: return GraphicsUnit.LCDCReg;
                                    case 1: return GraphicsUnit.STATReg;
                                    case 2: return GraphicsUnit.SCY;
                                    case 3: return GraphicsUnit.SCX;
                                    case 4: return GraphicsUnit.LCDYCoord;
                                    case 5: return GraphicsUnit.LYC;
                                    case 6: return DMAStart;
                                    case 7: return GraphicsUnit.BGPallet;
                                    case 8: return GraphicsUnit.ObjPallets[0];
                                    case 9: return GraphicsUnit.ObjPallets[1];
                                    case 10: return GraphicsUnit.WY;
                                    case 11: return GraphicsUnit.WX;
                                }
                                return 0;

                            case 0xF0: return (byte)IEnableReg;
                        }
                    }

                    // Addresses 0xFE00 - 0xFE9A is reserved for the graphics OAM.
                    // The rest is intentionally empty and should not be used.
                    else if (address >= 0xFEA0)
                    {
                        return 0;
                    }
                    else if (address >= 0xFE00)
                    {
                        return GraphicsUnit.ReadOAM(address);
                    }

                    // F000-FDFF echos D000-DDFF
                    return InternalRAM[(address & 0x1FFF)];
                }

                else if (address >= 0xC000) // C000 - EFFF: Internal RAM and Internal RAM Echo
                {
                    // C000 ~ DFFF is the actual internal RAM
                    // E000 ~ FDFF is an echo of the RAM, so we just read from the same place
                    return InternalRAM[(address & 0x1FFF)];
                }

                else if (address >= 0xA000) // A000 - BFFF: Switchable RAM bank, i.e. External/Cartridge RAM
                {
                    return Cartridge.ReadRAM(address);
                }

                else if (address >= 0x8000) //  Graphics VRAM
                {
                    return GraphicsUnit.ReadVRAM(address);
                }

                // Cartidge ROM
                else if ((address < 0x100) && BIOSLoaded) 
                {
                    return BIOS[address];
                }

                return Cartridge.ReadROM(address);
            }

            set
            {
                address &= 0xFFFF; // Sanity check again

                if (address >= 0xFF80 && address < 0xFFFF) // HRAM
                {
                    HRAM[address & 0x7F] = value;
                }

                // During DMA, only HRAM is accessable
                else if (DMAInProcess)
                {
                    return;
                }

                if (address >= 0xF000)
                {
                    if (address >= 0xFF00) // IO Registers
                    {
                        switch (address & 0xF0)
                        {
                            case 0:
                                switch (address & 0x0F)
                                {
                                    case 0: Joypad.P1Register = value; break;
                                    case 1: SerialTxReg = value; break;
                                    case 2: SerialControlReg = value; break;
                                    //   3: unused
                                    case 4: Timer.DivisionRegister = value; break;
                                    case 5: Timer.CountRegister = value; break;
                                    case 6: Timer.ModuloRegister = value; break;
                                    case 7: Timer.ControlRegister = value; break;
                                    //   8 - 14 unused
                                    case 15: IFlagsReg = (InterruptFlags)value; break;
                                }
                                return;

                            case 0x10: // Sound Registers
                            case 0x20:
                            case 0x30:
                                return; //su.WriteByte(address, value);

                            case 0x40: // LCD registers
                                switch (address & 0x0F)
                                {
                                    case 0: GraphicsUnit.LCDCReg = value; break;
                                    case 1: GraphicsUnit.STATReg = value; break;
                                    case 2: GraphicsUnit.SCY = value; break;
                                    case 3: GraphicsUnit.SCX = value; break;
                                    //   4: LY (read-only)
                                    case 5: GraphicsUnit.LYC = value; break;
                                    case 6:
                                        DMAStart = value;
                                        StartDMATransfer();
                                        break;
                                    case 7: GraphicsUnit.BGPallet = value; break;
                                    case 8: GraphicsUnit.ObjPallets[0] = value; break;
                                    case 9: GraphicsUnit.ObjPallets[1] = value; break;
                                    case 10: GraphicsUnit.WY = value; break;
                                    case 11: GraphicsUnit.WX = value; break;
                                }
                                return;

                            case 0x50:
                                if (BIOSLoaded)
                                {
                                   BIOSLoaded = value == 0;
                                }
                                break;

                            case 0xF0:
                                if (address == 0xFFFF)
                                {
                                    IEnableReg = (InterruptFlags)value;
                                }
                                break;
                        }
                    }

                    // Addresses 0xFE00 - 0xFE9A is reserved for the graphics OAM.
                    // The rest is intentionally empty and should not be used.
                    else if (address >= 0xFE00 && address < 0xFEA0)
                    {
                        GraphicsUnit.WriteOAM(address, value);
                    }

                    // F000-FDFF echos D000-DDFF and is read-only
                }
                
                else if (address >= 0xC000 && address < 0xE000) // C000-DFFF - Internal RAM
                {
                    switch (address)
                    {
                        case 0xc004:
                            break;
                    }

                    InternalRAM[address & 0x1FFF] = value;
                }

                else if (address >= 0xA000 && address < 0xC000) // A000-BFFF : External/Cartridge RAM
                {
                    Cartridge.WriteRAM(address, value);
                }

                else if (address >= 0x8000 && address < 0xA000) // Graphics VRAM
                {
                    GraphicsUnit.WriteVRAM(address, value);
                }

                else if (address < 0x8000)
                {
                    Cartridge.WriteToRegister(address, value);
                }
            }
        }

        public void DMAClockTick(int ticks)
        {
            if (DMAInProcess)
            {
                DMAClock += ticks;

                // The DMA runs for 160 milliseconds, which we assume to be around 80 clock cycles
                if (DMAClock > 80)
                {
                    DMAInProcess = false;
                }

                while (DMACurrentOffset < (DMAClock * 2) && DMACurrentOffset < 160)
                {
                    DMAInProcess = false;
                    GraphicsUnit.WriteOAM(DMACurrentOffset, this[(DMAStart << 8) + DMACurrentOffset]);
                    DMAInProcess = true;
                    DMACurrentOffset++;
                }
            }
        }

        private void StartDMATransfer()
        {
            DMAClock = 0;
            DMACurrentOffset = 0;
            DMAInProcess = true;
        }


        #region Interrupt Events

        private void Joypad_JoypadInterrupt()
        {
            IFlagsReg |= InterruptFlags.InputSignalLow;
        }

        private void Timer_TimerOverflow()
        {
            IFlagsReg |= InterruptFlags.TimerOverflow;
        }

        private void Gu_LCDCInterrupt()
        {
            IFlagsReg |= InterruptFlags.LCDCStatus;
        }

        private void Gu_VBlankInterrupt()
        {
            IFlagsReg |= InterruptFlags.VerticalBlank;
        }

        #endregion
    }
}
