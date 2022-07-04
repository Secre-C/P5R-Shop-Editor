using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO;
using Amicitia.IO.Binary;

namespace Shop_Editor
{
    public class ItemParse
    {
        public static List<string> MakeShopItemList(int shopInput, int gameVersionIndex)
        {
            string tempFile;

            if (gameVersionIndex == 0)
            {
                tempFile = MainWindow.tempShopR;
            }
            else
            {
                tempFile = MainWindow.tempShopV;
            }

            string shopItemftd = tempFile;

            List<int> shopItemCountList = FtdParse.FindShopOffsetsandCount(shopItemftd, "ItemCount");
            List<int> shopOffsets = FtdParse.FindShopOffsetsandCount(shopItemftd, "ShopOffsets");
            List<string> shopItemList = new List<string>();

            if (shopOffsets.Count < shopInput)
            {
                shopInput = shopOffsets.Count - 1;
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (BinaryObjectReader P5FTDFile = new BinaryObjectReader(shopItemftd, Endianness.Big, Encoding.GetEncoding(932)))
            {
                for (int i = 0; i < shopItemCountList[shopInput]; i++)
                {
                    int shopItemIndexOffset = i * 40;
                    long startOffset = shopOffsets[shopInput] + shopItemIndexOffset;
                    P5FTDFile.AtOffset(startOffset + 2);
                    ushort itemIndex = P5FTDFile.ReadUInt16();

                    string[] itemNames;
                    if (gameVersionIndex == 0)
                    {
                        itemNames = P5RItems.ItemLists(itemIndex);
                    }
                    else
                    {
                        itemNames = P5Items.ItemLists(itemIndex);
                    }

                    ushort itemId = itemIndex;

                    if (itemNames.Length != 1)
                    {
                        if (itemIndex >= 0x1000)
                        {
                            itemId = (ushort)(itemIndex - ((itemIndex / 0x1000) * 0x1000));
                        }
                    }
                    else
                    {
                        itemId = 0;
                    }

                    shopItemList.Add($"{itemNames[itemId]} ({i})");
                }
                return shopItemList;
            }
        }
    }
}
