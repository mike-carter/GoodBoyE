using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBE.Emulation
{
    class Joypad
    {
        DirectionKeys directionsPressed;
        ButtonKeys buttonsPressed;

        byte P1;

        public event Action JoypadInterrupt;

        public byte P1Register
        {
            get
            {
                return P1;
            }
            set
            {
                switch (value & 0x30)
                {
                    case 0x10:
                        P1 = (byte)(0xE0 | (int)directionsPressed);
                        break;

                    case 0x20:
                        P1 = (byte)(0xD0 | (int)buttonsPressed);
                        break;

                    case 0x30:
                        P1 = 0xFF;
                        break;
                }
            }
        }

        public void PressDirection(DirectionKeys direction)
        {
            int portValues = (int)directionsPressed & (int)buttonsPressed;
            if ((portValues & (int)direction) == (int)direction)
            {
                JoypadInterrupt?.Invoke();
            }
            directionsPressed &= ~direction;
        }

        public void ReleaseDirection(DirectionKeys direction)
        {
            directionsPressed |= direction;
        }

        public void PressButton(ButtonKeys button)
        {
            int portValues = (int)directionsPressed & (int)buttonsPressed;
            if ((portValues & (int)button) == (int)button)
            {
                JoypadInterrupt?.Invoke();
            }
            buttonsPressed &= ~button;
        }

        public void ReleaseButton(ButtonKeys button)
        {
            buttonsPressed |= button;
        }
    }
}
