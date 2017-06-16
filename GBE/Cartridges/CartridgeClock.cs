using System;
using System.Threading;
using System.Xml;

namespace GBE.Cartridges
{
    internal class CartridgeClock : IDisposable
    {
        private const string DateTag = "Date";
        private const string SecondsTag = "Seconds";
        private const string MinutesTag = "Minutes";
        private const string HoursTag = "Hours";
        private const string DaysTag = "Days";
        private const string HaltedTag = "Halted";
        private const string CarryTag = "Carry";

        private Timer timer;
        
        private byte secCounter = 0;
        private byte minCounter = 0;
        private byte hourCounter = 0;
        private ushort daysCounter = 0;
        private bool countersLocked = false;

        private byte latchedSeconds = 0;
        private byte latchedMinutes = 0;
        private byte latchedHours = 0;
        private ushort latchedDays = 0;

        private bool halted = false;
        private bool carryBitSet = false;

        private bool timeLatched = false;
        private bool latching;
        
        public CartridgeClock()
        {
            // Set up a timer to count every second. 
            // Begin counting one second after creation.
            timer = new Timer(new TimerCallback(TimerTick), null, 1000, 1000);
        }

        public byte SecondsCount
        {
            get { return latchedSeconds; }
            set
            {
                while (countersLocked) { /* block until counters are available */ }
                countersLocked = true;

                secCounter = value;
                if (secCounter >= 60)
                {
                    // What happens when the value is out of range is not well documented.
                    // For simplicity's sake, just reset the counter. (same for the others)
                    secCounter = 0;
                }

                countersLocked = false;
            }
        }

        public byte MinutesCount
        {
            get { return latchedMinutes; }
            set
            {
                while (countersLocked) { /* block until counters are available */ }
                countersLocked = true;

                minCounter = value;
                if (minCounter >= 60)
                {
                    minCounter = 0;
                }

                countersLocked = false;
            }
        }

        public byte HoursCount
        {
            get { return latchedHours; }
            set
            {
                while (countersLocked) { /* block until counters are available */ }
                countersLocked = true;

                hourCounter = value;
                if (hourCounter >= 24)
                {
                    hourCounter = 0;
                }

                countersLocked = false;
            }
        }

        public byte DaysCountLoByte
        {
            get { return (byte)(latchedDays & 0xFF); }
            set
            {
                while (countersLocked) { /* block until counters are available */ }
                countersLocked = true;

                daysCounter = (ushort)((daysCounter & 0x100) | value);

                countersLocked = false;
            }
        }

        public byte DaysCountHiByte
        {
            get
            {
                return (byte)((latchedDays >> 8) | (carryBitSet ? 0x80 : 0) | (halted ? 0x40 : 0));
            }
            set
            {
                while (countersLocked) { /* block until counters are available */ }
                countersLocked = true;

                daysCounter = (ushort)((daysCounter & 0xFF) | ((value & 1) << 8));
                carryBitSet = (value & 0x80) != 0;

                countersLocked = false;
                
                if (halted && (value & 0x40) == 0)
                {
                    halted = false;

                    // Restart the timer
                    timer.Change(1000, 1000);
                }
                else if (!halted && (value & 0x40) != 0)
                {
                    halted = true;

                    // Stop the timer
                    timer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
        }
        

        public void LatchTime()
        {
            if (!timeLatched)
            {
                while (countersLocked) { /* block until counters are available */ }
                countersLocked = true;

                latchedSeconds = secCounter;
                latchedMinutes = minCounter;
                latchedHours = hourCounter;
                latchedDays = daysCounter;

                countersLocked = false;
            }
        }


        public void ResetLatch()
        {
            timeLatched = false;
        }

        
        public void SaveXML(XmlWriter writer)
        {
            // Save the current time so that we can calculate how much time has passed
            // since the last time this cartridge was loaded.
            writer.WriteStartElement(DateTag);
            writer.WriteValue(DateTime.UtcNow);
            writer.WriteEndElement();

            // Save the counters
            while (countersLocked) { /* block until counters are available */ }
            countersLocked = true;

            writer.WriteStartElement(SecondsTag);
            writer.WriteValue(secCounter);
            writer.WriteEndElement();

            writer.WriteStartElement(MinutesTag);
            writer.WriteValue(minCounter);
            writer.WriteEndElement();

            writer.WriteStartElement(HoursTag);
            writer.WriteValue(hourCounter);
            writer.WriteEndElement();

            writer.WriteStartElement(DaysTag);
            writer.WriteValue(daysCounter);
            writer.WriteEndElement();

            writer.WriteStartElement(CarryTag);
            writer.WriteValue(carryBitSet);
            writer.WriteEndElement();

            countersLocked = false;

            writer.WriteStartElement(HaltedTag);
            writer.WriteValue(halted);
            writer.WriteEndElement();
        }


        public static CartridgeClock FromXML(XmlReader reader)
        {
            CartridgeClock clock = new CartridgeClock();
            clock.timer.Change(Timeout.Infinite, Timeout.Infinite); // Stop the timer until we're finished loading.

            reader.ReadToFollowing(DateTag);
            DateTime timeSince = reader.ReadElementContentAsDateTime();
            reader.ReadEndElement();

            reader.ReadStartElement(SecondsTag);
            clock.secCounter = (byte)reader.ReadElementContentAsInt();
            reader.ReadEndElement();

            reader.ReadStartElement(MinutesTag);
            clock.minCounter = (byte)reader.ReadElementContentAsInt();
            reader.ReadEndElement();

            reader.ReadStartElement(HoursTag);
            clock.hourCounter = (byte)reader.ReadElementContentAsInt();
            reader.ReadEndElement();

            reader.ReadStartElement(DaysTag);
            clock.daysCounter = (ushort)reader.ReadElementContentAsInt();
            reader.ReadEndElement();

            reader.ReadStartElement(CarryTag);
            clock.carryBitSet = reader.ReadElementContentAsBoolean();
            reader.ReadEndElement();

            reader.ReadStartElement(HaltedTag);
            clock.halted = reader.ReadElementContentAsBoolean();
            reader.ReadEndElement();

            // Unless the clock has been halted all this time, advance the counters
            // by the amount since the cartridge was saved.
            if (!clock.halted)
            {
                DateTime now = DateTime.UtcNow;
                
                // If the current time on the computer does not come after the recorded time,
                // then don't advance the counters
                if (DateTime.Compare(now, timeSince) > 0)
                {
                    TimeSpan timespan = now - timeSince;

                    clock.secCounter += (byte)timespan.Seconds;
                    if (clock.secCounter >= 60)
                    {
                        clock.secCounter -= 60;
                        clock.minCounter += 1;
                    }

                    clock.minCounter += (byte)timespan.Minutes;
                    if (clock.minCounter >= 60)
                    {
                        clock.minCounter -= 60;
                        clock.hourCounter += 1;
                    }

                    clock.hourCounter += (byte)timespan.Hours;
                    if (clock.hourCounter >= 24)
                    {
                        clock.hourCounter -= 24;
                        clock.daysCounter += 1;
                    }

                    clock.daysCounter += (ushort)timespan.Days;
                    while (clock.daysCounter >= 0x200)
                    {
                        clock.daysCounter -= 0x200;
                        clock.carryBitSet = true;
                    }
                }

                // Restart the timer
                clock.timer.Change(1000, 1000);
            }

            return clock;
        }

        private void TimerTick(object o)
        {
            while (countersLocked) { /* block until counters are available */ }
            countersLocked = true;

            // Increment counters appropriately
            secCounter++;

            if (secCounter == 60)
            {
                secCounter = 0;
                minCounter++;

                if (minCounter == 60)
                {
                    minCounter = 0;
                    hourCounter++;

                    if (hourCounter == 24)
                    {
                        hourCounter = 0;
                        daysCounter++;

                        if (daysCounter == 0x200)
                        {
                            daysCounter = 0;
                            carryBitSet = true;
                        }
                    }
                }
            }

            countersLocked = false;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    timer.Change(Timeout.Infinite, Timeout.Infinite);
                    timer.Dispose();
                }

                disposedValue = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
