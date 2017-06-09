using System;

namespace GBE.Emulation.Cartridges
{
    // TODO: Finish timer functionality

    class CartMBC3 : Cartridge
    {
        private byte[][] rom;
        private byte[] rom0;
        private byte[] currentROMBank;

        private byte[,] ram;
        private int ramBankNumber;
        private bool ramEnabled;

        private DateTime latchedTime;
        private bool latchSequenceStarted = false;

        public CartMBC3(System.IO.Stream cartDataStream)
        {
            rom = new byte[128][];

            for (int i = 0; i < 128; i++)
            {
                byte[] nextBank = new byte[0x4000];

                if (cartDataStream.Read(nextBank, 1, 0x3FFF) > 0)
                {
                    rom[i] = nextBank;
                }
                else
                {
                    break;
                }
            }
            rom0 = rom[0];
        }

        public override void Reset()
        {
            currentROMBank = rom[1];

            ram = new byte[4, 0x2000];
            ramBankNumber = 0;
            ramEnabled = false;

            latchedTime = DateTime.Now;
            latchSequenceStarted = false;

            LatchTime();
        }

        internal override byte this[int address]
        {
            get
            {
                if (address < 0x4000)
                {
                    return rom0[address];
                }
                else if (address < 0x8000)
                {
                    return currentROMBank[address & 0x3FFF];
                }
                else if (ramEnabled && address >= 0xA000 && address < 0xC000)
                {
                    switch (ramBankNumber)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                            return ram[ramBankNumber, address & 0x1FFF];

                            //        case 8:
                            //            return secondsReg;
                            //        case 9:
                            //            return minutesReg;
                            //        case 10:
                            //            return hoursReg;
                            //        case 11:
                            //            return (byte)(daysReg & )
                    }
                }
                return 0;
            }

            set
            {
                switch (address & 0xF000)
                {
                    default: break;

                    case 0:
                    case 0x1000: // Register 0: Enables RAM
                        // Writing XXXX1010 (X == don't cares) to this address enables RAM access (Read and Write). 
                        // Writing any other value disables RAM.
                        ramEnabled = (value & 0x0F) == 0xA;
                        break;

                    case 0x2000:
                    case 0x3000: // Register 1: Selects the ROM bank to access.
                        // Writing 0 is the same as writing 1.
                        value &= 0x1F;
                        currentROMBank = rom[value == 0 ? 1 : value];
                        break;

                    case 0x4000:
                    case 0x5000: // Register 2: RAM Select or RTC Resgister Select
                        ramBankNumber = value & 0x0F;
                        break;

                    case 0x6000:
                    case 0x7000: // Register 3: Latch Clock Data
                        // Clock time is latched when 0x00 is written, then 0x01 is written to this register.
                        if (latchSequenceStarted)
                        {
                            if (value == 1)
                            {
                                LatchTime();
                            }
                            latchSequenceStarted = false;
                        }
                        else if (value == 0)
                        {
                            latchSequenceStarted = true;
                        }
                        break;

                    case 0xA000:
                    case 0xB000: // RAM and clock register access
                        if (ramEnabled)
                        {
                            switch (ramBankNumber)
                            {
                                case 0:
                                case 1:
                                case 2:
                                case 3:
                                    ram[ramBankNumber, address & 0x1FFF] = value;
                                    break;

                                // Clock Registers
                                case 8:
                                case 9:
                                case 10:
                                case 11:
                                case 12:
                                    break;
                            }
                        }
                        break;
                }
            }
        }

        private void LatchTime()
        {
            //// If the HALT bit is set, then don't advance the time
            //if ()

            //DateTime now = DateTime.Now;

            //secondsReg = (byte)now.Second;
            //minutesReg = (byte)now.Minute;

            //daysReg = (ushort)(daysReg | (byte)now.DayOfYear);
        }
    }
}
