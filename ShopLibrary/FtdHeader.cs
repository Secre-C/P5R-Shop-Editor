using Amicitia.IO.Binary;

namespace ShopLibrary
{
    public class FtdHeader
    {
        public int Field00 { get; private set; }
        public int Magic { get; private set; }
        public uint FileSize { get; internal set; }
        public short DataType { get; private set; }
        public short DataCount{ get; private set; }
        public int[] DataOffsets { get; private set; }
        public int unk0 { get; private set; }
        public int unk1 { get; private set; }
        public int unk2 { get; private set; }

        public FtdHeader Read(BinaryObjectReader reader)
        {
            Field00 = reader.ReadInt32();
            Magic = reader.ReadInt32();
            FileSize = reader.ReadUInt32();
            DataType = reader.ReadInt16();
            DataCount = reader.ReadInt16();
            DataOffsets = reader.ReadArray<int>(DataCount);
            unk0 = reader.ReadInt32();
            unk1 = reader.ReadInt32();
            unk2 = reader.ReadInt32();
            return this;
        }

        public void Write(BinaryObjectWriter writer, FtdHeader ftdHeader)
        {
            writer.Write(Field00);
            writer.Write(Magic);
            writer.Write(FileSize);
            writer.Write(DataType);
            writer.Write(DataCount);
            writer.WriteArray(DataOffsets);
            writer.Write(unk0);
            writer.Write(unk1);
            writer.Write(unk2);
        }
    }
}