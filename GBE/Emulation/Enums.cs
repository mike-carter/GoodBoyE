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

    [Flags]
    enum DirectionKeys : byte
    {
        Right = 1,
        Left = 2,
        Up = 4,
        Down = 8
    }

    [Flags]
    enum ButtonKeys : byte
    {
        A = 1,
        B = 2,
        Select = 4,
        Start = 8
    }
}
