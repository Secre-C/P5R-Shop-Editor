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
        public List<int> ShopOffsets { get; set; }
        public List<Shop> Shops { get; set; }
        public byte[] EndOfFile { get; set; }

        public void Read(string shopItemFilePath, out ShopItemFile shopItemFileModel)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var reader = new BinaryObjectReader(shopItemFilePath, Endianness.Big, Encoding.GetEncoding(932));

            FtdHeader = new FtdHeader().Read(reader);
            FtdList = new FtdList().Read(reader);
            Shops = new List<Shop>();

            for (int i = 0; i < FtdList.EntryCount; i += Shops[Shops.Count - 1].Items.Count)
            {
                Shops.Add(new Shop().Read(reader));
            }

            EndOfFile = reader.ReadArray<byte>((int)(FtdHeader.FileSize - reader.Position));

            shopItemFileModel = this;
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

        public void AddItem(ref ShopItemFile shopItemFileModel, int shopId, int itemInsertIndex, int itemCopyIndex)
        {
            shopItemFileModel.Shops[shopId].Items.Insert(itemInsertIndex, shopItemFileModel.Shops[shopId].Items[itemCopyIndex]);
            FtdList.EntryCount += 1;
            FtdList.DataSize += (uint)FtdList.EntrySize;
            FtdHeader.FileSize += (uint)FtdList.EntrySize;

            shopItemFileModel = this;
        }

        public void RemoveItem(ref ShopItemFile shopItemFileModel, int shopId, int itemRemoveIndex)
        {
            shopItemFileModel.Shops[shopId].Items.Remove(shopItemFileModel.Shops[shopId].Items[itemRemoveIndex]);
            FtdList.EntryCount -= 1;
            FtdList.DataSize -= (uint)FtdList.EntrySize;
            FtdHeader.FileSize -= (uint)FtdList.EntrySize;

            shopItemFileModel = this;
        }

        public void AddBlankShop(ref ShopItemFile shopItemFileModel, int shopInsertIndex)
        {
            var blankShop = new Shop();
            blankShop.Items = new();
            blankShop.CreateEndEntry(out var blankEntry);
            blankShop.Items.Add(blankEntry);
            shopItemFileModel.Shops.Insert(shopInsertIndex, blankShop);

            FtdList.EntryCount += 1;
            FtdList.DataSize += (uint)FtdList.EntrySize;
            FtdHeader.FileSize += (uint)FtdList.EntrySize;

            shopItemFileModel = this;
        }

        public void CopyShop(ref ShopItemFile shopItemFileModel, int itemInsertIndex, int itemCopyIndex)
        {
            var fileSizeDiff = (uint)(FtdList.EntrySize * shopItemFileModel.Shops[itemCopyIndex].Items.Count);
            FtdList.EntryCount += (uint)shopItemFileModel.Shops[itemCopyIndex].Items.Count;
            FtdList.DataSize += fileSizeDiff;
            FtdHeader.FileSize += fileSizeDiff;

            shopItemFileModel.Shops.Insert(itemInsertIndex, shopItemFileModel.Shops[itemCopyIndex]);

            shopItemFileModel = this;
        }

        public void RemoveShop(ref ShopItemFile shopItemFileModel, int shopRemoveIndex)
        {
            var fileSizeDiff = (uint)(FtdList.EntrySize * shopItemFileModel.Shops[shopRemoveIndex].Items.Count);
            FtdList.EntryCount -= (uint)shopItemFileModel.Shops[shopRemoveIndex].Items.Count;
            FtdList.DataSize -= fileSizeDiff;
            FtdHeader.FileSize -= fileSizeDiff;

            shopItemFileModel.Shops.Remove(shopItemFileModel.Shops[shopRemoveIndex]);

            shopItemFileModel = this;
        }
    }
}
