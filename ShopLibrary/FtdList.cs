using Amicitia.IO.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopLibrary
{
    public class FtdList
    {
        public int unk0 { get; private set; }
        public uint DataSize { get; private set; }
        public uint EntryCount { get; private set; }
        public short EntryType { get; private set; }
        public short unk1 { get; private set; }

        public FtdList Read(BinaryObjectReader reader)
        {
            unk0 = reader.ReadInt32();
            DataSize = reader.ReadUInt32();
            EntryCount = reader.ReadUInt32();
            EntryType = reader.ReadInt16();
            unk1 = reader.ReadInt16();
            return this;
        }
    }
}
