namespace GBE.Cartridges
{
    class ROMOnlyController : MemoryController
    {
        public ROMOnlyController(int ramSize) : base(0x8000, ramSize)
        {
        }

        public override void WriteToRegister(int address, byte value)
        {
            // There are no registers
        }
    }
}
