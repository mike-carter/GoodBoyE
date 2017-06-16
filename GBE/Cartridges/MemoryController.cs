using System;
using System.IO;
using System.Xml;

namespace GBE.Cartridges
{
    internal abstract class MemoryController : IDisposable
    {
        protected byte[][] ROMBanks;
        protected int CurrentROMBank;

        protected byte[][] RAMBanks;
        protected int CurrentRAMBank;
        protected bool RAMEnabled;

        protected MemoryController(int romSize, int ramSize)
        {
            ROMBanks = new byte[romSize / 0x4000][];

            // ROM[0] is accessed directly by the cartridge
            for (int i = 1; i < ROMBanks.Length; i++)
            {
                ROMBanks[i] = new byte[0x4000];
            }
            
            if (ramSize > 0)
            {
                if (ramSize < 0x2000)
                {
                    RAMBanks = new byte[1][] { new byte[ramSize] };
                }
                else
                {
                    RAMBanks = new byte[ramSize / 0x2000][];
                    for (int i = 0; i < RAMBanks.Length; i++)
                    {
                        RAMBanks[i] = new byte[0x2000];
                    }
                }
            }
            
            CurrentROMBank = 1;
            CurrentRAMBank = 0;
        }

        /// <summary>
        /// Loads the ROM data from the given stream. The stream's position
        /// is assumed to be the first switchable ROM bank (bank 1)
        /// </summary>
        /// <param name="stream"></param>
        public virtual void LoadROM(Stream stream)
        {
            for (int i = 1; i < ROMBanks.Length; i++)
            {
                stream.Read(ROMBanks[i], 0, 0x4000);
            }
        }

        /// <summary>
        /// Loads RAM from a battery storage file.
        /// </summary>
        /// <param name="reader"></param>
        public void LoadStorageRAM(string fileName)
        {
            using (XmlReader reader = XmlReader.Create(fileName))
            {
                if (reader.IsStartElement("cartridge"))
                {
                    reader.Read();
                    ReadXML(reader);
                }
            }
        }
        

        protected virtual void ReadXML(XmlReader reader)
        {
            if (reader.IsStartElement("savedata"))
            {
                reader.Read();

                while (reader.IsStartElement("bank"))
                {
                    string indexAttr = reader.GetAttribute("index");
                    int i;

                    reader.ReadStartElement();
                    if (int.TryParse(indexAttr, out i))
                    {
                        reader.ReadContentAsBase64(RAMBanks[i], 0, RAMBanks[i].Length);
                    }
                    reader.Read();
                }
            }
        }

        /// <summary>
        /// Saves 
        /// </summary>
        /// <param name="writer"></param>
        public void StoreRAMData(string fileName)
        {
            using (XmlWriter writer = XmlWriter.Create(fileName))
            {
                writer.WriteStartDocument();

                writer.WriteStartElement("cartridge");

                WriteXML(writer);

                writer.WriteEndElement();
            }
        }

        protected virtual void WriteXML(XmlWriter writer)
        {
            writer.WriteStartElement("savedata");
            
            for (int i = 0; i < RAMBanks.Length; i++)
            {
                writer.WriteStartElement("bank");
                writer.WriteAttributeString("index", i.ToString());
                writer.WriteBase64(RAMBanks[i], 0, RAMBanks[i].Length);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        public virtual void PowerOn()
        {
            CurrentROMBank = 1;
            CurrentRAMBank = 0;
            RAMEnabled = false;
        }


        public abstract void WriteToRegister(int address, byte value);


        public byte ReadFromROM(int address)
        {
            return ROMBanks[CurrentROMBank][address & 0x3FFF];
        }
        

        public virtual byte ReadFromRAM(int address)
        {
            if (RAMEnabled && RAMBanks != null)
            {
                address &= 0x1FFF;
                if (address < RAMBanks[CurrentRAMBank].Length)
                {
                    return RAMBanks[CurrentRAMBank][address];
                }
            }
            return 0;
        }


        public virtual void WriteToRAM(int address, byte value)
        {
            if (RAMEnabled && RAMBanks != null)
            {
                address &= 0x1FFF;
                if (address < RAMBanks[CurrentRAMBank].Length)
                {
                    RAMBanks[CurrentRAMBank][address] = value;
                }
            }
        }
        

        public virtual void Dispose()
        {
            ROMBanks = null;
            RAMBanks = null;
        }
    }
}
