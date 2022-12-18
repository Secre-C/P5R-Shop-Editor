using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO;
using Amicitia.IO.Binary;

namespace ShopLibrary
{
    public class ShopItem
    {
        public byte ShopEndIndicator { get; set; }
        public byte unk0 { get; private set; }
        public Item ItemId { get; set; }
        public byte AmountPerUnit { get; set; } //the amount of items you get per unit purchased
        public byte AvailabilityStartMonth { get; set; }
        public byte AvailabilityStartDay { get; set; }
        public byte AvailabilityEndMonth { get; set; }
        public byte AvailabilityEndDay { get; set; }
        public byte unk1 { get; private set; }
        public byte Quantity { get; set; }
        public byte Quantity2 { get; set; } //often matches quantity, not sure how it works
        public byte Quantity3 { get; set; } //often matches quantity, not sure how it works
        public byte unk2 { get; private set; }
        public short unk3 { get; private set; }
        public int unk4 { get; private set; }
        public int unk5 { get; private set; }
        public Bitflag Bitflag { get; set; }
        public int unk6 { get; private set; }
        public int PercentageOfPrice { get; set; }
        public int unk7 { get; private set; }

        public ShopItem Read(BinaryObjectReader reader)
        {
            ShopEndIndicator = reader.ReadByte();
            unk0 = reader.ReadByte();
            ItemId = new Item();
            ItemId.data = reader.ReadInt16();
            AmountPerUnit = reader.ReadByte();
            AvailabilityStartMonth = reader.ReadByte();
            AvailabilityStartDay = reader.ReadByte();
            AvailabilityEndMonth = reader.ReadByte();
            AvailabilityEndDay = reader.ReadByte();
            unk1 = reader.ReadByte();
            Quantity = reader.ReadByte();
            Quantity2 = reader.ReadByte();
            Quantity3 = reader.ReadByte();
            unk2 = reader.ReadByte();
            unk3 = reader.ReadInt16();
            unk4 = reader.ReadInt32();
            unk5 = reader.ReadInt32();
            Bitflag = new Bitflag();
            Bitflag.data = reader.ReadInt32();
            unk6 = reader.ReadInt32();
            PercentageOfPrice = reader.ReadInt32();
            unk7 = reader.ReadInt32();

            return this;
        }
        public void write(BinaryObjectWriter writer)
        {

        }

        public ShopItem GenerateEndEntry()
        {
            ShopEndIndicator = 0x9D;
            unk0 = 0;
            ItemId = new Item();
            ItemId.data = 0;
            AmountPerUnit = 0;
            AvailabilityStartMonth = 0;
            AvailabilityStartDay = 0;
            AvailabilityEndMonth = 0;
            AvailabilityEndDay = 0;
            unk1 = 0;
            Quantity = 0;
            Quantity2 = 0;
            Quantity3 = 0;
            unk2 = 0;
            unk3 = 0;
            unk4 = 0;
            unk5 = 0;
            Bitflag = new Bitflag();
            Bitflag.data = 0;
            unk6 = 0;
            PercentageOfPrice = 0;
            unk7 = 0;

            return this;
        }
    }
    public class Item
    {
        internal short data { get; set; }

        public short ItemCategory
        {
            get { return (short)(data >> 12); }
            set { data = (short)(value << 12); }
        }

        public short ItemIndex
        {
            get { return (short)(data & 0xFFF); }
            set { data += value; }
        }
    }

    public class Bitflag
    {
        internal int data { get; set; }
        public int BitflagCategory
        {
            get { return data >> 28; }
            set { data = value << 28; }
        }
        public short BitflagIndex
        {
            get { return (short)(data & 0xFFFF); }
            set { data += value; }
        }
        public int ConvertedBitflag
        {
            get { return SumRoyalFlag(this); }
            set { data += value; }
        }

        public short SumRoyalFlag(Bitflag bitflag)
        {
            short newBitflag = (short)bitflag.BitflagIndex;
            switch (bitflag.BitflagCategory)
            {
                case 1:
                    newBitflag += 3072;
                    break;

                case 2:
                    newBitflag += 6144;
                    break;

                case 3:
                    newBitflag += 11264;
                    break;

                case 4:
                    newBitflag += 11776;
                    break;

                case 5:
                    newBitflag += 12288;
                    break;
            }
            return newBitflag;
        }
    }
}
