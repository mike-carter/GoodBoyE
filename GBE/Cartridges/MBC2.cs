namespace GBE.Cartridges
{
    class MBC2 : MemoryController
    {
        public MBC2(int romSize) : base(romSize, 512)
        {
        }

        public override void WriteToRegister(int address, byte value)
        {
            // There is some sort of wonky rule that the least significant bit of the upper
            // address byte must be 0 in order to enable/disable RAM
            if ((address & 0x100) == 0)
            {
                switch (address & 0xF000)
                {
                    case 0:
                    case 0x1000: // Register 0: RAM Enable
                        RAMEnabled = (value & 0x0F) == 0x0A;
                        break;

                    case 0x2000:
                    case 0x3000: // Register 1: ROM Bank number

                        value &= 0x0F; // 4-bit register
                        CurrentROMBank = value == 0 ? 1 : value;
                        break;
                }
            }
        }

        public override void WriteToRAM(int address, byte value)
        {
            // Only the 4 least significant bits are stored
            value &= 0x0F;
            base.WriteToRAM(address, value);
        }
    }
}
