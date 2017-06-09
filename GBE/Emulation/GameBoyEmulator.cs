using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace GBE.Emulation
{
    public partial class GameBoyEmulator : UserControl
    {
        Z80Processor processor;
        MemoryDispatcher memory;
        GraphicsUnit graphics;
        SoundUnit sound;
        Timer timer;
        
        private BackgroundWorker gameThread;

        bool stopped = false;
        
        public GameBoyEmulator()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            InitializeComponent();

            graphics = new GraphicsUnit();
            sound = new SoundUnit();
            timer = new Timer();

            memory = new MemoryDispatcher(graphics, sound, timer);
            processor = new Z80Processor(memory);

            gameThread = new BackgroundWorker();
            gameThread.DoWork += GameThread_DoWork;
            gameThread.WorkerSupportsCancellation = true;

            frameTimer.Interval = 15; // Run at 60 FPS
        }

        public void InsertCartridge(Cartridge cartridge)
        {
            if (gameThread.IsBusy)
            {
                throw new InvalidOperationException("Game is running.");
            }

            processor.Reset();
            memory.Reset();
            graphics.Reset();
            //sound.Reset();
            
            memory.Cartridge = cartridge;
        }
        
        public void Run()
        {
            processor.Reset();
            memory.Reset();
            graphics.Reset();

            if (memory.Cartridge == null)
            {
                throw new InvalidOperationException("No cartridge loaded.");
            }
            if (gameThread.IsBusy)
            {
                throw new InvalidOperationException("Game is already running.");
            }

            gameThread.RunWorkerAsync();
            frameTimer.Start();
        }

        public void Stop()
        {
            gameThread.CancelAsync();
            frameTimer.Stop();
        }
        
        private void GameThread_DoWork(object sender, DoWorkEventArgs e)
        {
            // Main Emulation Loop
            while (!gameThread.CancellationPending)
            {
                try
                {
                    int ticks = processor.Step();

                    timer.Increment(ticks);
                    graphics.ClockTick(ticks);
                }
                catch
                {
                    
                }
            }
        }

        private void frameTimer_Tick(object sender, EventArgs e)
        {
            using (Graphics myGraphics = CreateGraphics())
            {
                Image frame = graphics.GetFrame();

                myGraphics.DrawImage(frame, ClientRectangle);
            }
        }

    }
}
