using System;

namespace GBE.Emulation
{
    class TimerUnit
    {
        private byte divReg = 0;
        private byte countReg = 0;
        private byte moduloReg = 0;

        private bool timerEnabled = false;
        private int timerFrequency = 0;

        private int mtracker = 0;
        private int ticks = 0;

        public byte DivisionRegister
        {
            get { return divReg; }
            set { divReg = (byte)(value & 0xF0); }
        }

        public byte CountRegister
        {
            get { return countReg; }
            set { countReg = value; }
        }

        public byte ModuloRegister
        {
            get { return moduloReg; }
            set { moduloReg = value; }
        }

        public byte ControlRegister
        {
            get
            {
                return (byte)(timerFrequency + (timerEnabled ? 4 : 0));
            }
            set
            {
                timerEnabled = (value & 4) != 0;
                timerFrequency = value & 3;
            }
        }

        public event Action TimerOverflow;

        public void Reset()
        {
            divReg = 0;
            countReg = 0;
            moduloReg = 0;
            timerEnabled = false;
            timerFrequency = 0;
            mtracker = 0;
            ticks = 0;
        }

        public void Increment(int mticks)
        {
            mtracker += mticks;

            if (mtracker >= 4)
            {
                mtracker -= 4;
                ticks++;

                if ((ticks & 15) == 0)
                {
                    divReg++;
                }

                if (timerEnabled)
                {
                    bool increment = false;

                    switch (timerFrequency)
                    {
                        case 0:
                            increment = (ticks & 0x3F) == 0; break;
                        case 1:
                            increment = true; break;
                        case 2:
                            increment = (ticks & 3) == 0; break;
                        case 3:
                            increment = (ticks & 15) == 0; break;
                    }

                    if (increment)
                    {
                        countReg++;
                        if (countReg == 0)
                        {
                            countReg = moduloReg;
                            TimerOverflow?.Invoke();
                        }
                    }
                }
            }
        }
    }
}
