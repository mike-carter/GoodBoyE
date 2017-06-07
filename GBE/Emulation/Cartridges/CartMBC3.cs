using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBE.Emulation.Cartridges
{
    class CartMBC3 : Cartridge
    {
        private byte[][] rom;
        private byte[] rom0;
        private byte[] currentROMBank;

        private byte[,] ram;
        private int ramBankNumber;
        private bool ramEnabled;

        DateTime latchedTime;

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

            LatchTime();
        }

        internal override byte this[int address]
        {
            get
            {
                //if (address < 0x4000)
                //{
                //    return rom0[address];
                //}
                //else if (address < 0x8000)
                //{
                //    return currentROMBank[address & 0x3FFF];
                //}
                //else if (ramEnabled && address >= 0xA000 && address < 0xC000)
                //{
                //    switch (ramBankNumber)
                //    {
                //        case 0:
                //        case 1:
                //        case 2:
                //        case 3:
                //            return ram[ramBankNumber, address & 0x1FFF];

                //        case 8:
                //            return secondsReg;
                //        case 9:
                //            return minutesReg;
                //        case 10:
                //            return hoursReg;
                //        case 11:
                //            return (byte)(daysReg & )
                //    }
                //}
                return 0;
            }

            set
            {

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
