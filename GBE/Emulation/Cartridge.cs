using System;
using System.IO;
using GBE.Emulation.Cartridges;

namespace GBE.Emulation
{
    public abstract class Cartridge
    {
        public static Cartridge LoadROM(string romFileName)
        {
            using (FileStream stream = new FileStream(romFileName, FileMode.Open, FileAccess.Read))
            {
                // Position 0x0147 in the cartridge header contains the cartridge type.
                stream.Seek(0x0147, SeekOrigin.Begin);

                int cartType = stream.ReadByte();
                stream.Seek(0, SeekOrigin.Begin);

                switch (cartType)
                {
                    case 0:
                        return new CartROMOnly(stream);

                    case 0x13:
                        return new CartMBC3(stream);
                }
            }

            return null;
        }

        internal abstract byte this[int address]
        {
            get; set;
        }

        public abstract void Reset();
    }
}
