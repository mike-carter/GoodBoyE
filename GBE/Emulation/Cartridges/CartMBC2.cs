using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBE.Emulation.Cartridges
{
    class CartMBC2 : Cartridge
    {
        private byte[][] rom;
        private byte[] rom0;
        private byte[] currentROMBank;

        private byte[] ram;
        private bool ramEnabled;

        public CartMBC2(System.IO.Stream cartDataStream)
        {
            rom = new byte[0x0F][];

            // Read bank 0 first, since it is addressed differently.
            rom0 = new byte[0x4000];
            cartDataStream.Read(rom0, 0, 0x4000);
            rom[0] = rom0;

            for (int i = 1; i < 0x0F; i++)
            {
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
            currentROMBank = rom[1];

            ram = new byte[0x200];
            ramEnabled = false;
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
                else if (ramEnabled && address >= 0xA000 && address < 0xA200)
                {
                    return ram[address & 0x1FF];
                }
                return 0;
            }

            set
            {
                switch (address & 0xF000)
                {
                    default: break;

                    case 0:
                    case 0x1000: // Register 0: RAM Enable
                        // Enable by Writing XXXX1010 (X == don't cares). Any other value disables RAM.
                        if ((address & 0x0100) == 0)
                            ramEnabled = (value & 0x0F) == 0x0A;
                        break;

                    case 0x2000:
                    case 0x3000: // Register 1: ROM bank number.
                        // Again, the least significant bit of the high-byte of the address must be 0 to enable/disable.
                        // Writing 0 is the same as writing 1.
                        value &= 0x0F;
                        currentROMBank = rom[value == 0 ? 1 : value];
                        break;

                    case 0xA000: // 
                        // The RAM on the MBC2 controller only supports 4-bit values, so only
                        // the lower 4 bits are written.
                        if (ramEnabled && address < 0xA200)
                            ram[address & 0x1FF] = (byte)(value & 0x0F);
                        break;
                }
            }
        }
    }
}
