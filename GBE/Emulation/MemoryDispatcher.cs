namespace GBE.Emulation
{
    class MemoryDispatcher
    {
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

        internal bool BIOSLoaded;

        private byte[] InternalRAM; // Internal gameboy RAM 

        private byte[] ZeroRAM; // Zero-Page RAM, also internal.

        // Addressable Registers
        internal byte PortsReg; // Ports P10 ~ P15 register (FF00)
        internal byte SerialTxReg; // Serial communications register
        internal byte SerialControlReg;
        internal InterruptFlags IFlagsReg; // Interrupts register
        internal InterruptFlags IEnableReg;

        public Timer Timer;

        public GraphicsUnit GraphicsUnit;

        public SoundUnit SoundUnit;

        public MemoryDispatcher(GraphicsUnit graphics, SoundUnit sound, Timer timer)
        {
            Timer = timer;
            Timer.TimerOverflow += Timer_TimerOverflow;

            GraphicsUnit = graphics;
            GraphicsUnit.VBlankInterrupt += Gu_VBlankInterrupt;
            GraphicsUnit.LCDCInterrupt += Gu_LCDCInterrupt;

            SoundUnit = sound;
        }
        
        public void Reset()
        {
            InternalRAM = new byte[8192];
            ZeroRAM = new byte[127];

            Timer.Reset();

            if (Cartridge != null)
            {
                Cartridge.Reset();
            }

            BIOSLoaded = true;
        }

        public byte this[int address]
        {
            get
            {
                address &= 0xFFFF;
                if (address >= 0xF000)
                {
                    if (address >= 0xFF00) // FF00 ~ FFFF : IO and Hardware registers; Zero-Page RAM
                    {
                        switch (address & 0xF0)
                        {
                            case 0:
                                switch (address & 0x0F)
                                {
                                    case 0xFF00: return PortsReg;
                                    case 0xFF01: return SerialTxReg;
                                    case 0xFF02: return SerialControlReg;
                                    case 0xFF04: return Timer.DivisionRegister;
                                    case 0xFF05: return Timer.CountRegister;
                                    case 0xFF06: return Timer.ModuloRegister;
                                    case 0xFF07: return Timer.ControlRegister;
                                    case 0xFF0F: return (byte)IFlagsReg;
                                    default: return 0;
                                }

                            case 0x10:
                            case 0x20:
                            case 0x30:
                                return 0; //su.ReadByte(address);

                            case 0x40:
                                return GraphicsUnit[address];

                            case 0xF0:
                                if (address == 0xFFFF)
                                    return (byte)IEnableReg;
                                else
                                    return ZeroRAM[address & 0x7F];

                            case 0x80:
                            case 0x90:
                            case 0xA0:
                            case 0xB0:
                            case 0xC0:
                            case 0xD0:
                            case 0xE0:
                                return ZeroRAM[address & 0x7F];

                            default: return 0;
                        }
                    }
                    // The only addresses that are interesting in the FExx space are the Sprite Attribute Registers (OAM)
                    // in the GPU. The rest is intentionally empty and should not be used.
                    else if (address >= 0xFE00)
                    {
                        return address < 0xFEA0 ? GraphicsUnit[address] : (byte)0;
                    }

                    // F000-FDFF echos D000-DDFF
                    return InternalRAM[(address & 0x1FFF)];
                }
                else if (address >= 0xC000) // C000-EFFF - Internal RAM and Internal RAM Echo
                {
                    // C000 ~ DFFF is the actual internal RAM
                    // E000 ~ FDFF is an echo of the RAM, so we just read from the same place
                    return InternalRAM[(address & 0x1FFF)];
                }
                else if (address >= 0xA000) // A000 ~ BFFF : Switchable RAM bank, i.e. External/Cartridge RAM
                {
                    return Cartridge[address];
                }
                else if (address >= 0x8000) // Graphics VRAM
                {
                    return GraphicsUnit[address];
                }
                else if ((address < 0x100) && BIOSLoaded)
                    return BIOS[address];

                return Cartridge[address];
            }

            set
            {
                address &= 0xFFFF;
                if (address >= 0xF000)
                {
                    if (address >= 0xFF00) // FF00 ~ FFFF : IO and Hardware registers; Zero-Page RAM
                    {
                        switch (address & 0xF0)
                        {
                            case 0:
                                switch (address & 0x0F)
                                {
                                    case 0xFF00: PortsReg = value; return;
                                    case 0xFF01: SerialTxReg = value; return;
                                    case 0xFF02: SerialControlReg = value; return;
                                    case 0xFF04: Timer.DivisionRegister = value; return;
                                    case 0xFF05: Timer.CountRegister = value; return;
                                    case 0xFF06: Timer.ModuloRegister = value; return;
                                    case 0xFF07: Timer.ControlRegister = value; return;
                                    case 0xFF0F: IFlagsReg = (InterruptFlags)value; return;
                                }
                                break;

                            case 0x10:
                            case 0x20:
                            case 0x30:
                                //su.WriteByte(address, value);
                                break;

                            case 0x40:
                                GraphicsUnit[address] = value;
                                break;
                                
                            case 0x50:
                                BIOSLoaded = value == 0;
                                break;

                            case 0xF0:
                                if (address == 0xFFFF)
                                    IEnableReg = (InterruptFlags)value;
                                else
                                    ZeroRAM[address & 0x7F] = value;
                                break;

                            case 0x80:
                            case 0x90:
                            case 0xA0:
                            case 0xB0:
                            case 0xC0:
                            case 0xD0:
                            case 0xE0:
                                if (address >= 0xFF80)
                                {
                                    ZeroRAM[address & 0x7F] = value;
                                }
                                break;
                        }
                    }
                    // The only addresses that are interesting in the FExx space are the Sprite Attribute Registers (OAM)
                    // in the GPU. The rest is intentionally empty and should not be used.
                    else if (address >= 0xFE00 && address < 0xFEA0)
                    {
                        GraphicsUnit[address] = value;
                    }
                }
                else if (address >= 0xC000) // C000-EFFF - Internal RAM and Internal RAM Echo
                {
                    // C000 ~ DFFF is the actual internal RAM
                    // E000 ~ FDFF is an echo of the RAM, so we just read from the same place
                    InternalRAM[(address & 0x1FFF)] = value;
                }
                else if (address >= 0xA000) // A000-BFFF : External/Cartridge RAM
                {
                    Cartridge[address] = value;
                }
                else if (address >= 0x8000) // Graphics VRAM
                {
                    GraphicsUnit[address] = value;
                }
                else
                {
                    Cartridge[address] = value;
                }
            }
        }


        #region Interrupt Events

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
