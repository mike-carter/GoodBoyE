namespace GBE.Emulation
{
    struct SpriteDotData
    {
        private byte[,] dotData;

        public byte this[int address]
        {
            get
            {
                if (dotData == null)
                    return 0;

                address &= 0xFF;
                if ((address & 1) == 0) // Even address: Low Byte
                {
                    address >>= 1;
                    byte value = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        value <<= 1;
                        value |= (byte)(dotData[address, i] & 1); 
                    }
                    return value;
                }
                else
                {
                    address >>= 1;
                    byte value = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        value <<= 1;
                        value |= (byte)((dotData[address, i] >> 1) & 1);
                    }
                    return value;
                }
            }

            set
            {
                if (dotData == null && value != 0)
                {
                    dotData = new byte[8, 8];
                }
                if (dotData != null)
                {
                    if ((address & 1) == 0)
                    {
                        address = (address & 0xFF) >> 1;
                        for (int i = 0; i < 8; i++)
                        {
                            dotData[address, i] &= 2;
                            dotData[address, i] |= (byte)((value >> (7 - i)) & 1);
                        }
                    }
                    else
                    {
                        address = (address & 0xFF) >> 1;
                        for (int i = 0; i < 8; i++)
                        {
                            dotData[address, i] &= 1;
                            dotData[address, i] |= (byte)(((value >> (7 - i)) & 1) << 1);
                        }
                    }
                }
            }
        }

        public byte GetColorCode(int y, int x)
        {
            if (dotData == null)
            {
                return 0;
            }
            return dotData[y & 7, x & 7];
        }
    }
}
