using System.IO;

namespace GBE.Cartridges
{
    class MBC1 : MemoryController
    {
        private bool ramSwitchEnabled;

        public MBC1(int ramSize, int romSize) : base(ramSize, romSize)
        {
        }

        public override void PowerOn()
        {
            base.PowerOn();
            ramSwitchEnabled = false;
        }

        public override void LoadROM(Stream stream)
        {
            for (int i = 0; i < ROMBanks.Length; i++)
            {
                // Due to how RAM/ROM switching works, ROM banks 0x20, 0x40, and 0x60 are inaccessable,
                // so we just skip those.
                if ((i & 0x1F) == 0)
                {
                    i++;
                }

                ROMBanks[i] = new byte[0x4000];
                stream.Read(ROMBanks[i], 0, 0x4000);
            }
        }

        public override void WriteToRegister(int address, byte value)
        {
            switch (address & 0xF000)
            {
                case 0:
                case 0x1000: // Register 0: RAM Gate

                    RAMEnabled = (value & 0x0F) == 0x0A;
                    break;

                case 0x2000:
                case 0x3000: // Register 1: ROM Bank number

                    value &= 0x1F; // Only the lower 5 bits are considered
                    if (value == 0)
                    {
                        value = 1;
                    }
                    CurrentROMBank = (CurrentROMBank & 0xFFE0) | (value & 0x1F);
                    break;

                case 0x4000:
                case 0x5000: // Register 2: RAM bank number, or Upper ROM bank bits

                    value &= 3; // Only care about lower 3 bits
                    if (ramSwitchEnabled)
                    {
                        CurrentRAMBank = value;
                    }
                    else
                    {
                        CurrentROMBank = (CurrentROMBank & 0x1F) | (value << 5);
                    }
                    break;

                case 0x6000:
                case 0x7000: // Register 3: ROM/RAM Mode select

                    value &= 1; // This is a 1-bit register

                    if (ramSwitchEnabled && (value == 0))
                    {
                        ramSwitchEnabled = false;
                        CurrentROMBank |= CurrentRAMBank << 5;
                        CurrentRAMBank = 0;
                    }
                    else if (!ramSwitchEnabled && (value != 0))
                    {
                        ramSwitchEnabled = true;
                        CurrentRAMBank = CurrentROMBank >> 5;
                        CurrentROMBank &= 0x1F;
                    }
                    break;
            }
        }
    }
}
