using System;
using System.IO;

using GBE.Cartridges;

namespace GBE
{
    public class Cartridge
    {
        private const string StoragePath = ".\\Battery";
        private const string BatteryExtension = "batt";

        private readonly byte[] LogoData =
        {
            0xCE, 0xED, 0x66, 0x66, 0xCC, 0x0D, 0x00, 0x0B, 0x03,
            0x73, 0x00, 0x83, 0x00, 0x0C, 0x00, 0x0D, 0x00, 0x08
        };

        private string storageFile;

        private byte[] rom0;
        private MemoryController mbc;
        
        private bool hasBattery = false;

        public Cartridge(string romFile)
        {
            using (FileStream fs = new FileStream(romFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // Read in the first ROM bank, which also contains the cartridge header
                rom0 = new byte[0x4000];
                fs.Read(rom0, 0, 0x4000);

                // Check to make sure that this is an actual GameBoy ROM File by checking the logo in the header
                // (CGB only checks first 18 bytes, so that's what we will do)
                for (int i = 0, addr = 0x0104; i < LogoData.Length; i++, addr++)
                { 
                    if (addr >= rom0.Length || rom0[addr] != LogoData[i])
                    {
                        throw new FileFormatException("File is not a valid GameBoy ROM");
                    }
                }

                // Get the number of ROM and RAM banks from the cartridge header.
                int romSize = rom0[0x0148];
                switch (romSize)
                {
                    case 0x52: romSize = 0x4000 * 72; break; // Awkward sizes
                    case 0x53: romSize = 0x4000 * 80; break;
                    case 0x54: romSize = 0x4000 * 92; break;

                    default: romSize = 0x8000 << romSize; break;
                }

                int ramSize = rom0[0x0149];
                switch (ramSize) // 0 => 0, 1 => 1
                {
                    case 0: ramSize = 0; break;
                    case 1: ramSize = 0x800; break;
                    case 2: ramSize = 0x2000; break;
                    case 3: ramSize = 0x2000 * 4; break;
                    case 4: ramSize = 0x2000 * 16; break;
                    case 5: ramSize = 0x2000 * 8; break;
                }
                
                // Determine cartridge features
                
                switch (rom0[0x0147])
                {
                    default:
                        throw new NotSupportedException("Cartridge type not supported.");

                    case 9:
                        hasBattery = true;
                        goto case 8;
                    case 8:
                    case 0:
                        mbc = new ROMOnlyController(ramSize);
                        break;

                    case 3:
                        hasBattery = true;
                        goto case 2;
                    case 2: 
                    case 1:
                        mbc = new MBC1(romSize, ramSize);
                        break;

                    case 6:
                        hasBattery = true;
                        goto case 5;
                    case 5:
                        mbc = new MBC2(romSize);
                        break;

                    case 16:
                    case 15:
                        hasBattery = true;
                        mbc = new MBC3(romSize, ramSize, true);
                        break;

                    case 19:
                        hasBattery = true;
                        goto case 18;
                    case 18: 
                    case 17:
                        mbc = new MBC3(romSize, ramSize, false);
                        break;
                }

                mbc.LoadROM(fs);
            }
            
            storageFile = Path.Combine(StoragePath, Path.ChangeExtension(Path.GetFileName(romFile), BatteryExtension));

            if (hasBattery && File.Exists(storageFile))
            {
                mbc.LoadStorageRAM(storageFile);
            }
        }
        
        public byte ReadRAM(int address)
        {
            return mbc.ReadFromRAM(address);
        }

        public void WriteRAM(int address, byte value)
        {
            mbc.WriteToRAM(address, value);
        }

        public byte ReadROM(int address)
        {
            if (address < 0x4000)
            {
                return rom0[address];
            }
            else if (address < 0x8000)
            {
                return mbc.ReadFromROM(address);
            }
            return 0;
        }

        public void WriteToRegister(int address, byte value)
        {
            mbc.WriteToRegister(address, value);
        }

        public void Save()
        {
            if (hasBattery)
            {
                Directory.CreateDirectory(StoragePath);
                mbc.StoreRAMData(storageFile);
            }
        }
    }
}
