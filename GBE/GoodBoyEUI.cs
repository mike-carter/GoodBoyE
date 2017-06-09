using System;
using System.IO;
using System.Windows.Forms;
using GBE.Emulation;
using GBE.Emulation.Cartridges;

namespace GBE
{
    public partial class GoodBoyEUI : Form
    {
        public GoodBoyEUI()
        {
            InitializeComponent();
        }

        private void GoodBoyEUI_Load(object sender, EventArgs e)
        {
            using (MemoryStream ms = new MemoryStream(TestROMs.TestROMs.Tetris))
            {
                Cartridge cart = new CartROMOnly(ms);

                emulator.InsertCartridge(cart);
            }
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            emulator.Run();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            emulator.Stop();
        }
    }
}
