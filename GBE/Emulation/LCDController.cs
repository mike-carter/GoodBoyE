using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

using GBE.Emulation.GraphicsData;

namespace GBE.Emulation
{
    class LCDController
    {
        public const int ScreenPixelWidth = 160;
        public const int ScreenPixelHeight = 144;
        
        #region Instance Variables
        
        private int lcdClock; // Used to time graphic update events
        
        // Graphics Virtual Memory
        private CharData[] objChars = new CharData[0x80];
        private CharData[] bgChars = new CharData[0x80];
        private CharData[] sharedChars = new CharData[0x80];

        private byte[,] bgMap0 = new byte[32, 32];
        private byte[,] bgMap1 = new byte[32, 32];

        private OAMRegister[] oam = new OAMRegister[40];

        // GPU Registers

        // LCDC (LCD Control) Register Flags
        private bool bgEnabled;
        private bool spritesEnabled;
        private bool sprites16Bit;
        private byte[,] bgMapSelected;
        private CharData[] bgDataSelected;
        private bool windowEnabled;
        private byte[,] windowMapSelected;
        private bool lcdEnabled;

        // STAT (LCD Status) Register Flags
        private enum GPUMode
        {
            HBlank = 0, VBlank = 1, OAMRead = 2, VRAMRead = 3
        }
        GPUMode lcdMode;

        [Flags]
        private enum GPUInterrupts
        {
            OnHBlank = 0x08, OnVBlank = 0x10, OnOAMRead = 0x20, OnMatch = 0x40,
            All = OnHBlank | OnVBlank | OnOAMRead | OnMatch
        }
        GPUInterrupts enabledInterrupts;

        private bool matchFlag;

        // Other Registers
        public byte SCY, SCX; // Background scroll registers

        private byte LY; // Line being drawn (Read only)
        public byte LYC; // Line to raise Match interrupt

        public byte WY, WX; // Window position registers

        // Pallets
        public byte BGPallet;
        private byte[] objPallet = new byte[2];

        // Frame double buffering
        private Bitmap frame0 = new Bitmap(ScreenPixelWidth, ScreenPixelHeight);
        private Bitmap frame1 = new Bitmap(ScreenPixelWidth, ScreenPixelHeight);
        private int frameRendering = 0;

        // Interrupt Events
        public event Action VBlankInterrupt;
        public event Action LCDCInterrupt;

        // Background rendering task
        private Task renderTask;

        #endregion Instance Variables

        #region Memory Access

        /// <summary>
        /// Gets/Sets the value of the LCD Control register. 
        /// </summary>
        public byte LCDCReg
        {
            get
            {
                return (byte)((bgEnabled ? 1 : 0) |
                    (spritesEnabled ? 2 : 0) |
                    (sprites16Bit ? 4 : 0) |
                    (bgMapSelected == bgMap0 ? 0 : 8) |
                    (bgDataSelected == bgChars ? 0 : 0x10) |
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
                bgDataSelected = (value & 0x10) == 0 ? bgChars : objChars;
                windowEnabled = (value & 0x20) != 0;
                windowMapSelected = (value & 0x40) == 0 ? bgMap0 : bgMap1;
                EnableLCD((value & 0x80) != 0);
            }
        }

        public byte STATReg
        {
            get
            {
                return (byte)((int)lcdMode | (matchFlag ? 0x04 : 0) | (int)enabledInterrupts);
            }
            set
            {
                // Interrupts are the only bits that are writable
                enabledInterrupts = (GPUInterrupts)value & GPUInterrupts.All;
            }
        }

        public byte LCDYCoord
        {
            get { return LY; }
        }

        public byte[] ObjPallets
        {
            get { return objPallet; }
        }

        public byte ReadVRAM(int address)
        {
            // Access blocked during mode 3
            if (!lcdEnabled || lcdMode != GPUMode.VRAMRead)
            {
                address &= 0x1FFF;
                if (address < 0x800) // Object Character data
                {
                    return objChars[(address >> 4) & 0x7F].ReadByte(address & 15);
                }
                else if (address < 0x1000) // BG and OBJ Shared character data
                {
                    return sharedChars[(address >> 4) & 0x7F].ReadByte(address & 15);
                }
                else if (address < 0x1800) // BG only character data
                {
                    return bgChars[(address >> 4) & 0x7F].ReadByte(address & 15);
                }
                else if (address < 0x1C00) // BG Map 0
                {
                    return bgMap0[(address >> 5) & 0x1F, address & 0x1F];
                }
                else // BG Map 1
                {
                    return bgMap1[(address >> 5) & 0x1F, address & 0x1F];
                }
            }
            return 0;
        }

        public void WriteVRAM(int address, byte value)
        {
            // Access blocked during mode 3
            if (!lcdEnabled || lcdMode != GPUMode.VRAMRead)
            {
                address &= 0x1FFF;
                if (address < 0x800) // Object Character data
                {
                    objChars[(address >> 4) & 0x7F].WriteByte(address & 15, value);
                }
                else if (address < 0x1000) // BG and OBJ Shared character data
                {
                    sharedChars[(address >> 4) & 0x7F].WriteByte(address & 15, value);
                }
                else if (address < 0x1800) // BG only character data
                {
                    bgChars[(address >> 4) & 0x7F].WriteByte(address & 15, value);
                }
                else if (address < 0x1C00) // BG Map 0
                {
                    bgMap0[(address >> 5) & 0x1F, address & 0x1F] = value;
                }
                else // BG Map 1
                {
                    bgMap1[(address >> 5) & 0x1F, address & 0x1F] = value;
                }
            }
        }

        public byte ReadOAM(int address)
        {
            // Access to OAM is denied during modes 2 and 3
            if (!lcdEnabled || (lcdMode != GPUMode.OAMRead && lcdMode != GPUMode.VRAMRead))
            {
                address &= 0xFF;
                if (address >= 0xA0) // Sanity check
                {
                    return 0;
                }

                switch (address & 3)
                {
                    case 0: return oam[address >> 2].CoordY;
                    case 1: return oam[address >> 2].CoordX;
                    case 2: return oam[address >> 2].SpriteCode;
                    case 3: return oam[address >> 2].Attributes;
                }
            }
            return 0;
        }

        public void WriteOAM(int address, byte value)
        {
            if (!lcdEnabled || (lcdMode != GPUMode.OAMRead && lcdMode != GPUMode.VRAMRead))
            {
                address &= 0xFF;
                if (address >= 0xA0) // Sanity check
                {
                    return;
                }

                switch (address & 3)
                {
                    case 0: oam[address >> 2].CoordY = value; break;
                    case 1: oam[address >> 2].CoordX = value; break;
                    case 2: oam[address >> 2].SpriteCode = value; break;
                    case 3: oam[address >> 2].Attributes = value; break;
                }
            }
        }

        #endregion
        
        /// <summary>
        /// Resets the LCD 
        /// </summary>
        public void PowerOn()
        {
            // Clear registers.
            LCDCReg = 0;
            STATReg = 0;
            SCY = 0;
            SCX = 0;
            LY = 0;
            LYC = 0;
            WX = 0;
            WY = 0;
            BGPallet = 0;
            objPallet[0] = 0;
            objPallet[1] = 0;

            // Clear one of the frames
            using (Graphics g = Graphics.FromImage(frame0))
            {
                g.FillRectangle(Brushes.White, 0, 0, ScreenPixelWidth, ScreenPixelHeight);
            }
            frameRendering = 1;
        }

        /// <summary>
        /// Gets the frame image to display.
        /// </summary>
        /// <returns></returns>
        public Image GetFrame()
        {
            if (frameRendering == 0)
            {
                return frame1;
            }
            return frame0;
        }

        /// <summary>
        /// While the LCD is enabled, advances the LCD clock by the specified number of clicks, and performs 
        /// graphics events based on the current clock value.
        /// </summary>
        /// <param name="ticks"></param>
        public void ClockTick(int ticks)
        {
            if (lcdEnabled)
            {
                lcdClock += ticks;

                switch (lcdMode)
                {
                    case GPUMode.HBlank:
                        RunHBlank();
                        break;

                    case GPUMode.VBlank:
                        RunVBlank();
                        break;

                    case GPUMode.OAMRead:
                        RunOAMRead();
                        break;

                    case GPUMode.VRAMRead:
                        RunVRAMRead();
                        break;
                }
            }
        }
        
        #region Private Methods

        private void EnableLCD(bool enable)
        {
            if (lcdEnabled && !enable)
            {
                // Disable the LCD
                lcdEnabled = false;
                lcdClock = 0;
                LY = 0;
            }
            else if (!lcdEnabled && enable)
            {
                // Enable the LCD
                lcdEnabled = true;
                lcdMode = GPUMode.OAMRead;
            }
        }

        private void RunOAMRead()
        {
            if (lcdClock >= 20)
            {
                renderTask?.Wait(); // Wait for sprite load to finish
                lcdClock -= 20;

                // Pre-render the line
                lcdMode = GPUMode.VRAMRead;
                renderTask = Task.Run(() => PreRender());
            }
        }

        private void RunVRAMRead()
        {
            if (lcdClock >= 43)
            {
                renderTask.Wait(); // Wait for pre-rendering to finish
                lcdClock -= 43;

                // Switch to HBlank mode
                lcdMode = GPUMode.HBlank;
                if ((enabledInterrupts & GPUInterrupts.OnHBlank) != 0)
                {
                    LCDCInterrupt?.Invoke();
                }
                // No task is run during HBlank
            }
        }

        private void RunHBlank()
        {
            if (lcdClock >= 51) // Horizontal Blank lasts for ~51 m-clock ticks
            {
                lcdClock -= 51;

                LY++; // Move to next line

                // If LY == LYC, set the flag and raise interrupt if it's enabled
                matchFlag = LY == LYC;
                if (matchFlag && (enabledInterrupts & GPUInterrupts.OnMatch) != 0)
                {
                    LCDCInterrupt?.Invoke();
                }

                if (LY == 144) // Switch to vertical blank mode
                {
                    // Begin rendering the frame
                    renderTask = Task.Run(() => RenderFrame());

                    lcdMode = GPUMode.VBlank;

                    // signal vertical blank interrupt and LCD interrupt (if enabled)
                    VBlankInterrupt?.Invoke();
                    if ((enabledInterrupts & GPUInterrupts.OnVBlank) != 0)
                    {
                        LCDCInterrupt?.Invoke();
                    }
                }
                else // Start rendering the next line
                {
                    lcdMode = GPUMode.OAMRead;
                    if ((enabledInterrupts & GPUInterrupts.OnOAMRead) != 0)
                    {
                        LCDCInterrupt?.Invoke();
                    }
                    renderTask = Task.Run(() => LoadSpriteData());
                }
            }
        }

        private void RunVBlank()
        {
            if (lcdClock >= 144)
            {
                lcdClock -= 144;

                LY++; // Move to next line

                // If LY == LYC, set the flag and raise interrupt if it's enabled
                matchFlag = LY == LYC;
                if (matchFlag && (enabledInterrupts & GPUInterrupts.OnMatch) != 0)
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

                    lcdMode = GPUMode.OAMRead;
                    if ((enabledInterrupts & GPUInterrupts.OnOAMRead) != 0)
                    {
                        LCDCInterrupt?.Invoke();
                    }
                    renderTask = Task.Run(() => LoadSpriteData());
                }
            }
        }

        #endregion Private Methods

        #region Video Rendering

        private byte[,] dotMatrix = new byte[ScreenPixelHeight, ScreenPixelWidth];
        private List<int> spritesList = new List<int>(10);
        private byte[] bgPixels = new byte[ScreenPixelWidth];

        // Pens quick lookup
        private static readonly Pen[] penColors = new Pen[]
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
                CharData dotData = spriteCode >= 0x80 ? sharedChars[spriteCode & 0x7F] : bgDataSelected[spriteCode & 0x7F];
                

                for (int sx = 0; sx < ScreenPixelWidth; sx++)
                {
                    byte colorCode = dotData.GetPixelShade(bgy, bgx);

                    bgPixels[sx] = colorCode;
                    dotMatrix[LY, sx] = (byte)((BGPallet >> (colorCode * 2)) & 3);
                    bgx++;

                    if ((bgx & 7) == 0)
                    {
                        spriteCode = bgMapSelected[bgy >> 3, bgx >> 3];
                        dotData = spriteCode >= 0x80 ? sharedChars[spriteCode & 0x7F] : bgDataSelected[spriteCode & 0x7F];
                    }
                }
            }
            else
            {
                // When disabled, the background is set to white
                for (int sx = 0; sx < ScreenPixelWidth; sx++)
                {
                    bgPixels[sx] = 0;
                    dotMatrix[LY, sx] = 0;
                }
            }

            // Load and render window
            if (windowEnabled && WY <= LY)
            {
                byte wy = (byte)(LY - WY);
                byte wx = 0;

                int spriteCode = windowMapSelected[wy >> 3, 0];
                CharData dotData = spriteCode >= 0x80 ? sharedChars[spriteCode & 0x7F] : bgDataSelected[spriteCode & 0x7F];

                for (int sx = ((WX < 7) ? 0 : WX - 7); sx < ScreenPixelWidth; sx++)
                {
                    byte colorCode = dotData.GetPixelShade(wy, wx);

                    bgPixels[sx] = colorCode;
                    dotMatrix[LY, sx] = (byte)((BGPallet >> (colorCode * 2)) & 3);
                    wx++;

                    if ((wx & 7) == 0)
                    {
                        spriteCode = windowMapSelected[wy >> 3, wx >> 3];
                        dotData = spriteCode >= 0x80 ? sharedChars[spriteCode & 0x7F] : bgDataSelected[spriteCode & 0x7F];
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
                    CharData dotData = (sprite.SpriteCode < 0x80) ?
                        objChars[sprite.SpriteCode] : sharedChars[sprite.SpriteCode & 0x7F];

                    int screenx = sprite.CoordX - 8;

                    // draw the sprite
                    for (int spx = 0; spx < 8; spx++, screenx++)
                    {
                        // Ignore pixels that are off the screen
                        if (screenx < 0)
                            continue;
                        if (screenx >= ScreenPixelWidth)
                            break;

                        byte colorCode = dotData.GetPixelShade(spy, sprite.HorizontalFlip ? 7 - spx : spx);

                        if (colorCode != 0) // 0 is transparent
                        {
                            // Override the pixel if the sprite has priority, or the background 
                            // color code is transparent
                            if (!sprite.BGPriority || bgPixels[screenx] == 0)
                            {
                                dotMatrix[LY, screenx] = (byte)((objPallet[sprite.Pallet] >> (colorCode * 2)) & 3) ;
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
                    int colorCode = dotMatrix[y, 0];

                    for (int x = 1; x < ScreenPixelWidth; x++)
                    {
                        if (colorCode != dotMatrix[y, x])
                        {
                            // Draw the line and start new color
                            g.DrawLine(penColors[colorCode], lineStart, y, x, y);

                            lineStart = x;
                            colorCode = dotMatrix[y, x];
                        }
                    }

                    g.DrawLine(penColors[colorCode], lineStart, y, ScreenPixelWidth - 1, y);
                }
            }

            g.Dispose();

            frameRendering = frameRendering == 0 ? 1 : 0;
        }

        #endregion Video Rendering
    }
}
