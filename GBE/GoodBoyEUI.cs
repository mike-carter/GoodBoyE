using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GBE.Emulation;

namespace GBE
{
    public partial class GoodBoyEUI : Form
    {
        private Dictionary<Keys, GBKeys> keyMap = new Dictionary<Keys, GBKeys>()
        {
            { Keys.W, GBKeys.Up },
            { Keys.A, GBKeys.Left },
            { Keys.S, GBKeys.Down },
            { Keys.D, GBKeys.Right },
            { Keys.OemPeriod, GBKeys.A },
            { Keys.Oemcomma, GBKeys.B },
            { Keys.Return, GBKeys.Start },
            { Keys.Back, GBKeys.Select },
        };


        public GoodBoyEUI()
        {
            InitializeComponent();
        }

        private void GoodBoyEUI_Load(object sender, EventArgs e)
        {
            //Cartridge cart = new Cartridge("TestROMs\\Tetris.gb");
            Cartridge cart = new Cartridge("TestROMs\\PokemonRed.gb");

            emulator.InsertCartridge(cart);

            emulator.PowerOn();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (emulator.Running)
            {
                emulator.Stop();
                runButton.Text = "Run";
                restartButton.Enabled = true;
            }
            else
            {
                emulator.Run();
                runButton.Text = "Stop";
                restartButton.Enabled = false;
            }
        }

        private void restartButton_Click(object sender, EventArgs e)
        {
            emulator.PowerOn();
        }

        private void controlButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (sender == aButton)
            {
                emulator.Joypad.PressKey(GBKeys.A);
            }
            else if (sender == bButton)
            {
                emulator.Joypad.PressKey(GBKeys.B);
            }
            else if (sender == startButton)
            {
                emulator.Joypad.PressKey(GBKeys.Start);
            }
            else if (sender == selectButton)
            {
                emulator.Joypad.PressKey(GBKeys.Select);
            }
            else if (sender == upButton)
            {
                emulator.Joypad.PressKey(GBKeys.Up);
            }
            else if (sender == downButton)
            {
                emulator.Joypad.PressKey(GBKeys.Down);
            }
            else if (sender == leftButton)
            {
                emulator.Joypad.PressKey(GBKeys.Left);
            }
            else if (sender == rightButton)
            {
                emulator.Joypad.PressKey(GBKeys.Right);
            }
        }

        private void controlButton_MouseUp(object sender, MouseEventArgs e)
        {
            if (sender == aButton)
            {
                emulator.Joypad.ReleaseKey(GBKeys.A);
            }
            else if (sender == bButton)
            {
                emulator.Joypad.ReleaseKey(GBKeys.B);
            }
            else if (sender == startButton)
            {
                emulator.Joypad.ReleaseKey(GBKeys.Start);
            }
            else if (sender == selectButton)
            {
                emulator.Joypad.ReleaseKey(GBKeys.Select);
            }
            else if (sender == upButton)
            {
                emulator.Joypad.ReleaseKey(GBKeys.Up);
            }
            else if (sender == downButton)
            {
                emulator.Joypad.ReleaseKey(GBKeys.Down);
            }
            else if (sender == leftButton)
            {
                emulator.Joypad.ReleaseKey(GBKeys.Left);
            }
            else if (sender == rightButton)
            {
                emulator.Joypad.ReleaseKey(GBKeys.Right);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (keyMap.ContainsKey(e.KeyCode))
            {
                emulator.Joypad.PressKey(keyMap[e.KeyCode]);
            }
            e.Handled = true;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (keyMap.ContainsKey(e.KeyCode))
            {
                emulator.Joypad.ReleaseKey(keyMap[e.KeyCode]);
            }
            e.Handled = true;
        }

        private void GoodBoyEUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            emulator.ShutDown();
        }
    }
}
