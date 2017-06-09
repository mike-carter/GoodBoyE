using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace GBE.Emulation
{
    class GraphicsUnit
    {
        public const int ScreenPixelWidth = 160;
        public const int ScreenPixelHeight = 144;


        private const int WHITE = 0;
        private const int LIGHT_GRAY = 1;
        private const int DARK_GRAY = 2;
        private const int BLACK = 3;

        #region Instance Variables

        private int gpuClock;

        // GPU VRAM
        private SpriteDotData[] objSprites = new SpriteDotData[0x80];
        private SpriteDotData[] bgSprites = new SpriteDotData[0x80];
        private SpriteDotData[] sharedSprites = new SpriteDotData[0x80];

        private byte[,] bgMap0 = new byte[32, 32];
        private byte[,] bgMap1 = new byte[32, 32];

        private OAMRegister[] oam = new OAMRegister[40];

        // GPU Registers

        // LCDC (LCD Control) Register Flags
        private bool bgEnabled;
        private bool spritesEnabled;
        private bool sprites16Bit;
        private byte[,] bgMapSelected;
        private SpriteDotData[] bgDataSelected;
        private bool windowEnabled;
        private byte[,] windowMapSelected;
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
        private Bitmap frame0 = new Bitmap(ScreenPixelWidth, ScreenPixelHeight);
        private Bitmap frame1 = new Bitmap(ScreenPixelWidth, ScreenPixelHeight);
        private int frameRendering;

        // Interrupt Events
        public event Action VBlankInterrupt;

        public event Action LCDCInterrupt;

        #endregion

        public GraphicsUnit()
        {
            using (Graphics g = Graphics.FromImage(frame0))
            {
                g.FillRectangle(Brushes.White, 0, 0, ScreenPixelWidth, ScreenPixelHeight);
            }
            using (Graphics g = Graphics.FromImage(frame0))
            {
                g.FillRectangle(Brushes.White, 0, 0, ScreenPixelWidth, ScreenPixelHeight);
            }
        }

        #region Memory Access

        private byte LCDCReg
        {
            get
            {
                return (byte)((bgEnabled ? 1 : 0) |
                    (spritesEnabled ? 2 : 0) |
                    (sprites16Bit ? 4 : 0) |
                    (bgMapSelected == bgMap0 ? 0 : 8) |
                    (bgDataSelected == bgSprites ? 0 : 0x10) |
                    (windowEnabled ? 0x20 : 0) |
                    (windowMapSelected == bgMap0 ? 0 : 0x40) |
                    (lcdEnabled ? 0x80 : 0));
            }

            set
            {
                bgEnabled = (value & 1) != 0;
                spritesEnabled = (value & 2) != 0;
                sprites16Bit = (value & 4) != 0;
                bgMapSelected = (value & 8) == 0 ? bgMap0 : bgMap1;
                bgDataSelected = (value & 0x01) == 0 ? bgSprites : objSprites;
                windowEnabled = (value & 0x20) != 0;
                windowMapSelected = (value & 0x40) == 0 ? bgMap0 : bgMap1;
                lcdEnabled = (value & 0x80) != 0;

                //if (!lcdEnabled)
                //{

                //}
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
                        case 0: return oam[(address >> 2) & 0xFF].CoordY;
                        case 1: return oam[(address >> 2) & 0xFF].CoordX;
                        case 2: return oam[(address >> 2) & 0xFF].SpriteCode;
                        case 3: return oam[(address >> 2) & 0xFF].Attributes;
                    }
                }
                else if (address >= 0x8000 && address < 0xA000)
                {
                    if (lcdEnabled && gpuMode == GPUMode.VRAMRead)
                        return 0;

                    if (address < 0x8800) // Background and Sprite Character Data
                    {
                        return objSprites[(address >> 4) & 0x7F][address & 15];
                    }
                    else if (address < 0x9000)
                    {
                        return sharedSprites[(address >> 4) & 0x7F][address & 15];
                    }
                    else if (address < 0x9800)
                    {
                        return bgSprites[(address >> 4) & 0x7F][address & 15];
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
                        //case 4: LY = value; break;
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

                    address &= 0xFF;
                    switch (address & 3)
                    {
                        case 0: oam[address >> 2].CoordY = value; break;
                        case 1: oam[address >> 2].CoordX = value; break;
                        case 2: oam[address >> 2].SpriteCode = value; break;
                        case 3: oam[address >> 2].Attributes = value; break;
                    }
                }
                else if (address >= 0x8000 && address < 0xA000)
                {
                    if (lcdEnabled && gpuMode == GPUMode.VRAMRead)
                        return;

                    if (address < 0x8800) // Background and Sprite Character Data
                    {
                        objSprites[(address >> 4) & 0x7F][address & 15] = value;
                    }
                    else if (address < 0x9000)
                    {
                        sharedSprites[(address >> 4) & 0x7F][address & 15] = value;
                    }
                    else if (address < 0x9800)
                    {
                        bgSprites[(address >> 4) & 0x7F][address & 15] = value;
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
                LY = 0;
                gpuMode = GPUMode.OAMRead;
                return;
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
                        renderTask?.Wait(); // Wait for sprite load to finish
                        gpuClock -= 20;

                        // Start loading background data
                        gpuMode = GPUMode.VRAMRead;
                        renderTask = Task.Run(() => PreRender());
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

        public Image GetFrame()
        {
            if (frameRendering == 0)
            {
                return frame1;
            }
            return frame0;
        }

        #endregion

        #region Render Task

        private Task renderTask;
        private byte[,] renderBuf = new byte[ScreenPixelHeight, ScreenPixelWidth];
        private List<int> spritesList = new List<int>(10);
        private byte[] bgPixels = new byte[ScreenPixelWidth];

        private Pen[] penColors = new Pen[]
        {
            Pens.White,
            Pens.Silver,
            Pens.DimGray,
            Pens.Black
        };

        private void LoadSpriteData()
        {
            spritesList.Clear();

            if (!spritesEnabled)
            {
                return;
            }

            // Determine which sprites are on this line
            for (int i = 0; i < oam.Length; i++)
            {
                int y = oam[i].CoordY - 16;
                
                if (sprites16Bit && LY >= y && LY < y + 16)
                {
                    spritesList.Add(i);
                }
                else if (LY >= y && LY < y + 8)
                {
                    spritesList.Add(i);
                }

                // Can only draw up to 10 sprites per line
                if (spritesList.Count == 10)
                {
                    break;
                }
            }

            // Determine priority order by sorting the sprites by thier x-coordinate
            for (int i = 1; i < spritesList.Count; i++)
            {
                int j = i;
                while (j > 0 && (oam[spritesList[j]].CoordX < oam[spritesList[j - 1]].CoordX))
                {
                    int tmp = spritesList[j];
                    spritesList[j] = spritesList[j - 1];
                    spritesList[--j] = tmp;
                }
            }
        }

        private void PreRender()
        {
            // Load and render background
            if (bgEnabled)
            {
                byte bgy = (byte)(SCY + LY);
                byte bgx = SCX;

                int spriteCode = bgMapSelected[bgy >> 3, bgx >> 3];
                SpriteDotData dotData = spriteCode > 0x80 ? sharedSprites[spriteCode & 0x7F] : bgDataSelected[spriteCode & 0x7F];

                for (int sx = 0; sx < ScreenPixelWidth; sx++)
                {
                    byte colorCode = dotData.GetColorCode(bgy, bgx);

                    bgPixels[sx] = colorCode;
                    renderBuf[LY, sx] = (byte)((bgPallet >> (colorCode * 2)) & 3);
                    bgx++;

                    if ((bgx & 7) == 0)
                    {
                        spriteCode = bgMapSelected[bgy >> 3, bgx >> 3];
                        dotData = spriteCode > 0x80 ? sharedSprites[spriteCode & 0x7F] : bgDataSelected[spriteCode & 0x7F];
                    }
                }
            }
            else
            {
                // When disabled, the background is set to white
                for (int sx = 0; sx < ScreenPixelWidth; sx++)
                {
                    bgPixels[sx] = 0;
                    renderBuf[LY, sx] = 0;
                }
            }

            // Load and render window
            if (windowEnabled && WY <= LY)
            {
                byte wy = (byte)(LY - WY);
                byte wx = 0;

                int spriteCode = windowMapSelected[wy >> 3, 0];
                SpriteDotData dotData = spriteCode > 0x80 ? sharedSprites[spriteCode & 0x7F] : bgDataSelected[spriteCode & 0x7F];

                for (int sx = ((WX < 7) ? 0 : WX - 7); sx < ScreenPixelWidth; sx++)
                {
                    byte colorCode = dotData.GetColorCode(wy, wx);

                    bgPixels[sx] = colorCode;
                    renderBuf[LY, sx] = (byte)((bgPallet >> (colorCode * 2)) & 3);
                    wx++;

                    if ((wx & 7) == 0)
                    {
                        spriteCode = windowMapSelected[wy >> 3, wx >> 3];
                        dotData = spriteCode > 0x80 ? sharedSprites[spriteCode & 0x7F] : bgDataSelected[spriteCode & 0x7F];
                    }
                }
            }

            // Render sprites onto background
            if (spritesEnabled)
            {
                for (int i = spritesList.Count - 1; i >= 0; i--)
                {
                    OAMRegister sprite = oam[spritesList[i]];

                    int spy = LY - (sprite.CoordY - 16); // sprite dot coordinate
                    if (sprite.VerticalFlip)
                    {
                        spy = sprites16Bit ? 15 - spy : 7 - spy;
                    }
                    
                    if (sprites16Bit && spy >= 8)
                    {
                        sprite.SpriteCode++;
                        spy -= 8;
                    }
                    SpriteDotData dotData = (sprite.SpriteCode < 0x80) ?
                        objSprites[sprite.SpriteCode] : sharedSprites[sprite.SpriteCode & 0x7F];

                    int screenx = sprite.CoordX - 8;

                    // draw the sprite
                    for (int spx = 0; spx < 8; spx++, screenx++)
                    {
                        // Ignore pixels that are off the screen
                        if (screenx < 0)
                            continue;
                        if (screenx >= ScreenPixelWidth)
                            break;

                        byte colorCode = dotData.GetColorCode(sprite.HorizontalFlip ? 7 - spx : spx, spy);

                        if (colorCode != 0) // 0 is transparent
                        {
                            // Override the pixel if the sprite has priority, or the background 
                            // color code is transparent
                            if (!sprite.BGPriority || bgPixels[screenx] == 0)
                            {
                                renderBuf[LY, screenx] = (byte)((objPallet[sprite.Pallet] >> (colorCode * 2)) & 3) ;
                            }
                        }
                    }
                }
            }
        }

        private void RenderFrame()
        {
            Bitmap frame = frameRendering == 0 ? frame0 : frame1;
            Graphics g = Graphics.FromImage(frame);

            if (lcdEnabled)
            {
                for (int y = 0; y < ScreenPixelHeight; y++)
                {
                    int lineStart = 0;
                    int colorCode = renderBuf[y, 0];

                    for (int x = 1; x < ScreenPixelWidth; x++)
                    {
                        if (colorCode != renderBuf[y, x])
                        {
                            // Draw the line and start new color
                            g.DrawLine(penColors[colorCode], lineStart, y, x - 1, y);

                            lineStart = x;
                            colorCode = renderBuf[y, x];
                        }
                    }

                    g.DrawLine(penColors[colorCode], lineStart, y, ScreenPixelWidth - 1, y);
                }
            }

            g.Dispose();

            frameRendering = frameRendering == 0 ? 1 : 0;
        }

        #endregion
    }
}
