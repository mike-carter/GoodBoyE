using System;

namespace GBE.Emulation
{
    [Flags]
    public enum GBKeys : byte
    {
        None = 0,
        Right = 0x10,
        Left = 0x20,
        Up = 0x40,
        Down = 0x80,
        A = 0x1,
        B = 0x2,
        Select = 0x4,
        Start = 0x8,
    }

    public class Joypad
    {
        private GBKeys keysPressed;
        private byte P1 = 0xFF;

        internal event Action JoypadInterrupt;

        internal byte P1Register
        {
            get
            {
                return P1;
            }
            set
            {
                switch (value & 0x30)
                {
                    case 0x10: // Query direction ports
                        P1 = (byte)(0xDF ^ (((int)keysPressed & 0x0F)));
                        break;

                    case 0x20: // Query button ports
                        P1 = (byte)(0xEF ^ (((int)keysPressed & 0xF0) >> 4));
                        break;

                    case 0x30: // Reset
                        P1 = 0xCF;
                        break;
                }
            }
        }

        public void PressKey(GBKeys keys)
        {
            if ((keysPressed & keys) != GBKeys.None)
            {
                JoypadInterrupt?.Invoke();
            }
            keysPressed |= keys;
        }

        public void ReleaseKey(GBKeys keys)
        {
            keysPressed &= ~keys;
        }
    }
}
