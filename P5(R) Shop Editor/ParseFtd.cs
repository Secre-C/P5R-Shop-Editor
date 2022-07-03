using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Amicitia.IO.Binary;
using Amicitia.IO;

namespace Shop_Editor
{
    internal class FtdParse
    {
        public static uint FtdRead(string shopftd)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (BinaryObjectReader P5FTDFile = new BinaryObjectReader(shopftd, Endianness.Big, Encoding.GetEncoding(932)))
            {
                FileInfo arg0 = new FileInfo(shopftd);
                int Field0 = P5FTDFile.ReadInt32();
                int Magic = P5FTDFile.ReadInt32();
                uint FileSize = P5FTDFile.ReadUInt32();
                short DataType = P5FTDFile.ReadInt16();
                short DataCount = P5FTDFile.ReadInt16();
                int[] DataOffsets;
                DataOffsets = new int[DataCount];
                for (int j = 0; j < DataCount; j++)
                {
                    DataOffsets[0] = P5FTDFile.ReadInt32();
                }
                P5FTDFile.ReadInt32();
                P5FTDFile.ReadInt32();
                P5FTDFile.ReadInt32();
                int Field1 = P5FTDFile.ReadInt32();
                uint DataSize = P5FTDFile.ReadUInt32();
                uint EntryCount = P5FTDFile.ReadUInt32();
                short EntryType = P5FTDFile.ReadInt16();
                short Field2 = P5FTDFile.ReadInt16();
                return EntryCount;
            }
        }

        public static List<int> FindShopOffsetsandCount(string shopftd, string returnParam)
        {
            uint entryCount = FtdRead(shopftd);
            short shopCount = 0;
            List<int> shopCountList = new List<int>(); //just a way to return how many shops there are 
            List<int> shopOffsets = new List<int>(); //offsets for the start of each shop
            List<int> shopItemCount = new List<int>(); //list with the amount of items in each shop

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (BinaryObjectReader P5FTDFile = new BinaryObjectReader(shopftd, Endianness.Big, Encoding.GetEncoding(932)))
            {
                int currentOffset = 48;
                int countItems = 0;
                for (int j = 0; j < entryCount; j++)
                {
                    byte value = P5FTDFile.ReadValueAtOffset<byte>(currentOffset);
                    if (value == 157)
                    {
                        shopOffsets.Add(currentOffset - (countItems * 40));
                        shopItemCount.Add(countItems);
                        countItems = 0;
                        shopCount++;
                    }
                    else
                    {
                        countItems++;
                    }

                    currentOffset += 40;
                }
                shopCount -= 1;
            }
            if (returnParam == "ItemCount")
            {
                return shopItemCount;
            }
            else if (returnParam == "ShopOffsets")
            {
                return shopOffsets;
            }
            shopCountList.Add(shopCount);
            return shopCountList;
        }

        public static int SumRoyalFlag(short section, short bitflag)
        {
            switch (section)
            {
                case 0x1000:
                    bitflag += 3072;
                    break;

                case 0x2000:
                    bitflag += 6144;
                    break;

                case 0x3000:
                    bitflag += 11264;
                    break;

                case 0x4000:
                    bitflag += 11776;
                    break;

                case 0x5000:
                    bitflag += 12288;
                    break;
            }
            return bitflag;
        }
    }
}