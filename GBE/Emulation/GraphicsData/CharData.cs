namespace GBE.Emulation.GraphicsData
{
    unsafe struct CharData
    {
        private fixed byte data[16];


        public byte ReadByte(int index)
        {
            fixed (byte *dataptr = data)
            {
                return dataptr[index];
            }
        }


        public void WriteByte(int index, byte value)
        {
            fixed (byte *dataptr = data)
            {
                dataptr[index] = value;
            }
        }
        

        public byte GetPixelShade(int py, int px)
        {
            py &= 7;
            px &= 7;

            int lo, hi;
            fixed (byte *dataptr = data)
            {
                py <<= 1;
                lo = dataptr[py];
                hi = dataptr[py + 1];
            }

            // mask off the bits 
            lo = (lo >> (7 - px)) & 1;
            hi = (hi >> (7 - px)) & 1;

            // combine the bits
            return (byte)(lo | (hi << 1));
        }
    }
}
