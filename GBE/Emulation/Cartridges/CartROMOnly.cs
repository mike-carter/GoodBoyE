using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBE.Emulation.Cartridges
{
    class CartROMOnly : Cartridge
    {
        private byte[] rom;

        public CartROMOnly(System.IO.Stream cartDataStream)
        {
            rom = new byte[0x8000];

            cartDataStream.Read(rom, 0, 0x8000);
        }

        internal override byte this[int address]
        {
            get
            {
                if (address < 0x8000)
                {
                    return rom[address];
                }
                return 0;
            }

            set { } // No RAM on this cartridge, so there's nothing to set
        }

        public override void Reset()
        {
            // Nothing to reset.
        }
    }
}
