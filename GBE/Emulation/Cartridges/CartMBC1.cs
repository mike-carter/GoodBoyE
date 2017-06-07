using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBE.Emulation.Cartridges
{
    class CartMBC1 : Cartridge
    {
        private byte[][] rom;
        private byte[] rom0;
        private byte[] currentROMBank;

        private byte[,] ram;

        private int romBankNumber;
        private int ramBankNumber;
        private bool ramEnabled;
        private bool inRAMBankMode;

        public CartMBC1(System.IO.Stream cartDataStream)
        {
            rom = new byte[128][];

            // Read bank 0 first, since it is addressed differently.
            rom0 = new byte[0x4000];
            cartDataStream.Read(rom0, 0, 0x4000);
            rom[0] = rom0;

            for (int i = 1; i < 128; i++)
            {
                // RAM blocks 0x20, 0x40, and 0x60 are not used, since 0x00 is changed to 0x01
                // when written to the ROM bank register. We just skip those banks.
                if ((i & 0x1f) == 0)
                    i++;

                // Check to see if we are at the end of the stream so that we don't
                // unecessarily create a huge 32 KBit buffer
                int test = cartDataStream.ReadByte();
                if (test == -1)
                    break;

                rom[i] = new byte[0x4000];
                rom[i][0] = (byte)test;

                if (cartDataStream.Read(rom[i], 1, 0x3FFF) != 0x3FFF)
                    break;
            }
        }

        public override void Reset()
        {
            ram = new byte[4, 0x2000];

            currentROMBank = rom[1];

            romBankNumber = 1;
            ramBankNumber = 0;
            ramEnabled = false;
            inRAMBankMode = false;
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
                    return ram[ramBankNumber, address & 0x1FFF];
                }
                return 0;
            }

            set
            {
                switch (address & 0xF000)
                {
                    default: break;

                    case 0:
                    case 0x1000: // Register 0: Sets the RAM access gate
                        // Writing XXXX1010 (X == don't cares) to this address enables RAM access (Read and Write). 
                        // Writing any other value disables RAM.
                        ramEnabled = (value & 0x0F) == 0xA;
                        break;

                    case 0x2000:
                    case 0x3000: // Register 1: Selects the ROM bank to access.
                        // Writing 0 is the same as writing 1.
                        // This register only sets the lower 5 bits of the bank number
                        value &= 0x1F;
                        romBankNumber &= 0xE0;
                        romBankNumber |= value == 0 ? 1 : value;
                        currentROMBank = rom[romBankNumber];
                        break;

                    case 0x4000:
                    case 0x5000: // Register 2: Selects the RAM bank, or upper bits of the ROM bank
                        // When in 8-Mbit ROM mode (inRAMBankMode == false), this sets the upper 2 bits of the ROM bank number
                        // When in 4-Mbit ROM mode (inRAMBankMode == true), this sets the RAM bank number.
                        if (inRAMBankMode)
                            ramBankNumber = value & 3;
                        else
                            romBankNumber = (romBankNumber & 0x1F) | ((value & 3) << 5);
                        currentROMBank = rom[romBankNumber];
                        break;

                    case 0x6000:
                    case 0x7000: // Register 3: Change ROM/RAM mode.
                        // Only the least significant bit is considered.
                        if ((value & 1) == 1)
                        {
                            if (!inRAMBankMode)
                            {
                                ramBankNumber = romBankNumber >> 5;
                                romBankNumber &= 0x1F;
                                currentROMBank = rom[romBankNumber];
                                inRAMBankMode = true;
                            }
                        }
                        else if (inRAMBankMode)
                        {
                            romBankNumber = (romBankNumber & 0x1F) | (ramBankNumber << 5);
                            ramBankNumber = 0;
                            currentROMBank = rom[romBankNumber];
                            inRAMBankMode = false;
                        }
                        break;

                    case 0xA000:
                    case 0xB000: // RAM access
                        if (ramEnabled && ram != null)
                            ram[ramBankNumber, address & 0x1FFF] = value;
                        break;
                }
            }
        }

    }
}
