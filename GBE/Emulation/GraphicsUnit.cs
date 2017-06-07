using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace GBE.Emulation
{
    class GraphicsUnit
    {
        private const int WHITE = 0;
        private const int LIGHT_GRAY = 1;
        private const int DARK_GRAY = 2;
        private const int BLACK = 3;

        #region Instance Variables

        private int gpuClock;

        // GPU VRAM
        private SpriteDotData[] charData;

        private byte[,] bgMap0;
        private byte[,] bgMap1;

        private OAMRegister[] oam;

        // GPU Registers

        // LCDC (LCD Control) Register Flags
        private bool bgEnabled;
        private bool spritesEnabled;
        private bool sprites16Bit;
        private int bgMapSelected;
        private int bgTilesSelected;
        private bool windowEnabled;
        private int windowMapSelected;
        private bool lcdEnabled;

        // STAT (LCD Status) Register Flags
        private enum GPUMode
        {
            HBlank = 0, VBlank = 1, OAMRead = 2, VRAMRead = 3
        }
        GPUMode gpuMode;

        [Flags]
        private enum GPUInterrupts
        {
            OnHBlank = 0x08, OnVBlank = 0x10, OnOAMRead = 0x20, OnMatch = 0x40,
            Mask = 0x18
        }
        GPUInterrupts enabledInterrupts;

        private bool matchFlag;

        // Other Registers
        private byte SCY, SCX; // Background scroll registers

        private byte LY;  // Line being drawn
        private byte LYC; // Line to raise Match interrupt

        private byte WY, WX; // Window position registers

        // TODO: This is temporary until we can get a DMA set up
        private byte DMA;

        // Pallets
        private byte bgPallet;
        private byte[] objPallet = new byte[2];

        // Frame Rendering
        private Bitmap frame0, frame1;
        private int frameRendering;

        // Interrupt Events
        public event Action VBlankInterrupt;

        public event Action LCDCInterrupt;

        #endregion

        #region Memory Access

        private byte LCDCReg
        {
            get
            {
                return (byte)((bgEnabled ? 1 : 0) |
                    (spritesEnabled ? 2 : 0) |
                    (sprites16Bit ? 4 : 0) |
                    (bgMapSelected << 3) |
                    (bgTilesSelected << 4) |
                    (windowEnabled ? 0x20 : 0) |
                    (windowMapSelected << 6) |
                    (lcdEnabled ? 0x80 : 0));
            }

            set
            {
                bgEnabled = (value & 1) != 0;
                spritesEnabled = (value & 2) != 0;
                sprites16Bit = (value & 4) != 0;
                bgMapSelected = (value >> 3) & 1;
                bgTilesSelected = (value >> 4) & 1;
                windowEnabled = (value & 0x20) != 0;
                windowMapSelected = (value >> 6) & 1;
                lcdEnabled = (value & 0x80) != 0;
            }
        }

        private byte STATReg
        {
            get
            {
                return (byte)((int)gpuMode | (matchFlag ? 0x04 : 0) | (int)enabledInterrupts);
            }
            set
            {
                enabledInterrupts = (GPUInterrupts)value & GPUInterrupts.Mask;
            }
        }


        public byte this[int address]
        {
            get
            {
                if (address >= 0xFF40)
                {
                    switch (address & 0x0F)
                    {
                        case 0: return LCDCReg;
                        case 1: return STATReg;
                        case 2: return SCY;
                        case 3: return SCX;
                        case 4: return LY;
                        case 5: return LYC;
                        case 6: return DMA;
                        case 7: return bgPallet;
                        case 8: return objPallet[0];
                        case 9: return objPallet[1];
                        case 10: return WY;
                        case 11: return WX;
                    }
                }
                else if (address >= 0xFE00 && address < 0xFEA0) // Sprite Attribute Memory
                {
                    // OAM access blocked during these modes
                    if (lcdEnabled && (gpuMode == GPUMode.OAMRead || gpuMode == GPUMode.VRAMRead))
                        return 0;

                    switch (address & 3)
                    {
                        case 0: return oam[address & 0xFF].CoordY;
                        case 1: return oam[address & 0xFF].CoordX;
                        case 2: return oam[address & 0xFF].SpriteCode;
                        case 3: return oam[address & 0xFF].Attributes;
                    }
                }
                else if (address >= 0x8000 && address < 0xA000)
                {
                    if (lcdEnabled && gpuMode == GPUMode.VRAMRead)
                        return 0;

                    if (address < 0x9800) // Background and Sprite Character Data
                    {
                        return charData[(address >> 4) & 0x1FF][address & 15];
                    }
                    else if (address < 0x9C00)
                    {
                        return bgMap0[(address >> 5) & 0x1F, address & 0x1F];
                    }
                    else
                    {
                        return bgMap1[(address >> 5) & 0x1F, address & 0x1F];
                    }
                }
                return 0;
            }

            set
            {
                if (address >= 0xFF40)
                {
                    switch (address & 0x0F)
                    {
                        case 0: LCDCReg = value; break;
                        case 1: STATReg = value; break;
                        case 2: SCY = value; break;
                        case 3: SCX = value; break;
                        case 4: LY = value; break;
                        case 5: LYC = value; break;
                        case 6: DMA = value; break;
                        case 7: bgPallet = value; break;
                        case 8: objPallet[0] = value; break;
                        case 9: objPallet[1] = value; break;
                        case 10: WY = value; break;
                        case 11: WX = value; break;
                    }
                }
                else if (address >= 0xFE00 && address < 0xFEA0) // Sprite Attribute Memory
                {
                    // OAM access blocked during these modes
                    if (lcdEnabled && (gpuMode == GPUMode.OAMRead || gpuMode == GPUMode.VRAMRead))
                        return;

                    switch (address & 3)
                    {
                        case 0: oam[address & 0xFF].CoordY = value; break;
                        case 1: oam[address & 0xFF].CoordX = value; break;
                        case 2: oam[address & 0xFF].SpriteCode = value; break;
                        case 3: oam[address & 0xFF].Attributes = value; break;
                    }
                }
                else if (address >= 0x8000 && address < 0xA000)
                {
                    if (lcdEnabled && gpuMode == GPUMode.VRAMRead)
                        return;

                    if (address < 0x9800) // Background and Sprite Character Data
                    {
                        charData[(address >> 4) & 0x1FF][address & 15] = value;
                    }
                    else if (address < 0x9C00)
                    {
                        bgMap0[(address >> 5) & 0x1F, address & 0x1F] = value;
                    }
                    else
                    {
                        bgMap1[(address >> 5) & 0x1F, address & 0x1F] = value;
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        public void Reset()
        {
            charData = new SpriteDotData[0x180];
            bgMap0 = new byte[32, 32];
            bgMap1 = new byte[32, 32];
            oam = new OAMRegister[40];

            LCDCReg = 0;
            STATReg = 0;
            SCY = 0;
            SCX = 0;
            LY = 0;
            LYC = 0;
            WX = 0;
            WY = 0;
            DMA = 0;
            bgPallet = 0;
            objPallet[0] = 0;
            objPallet[1] = 0;
        }

        public void ClockTick(int ticks)
        {
            gpuClock += ticks;

            if (!lcdEnabled)
            {
                gpuClock = 0;
                gpuMode = GPUMode.OAMRead;
            }

            switch (gpuMode)
            {
                case GPUMode.HBlank:
                    if (gpuClock >= 51)
                    {
                        gpuClock -= 51;

                        LY++; // Move to next line
                        if (LY == LYC && (enabledInterrupts & GPUInterrupts.OnMatch) != 0)
                        {
                            LCDCInterrupt?.Invoke();
                        }

                        if (LY == 144) // Move into vertical blank mode
                        {
                            // Begin rendering the frame
                            renderTask = Task.Run(() => RenderFrame());

                            gpuMode = GPUMode.VBlank;

                            // signal interrupts
                            VBlankInterrupt?.Invoke();
                            if ((enabledInterrupts & GPUInterrupts.OnVBlank) != 0)
                            {
                                LCDCInterrupt?.Invoke();
                            }
                        }
                        else // Start rendering next line
                        {
                            gpuMode = GPUMode.OAMRead;
                            if ((enabledInterrupts & GPUInterrupts.OnOAMRead) != 0)
                            {
                                LCDCInterrupt?.Invoke();
                            }
                            renderTask = Task.Run(() => LoadSpriteData());
                        }
                    }
                    break;

                case GPUMode.VBlank:
                    if (gpuClock >= 144)
                    {
                        gpuClock -= 144;

                        LY++;
                        if (LY == LYC && (enabledInterrupts & GPUInterrupts.OnMatch) != 0)
                        {
                            LCDCInterrupt?.Invoke();
                        }

                        if (LY == 154)
                        {
                            renderTask.Wait(); // Wait for the frame to finish rendering

                            // Start rendering the next frame
                            LY = 0;
                            if (LY == LYC && (enabledInterrupts & GPUInterrupts.OnMatch) != 0)
                            {
                                LCDCInterrupt?.Invoke();
                            }

                            gpuMode = GPUMode.OAMRead;
                            if ((enabledInterrupts & GPUInterrupts.OnOAMRead) != 0)
                            {
                                LCDCInterrupt?.Invoke();
                            }
                            renderTask = Task.Run(() => LoadSpriteData());
                        }
                    }
                    break;

                case GPUMode.OAMRead:
                    if (gpuClock >= 20)
                    {
                        renderTask.Wait(); // Wait for sprite load to finish
                        gpuClock -= 20;

                        // Start loading background data
                        gpuMode = GPUMode.VRAMRead;
                        renderTask = Task.Run(() => PreRenderLine());
                    }
                    break;

                case GPUMode.VRAMRead:
                    if (gpuClock >= 43)
                    {
                        renderTask.Wait(); // Wait for pre-rendering to finish
                        gpuClock -= 43;

                        gpuMode = GPUMode.HBlank;
                        if ((enabledInterrupts & GPUInterrupts.OnHBlank) != 0)
                        {
                            LCDCInterrupt?.Invoke();
                        }
                        // No task is run during HBlank
                    }
                    break;
            }
        }

        #endregion

        #region Render Task

        private Task renderTask;
        private byte[,] renderBuf = new byte[144, 160];
        private List<int> spritesList = new List<int>(10);

        private void LoadSpriteData()
        {

        }

        private void PreRenderLine()
        {

        }

        private void RenderFrame()
        {

        }

        #endregion
    }
}
