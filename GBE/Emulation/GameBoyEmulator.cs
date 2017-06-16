using System;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace GBE.Emulation
{
    public partial class GameBoyEmulator : UserControl
    {
        internal Z80Processor processor;
        internal MemoryDispatcher memory;
        internal LCDController graphics;
        internal SoundUnit sound;
        internal TimerUnit timer;
        internal Joypad joypad;
        
        private BackgroundWorker gameThread;

        private HashSet<int> breakpoints = new HashSet<int>();

        private bool debugMode = false;

        private long clockms = 0;
        private long syncms = 0;
        
        public GameBoyEmulator()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            InitializeComponent();

            graphics = new LCDController();
            sound = new SoundUnit();
            timer = new TimerUnit();
            joypad = new Joypad();

            memory = new MemoryDispatcher(graphics, sound, timer, joypad);
            processor = new Z80Processor(memory);

            gameThread = new BackgroundWorker();
            gameThread.DoWork += GameThread_DoWork;
            gameThread.RunWorkerCompleted += GameThread_RunWorkerCompleted;
            gameThread.WorkerSupportsCancellation = true;

            frameTimer.Interval = 15; // Run at 60 FPS
            syncTimer.Interval = 8;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public Joypad Joypad
        {
            get { return joypad; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool Running
        {
            get { return gameThread.IsBusy; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool DebugMode
        {
            get { return debugMode; }
            set { debugMode = true; }
        }

        public void InsertCartridge(Cartridge cartridge)
        {
            if (gameThread.IsBusy)
            {
                throw new InvalidOperationException("Game is running.");
            }
            memory.Cartridge = cartridge;
        }

        public void PowerOn()
        {
            if (memory.Cartridge == null)
            {
                throw new InvalidOperationException("No cartridge loaded.");
            }
            if (gameThread.IsBusy)
            {
                throw new InvalidOperationException("Game is already running.");
            }

            processor.ClearRegisters();
            graphics.PowerOn();
            sound.Reset();

            // Turn on BIOS
            memory.BIOSLoaded = true;

            using (Graphics myGraphics = CreateGraphics())
            {
                myGraphics.FillRectangle(Brushes.White, ClientRectangle);
            }
        }
        
        public void Run()
        {
            if (gameThread.IsBusy)
            {
                throw new InvalidOperationException("Game is already running.");
            }

            clockms = 0;
            syncms = 0;

            syncTimer.Start();
            gameThread.RunWorkerAsync();
            frameTimer.Start();
        }

        public void Stop()
        {
            gameThread.CancelAsync();
        }

        public void ShutDown()
        {
            memory.Cartridge.Save();
        }

        private void DrawFrame()
        {
            using (Graphics myGraphics = CreateGraphics())
            {
                Image frame = graphics.GetFrame();

                myGraphics.DrawImage(frame, ClientRectangle);
            }
        }

        #region Emulator Loop

        private void GameThread_DoWork(object sender, DoWorkEventArgs e)
        {
            int usTicks = 0;

            // Main Emulation Loop
            while (!gameThread.CancellationPending)
            {
                try
                {
                    int ticks = processor.Step();

                    timer.Increment(ticks);
                    graphics.ClockTick(ticks);
                    memory.DMAClockTick(ticks);

                    usTicks += ticks;

                    if (usTicks >= 1000)
                    {
                        clockms++;
                        usTicks = 0;
                    }

                    while (clockms > syncms)
                    {
                        Thread.Sleep(1);
                    }
                }
                catch  { }
            }
        }

        private void GameThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            frameTimer.Stop();
            syncTimer.Stop();
        }

        private void frameTimer_Tick(object sender, EventArgs e)
        {
            DrawFrame();
        }
        
        private void emulationTimer_Tick(object sender, EventArgs e)
        {
            syncms += 17;
        }

        #endregion
    }
}
