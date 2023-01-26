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
        public byte Quantity { get; set; }
        public byte Quantity2 { get; set; } //often matches quantity, not sure how it works
        public byte Quantity3 { get; set; } //often matches quantity, not sure how it works
        public byte unk1 { get; private set; }
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
            ItemId.data = reader.ReadUInt16();
            AmountPerUnit = reader.ReadByte();
            AvailabilityStartMonth = reader.ReadByte();
            AvailabilityStartDay = reader.ReadByte();
            AvailabilityEndMonth = reader.ReadByte();
            AvailabilityEndDay = reader.ReadByte();
            Quantity = reader.ReadByte();
            Quantity2 = reader.ReadByte();
            Quantity3 = reader.ReadByte();
            unk1 = reader.ReadByte();
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
        public void Write(BinaryObjectWriter writer, ShopItem shopItem)
        {
            writer.Write(shopItem.ShopEndIndicator); 
            writer.Write(shopItem.unk0);
            writer.Write(shopItem.ItemId.data); 
            writer.Write(shopItem.AmountPerUnit); 
            writer.Write(shopItem.AvailabilityStartMonth); 
            writer.Write(shopItem.AvailabilityStartDay); 
            writer.Write(shopItem.AvailabilityEndMonth); 
            writer.Write(shopItem.AvailabilityEndDay); 
            writer.Write(shopItem.Quantity); 
            writer.Write(shopItem.Quantity2); 
            writer.Write(shopItem.Quantity3);
            writer.Write(shopItem.unk1);
            writer.Write(shopItem.unk2); 
            writer.Write(shopItem.unk3); 
            writer.Write(shopItem.unk4); 
            writer.Write(shopItem.unk5); 
            writer.Write(shopItem.Bitflag.ConvertedBitflag); 
            writer.Write(shopItem.unk6); 
            writer.Write(shopItem.PercentageOfPrice); 
            writer.Write(shopItem.unk7); 
        }

        internal static ShopItem CreateEndEntry()
        {
            ShopItem endShopEntry = new();

            endShopEntry.ShopEndIndicator = 0x9D;
            endShopEntry.unk0 = 0;
            endShopEntry.ItemId = new Item();
            endShopEntry.ItemId.data = 0;
            endShopEntry.AmountPerUnit = 0;
            endShopEntry.AvailabilityStartMonth = 0;
            endShopEntry.AvailabilityStartDay = 0;
            endShopEntry.AvailabilityEndMonth = 0;
            endShopEntry.AvailabilityEndDay = 0;
            endShopEntry.unk1 = 0;
            endShopEntry.Quantity = 0;
            endShopEntry.Quantity2 = 0;
            endShopEntry.Quantity3 = 0;
            endShopEntry.unk2 = 0;
            endShopEntry.unk3 = 0;
            endShopEntry.unk4 = 0;
            endShopEntry.unk5 = 0;
            endShopEntry.Bitflag = new Bitflag();
            endShopEntry.Bitflag.data = 0;
            endShopEntry.unk6 = 0;
            endShopEntry.PercentageOfPrice = 0;
            endShopEntry.unk7 = 0;

            return endShopEntry;
        }

        public static ShopItem CreateDummyEntry()
        {
            var dummyShopEntry = new ShopItem();
            dummyShopEntry.ShopEndIndicator = 0xFF;
            dummyShopEntry.unk0 = 0;
            dummyShopEntry.ItemId = new Item();
            dummyShopEntry.ItemId.data = 0x3001;
            dummyShopEntry.AmountPerUnit = 1;
            dummyShopEntry.AvailabilityStartMonth = 4;
            dummyShopEntry.AvailabilityStartDay = 1;
            dummyShopEntry.AvailabilityEndMonth = 3;
            dummyShopEntry.AvailabilityEndDay = 31;
            dummyShopEntry.Quantity = 99;
            dummyShopEntry.Quantity2 = 99;
            dummyShopEntry.Quantity3 = 99;
            dummyShopEntry.unk1 = 0xFF;
            dummyShopEntry.unk2 = 0xFF;
            dummyShopEntry.unk3 = 0;
            dummyShopEntry.unk4 = 0;
            dummyShopEntry.unk5 = 0;
            dummyShopEntry.Bitflag = new Bitflag();
            dummyShopEntry.Bitflag.data = -1;
            dummyShopEntry.unk6 = 0;
            dummyShopEntry.PercentageOfPrice = 100;
            dummyShopEntry.unk7 = 0;

            return dummyShopEntry;
        }

        public ShopItem Clone()
        {
            ShopItem itemClone = (ShopItem)MemberwiseClone();

            itemClone.ItemId = new Item();
            itemClone.ItemId.data = ItemId.data;

            itemClone.Bitflag = new Bitflag();
            itemClone.Bitflag.data = Bitflag.data;

            return itemClone;
        }
    }

    public class Item
    {
        internal ushort data { get; set; }

        public ushort ItemCategory
        {
            get { return (ushort)(data >> 12); }
            set { data = (ushort)(value << 12); }
        }

        public ushort ItemIndex
        {
            get { return (ushort)(data & 0xFFF); }
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
            set { data = value; }
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
