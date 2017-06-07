using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBE.Emulation
{
    unsafe struct SpriteDotData
    {
        private fixed byte data[16];

        public byte this[int address]
        {
            get
            {
                fixed (byte* dataPtr = data)
                {
                    return dataPtr[address & 0x0F];
                }
            }

            set
            {
                fixed (byte* dataPtr = data)
                {
                    dataPtr[address & 0x0F] = value;
                }
            }
        }

        public int GetColorCode(int x, int y)
        {
            byte lb, hb;
            fixed (byte* dataPtr = data)
            {
                lb = dataPtr[y & 7];
                hb = dataPtr[(y & 7) + 1];
            }
            x &= 7;
            return ((lb >> x) & 1) | (x == 0 ? ((hb & 1) << 1) : ((hb >> (x - 1)) & 2));
        }
    }
}
