using System;
using System.Xml;

namespace GBE.Cartridges
{
    class MBC3 : MemoryController
    {
        private CartridgeClock clock;

        public MBC3(int romSize, int ramSize, bool hasClock) : base(romSize, ramSize)
        {
            if (hasClock)
            {
                clock = new CartridgeClock();
            }
            else
            {
                clock = null;
            }
        }

        protected override void ReadXML(XmlReader reader)
        {
            base.ReadXML(reader);

            if (reader.ReadToFollowing("clock"))
            {
                clock?.Dispose();

                reader.Read();
                clock = CartridgeClock.FromXML(reader);
            }
        }

        protected override void WriteXML(XmlWriter writer)
        {
            base.WriteXML(writer);

            if (clock != null)
            {
                writer.WriteStartElement("clock");
                clock.SaveXML(writer);
                writer.WriteEndElement();
            }
        }

        public override byte ReadFromRAM(int address)
        {
            if (RAMEnabled )
            {
                if (RAMBanks != null && CurrentRAMBank < 4)
                {
                    return RAMBanks[CurrentRAMBank][address & 0x1FFF];
                }

                else if (clock != null)
                {
                    switch (CurrentRAMBank)
                    {
                        case 8: return clock.SecondsCount;
                        case 9: return clock.MinutesCount;
                        case 10: return clock.HoursCount;
                        case 11: return clock.DaysCountLoByte;
                        case 12: return clock.DaysCountHiByte;
                    }
                }
            }
            return 0;
        }

        public override void WriteToRAM(int address, byte value)
        {
            if (RAMEnabled && RAMBanks.Length > 0)
            {
                if (RAMBanks != null && CurrentRAMBank < 4)
                {
                    RAMBanks[CurrentRAMBank][address & 0x1FFF] = value;
                }

                else if (clock != null)
                {
                    switch (CurrentRAMBank)
                    {
                        case 8: clock.SecondsCount = value; break;
                        case 9: clock.MinutesCount = value; break;
                        case 10: clock.HoursCount = value; break;
                        case 11: clock.DaysCountLoByte = value; break;
                        case 12: clock.DaysCountHiByte = value; break;
                    }
                }
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

                    value &= 0x7F;
                    CurrentROMBank = value == 0 ? 1 : value;
                    break;

                case 0x4000:
                case 0x5000: // Register 2: RAM bank number and RTC switch

                    CurrentRAMBank = value & 0x0F;
                    break;

                case 0x6000:
                case 0x7000: // Register 3: Latch Clock Data

                    value &= 1; // This is a 1-bit register
                    if (value == 0)
                    {
                        clock?.ResetLatch();
                    }
                    else
                    {
                        clock?.LatchTime();
                    }
                    break;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            clock?.Dispose();
        }
    }
}
