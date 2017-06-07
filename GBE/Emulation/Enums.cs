using System;

namespace GBE.Emulation
{
    [Flags]
    enum InterruptFlags : byte
    {
        None = 0,
        VerticalBlank = 1,
        LCDCStatus = 2,
        TimerOverflow = 4,
        SerialTxComplete = 8,
        InputSignalLow = 16
    }
}
