using Amicitia.IO.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ShopLibrary
{
    public class ShopItemFile
    {
        public FtdHeader FtdHeader { get; set; }
        public FtdList FtdList { get; set; }
        public List<Shop> Shops { get; set; }
        public byte[] EndOfFile { get; set; }

        public ShopItemFile Read(string shopItemFilePath)
        {
            ShopItemFile shopItemFileModel = new();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var reader = new BinaryObjectReader(shopItemFilePath, Endianness.Big, Encoding.GetEncoding(932));

            FtdHeader = new FtdHeader().Read(reader);
            FtdList = new FtdList().Read(reader);
            Shops = new List<Shop>();

            for (int i = 0; i < FtdList.EntryCount; i += Shops[Shops.Count - 1].Items.Count + 1)
            {
                Shops.Add(new Shop().Read(reader));
            }

            EndOfFile = reader.ReadArray<byte>((int)(FtdHeader.FileSize - reader.Position));

            return this;
        }

        public void Write(string newShopItemFile, ShopItemFile shopItemFileModel)
        {
            var asSpan = CollectionsMarshal.AsSpan(shopItemFileModel.Shops);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (var writer = new BinaryObjectWriter(newShopItemFile, Endianness.Big, Encoding.GetEncoding(932)))
            {
                FtdHeader.Write(writer, FtdHeader);

                FtdList.Write(writer, FtdList);

                foreach (var shop in asSpan)
                {
                    shop.Write(writer, shop);
                }

                writer.WriteArray(EndOfFile);
            }
        }

        public void AddItem(int shopId, int itemInsertIndex, int itemCopyIndex)
        {
            Shops[shopId].Items.Insert(itemInsertIndex, Shops[shopId].Items[itemCopyIndex].Clone());
            FtdList.EntryCount += 1;
            FtdList.DataSize += (uint)FtdList.EntrySize;
            FtdHeader.FileSize += (uint)FtdList.EntrySize;
        }
        public void AddItem(int shopId, int itemInsertIndex)
        {
            Shops[shopId].Items.Insert(itemInsertIndex, ShopItem.CreateDummyEntry());
            FtdList.EntryCount += 1;
            FtdList.DataSize += (uint)FtdList.EntrySize;
            FtdHeader.FileSize += (uint)FtdList.EntrySize;
        }

        public void RemoveItem(int shopId, int itemRemoveIndex)
        {
            Shops[shopId].Items.Remove(Shops[shopId].Items[itemRemoveIndex]);
            FtdList.EntryCount -= 1;
            FtdList.DataSize -= (uint)FtdList.EntrySize;
            FtdHeader.FileSize -= (uint)FtdList.EntrySize;
        }

        public void AddBlankShop(int shopInsertIndex)
        {
            var blankShop = new Shop();
            blankShop.Items = new();

            var blankItem = ShopItem.CreateDummyEntry();
            blankShop.Items.Add(blankItem);
            Shops.Insert(shopInsertIndex, blankShop);

            FtdList.EntryCount += 2;
            FtdList.DataSize += (uint)FtdList.EntrySize * 2;
            FtdHeader.FileSize += (uint)FtdList.EntrySize * 2;
        }

        public void CopyShop(int itemInsertIndex, int itemCopyIndex)
        {
            var fileSizeDiff = (uint)(FtdList.EntrySize * Shops[itemCopyIndex].Items.Count + FtdList.EntrySize);
            FtdList.EntryCount += (uint)(Shops[itemCopyIndex].Items.Count + 1);
            FtdList.DataSize += fileSizeDiff;
            FtdHeader.FileSize += fileSizeDiff;

            Shops.Insert(itemInsertIndex, Shops[itemCopyIndex]);
        }

        public void RemoveShop(int shopRemoveIndex)
        {
            var fileSizeDiff = (uint)(FtdList.EntrySize * Shops[shopRemoveIndex].Items.Count + FtdList.EntrySize);
            FtdList.EntryCount -= (uint)(Shops[shopRemoveIndex].Items.Count + 1);
            FtdList.DataSize -= fileSizeDiff;
            FtdHeader.FileSize -= fileSizeDiff;

            Shops.Remove(Shops[shopRemoveIndex]);
        }
    }
}
