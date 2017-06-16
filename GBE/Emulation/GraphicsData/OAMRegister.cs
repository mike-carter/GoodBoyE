namespace GBE.Emulation.GraphicsData
{
    struct OAMRegister
    {
        public byte CoordY;
        public byte CoordX;
        public byte SpriteCode;
        public byte Attributes;

        public int Pallet
        {
            get { return (Attributes >> 4) & 1; }
        }

        public bool HorizontalFlip
        {
            get { return (Attributes & 0x20) != 0; }
        }

        public bool VerticalFlip
        {
            get { return (Attributes & 0x40) != 0; }
        }

        public bool BGPriority
        {
            get { return (Attributes & 0x80) != 0; }
        }
    }
}
