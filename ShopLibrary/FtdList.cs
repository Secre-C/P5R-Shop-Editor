using Amicitia.IO.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace ShopLibrary
{
    public class FtdList
    {
        public int unk0 { get; private set; }
        public uint DataSize { get; internal set; }
        public uint EntryCount { get; internal set; }
        public short EntryType { get; private set; }
        public short unk1 { get; private set; }
        public int EntrySize { get; private set; }

        public FtdList Read(BinaryObjectReader reader)
        {
            unk0 = reader.ReadInt32();
            DataSize = reader.ReadUInt32();
            EntryCount = reader.ReadUInt32();
            EntryType = reader.ReadInt16();
            unk1 = reader.ReadInt16();
            EntrySize = (int)(DataSize / EntryCount);
            return this;
        }

        public void Write(BinaryObjectWriter writer, FtdList ftdList)
        {
            writer.Write(unk0);
            writer.Write(DataSize);
            writer.Write(EntryCount);
            writer.Write(EntryType);
            writer.Write(unk1);
        }
    }
}
