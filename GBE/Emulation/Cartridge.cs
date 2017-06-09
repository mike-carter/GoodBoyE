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

                    case 1:
                    case 2:
                    case 3:
                        return new CartMBC1(stream);

                    case 15:
                    case 16:
                    case 17:
                    case 18:
                    case 19:
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
