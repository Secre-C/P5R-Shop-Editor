using Amicitia.IO.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ShopLibrary
{
    public class ShopDataFile
    {
        public FtdHeader FtdHeader { get; set; }
        public FtdList FtdList { get; set; }
        public List<ShopData> ShopData { get; set; }
        public byte[] EndOfFile { get; set; }

        public ShopDataFile Read(string shopDataFilePath)
        {
            ShopDataFile shopDataFileModel = new();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var reader = new BinaryObjectReader(shopDataFilePath, Endianness.Big, Encoding.GetEncoding(932));

            FtdHeader = new FtdHeader().Read(reader);
            FtdList = new FtdList().Read(reader);
            ShopData = new List<ShopData>();

            for (int i = 0; i < FtdList.EntryCount; i++)
            {
                ShopData.Add(new ShopData().Read(reader));
            }

            EndOfFile = reader.ReadArray<byte>((int)(FtdHeader.FileSize - reader.Position));

            return this;
        }

        public void Write(string newShopDataFile, ShopDataFile shopDataFileModel)
        {
            var asSpan = CollectionsMarshal.AsSpan(shopDataFileModel.ShopData);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (var writer = new BinaryObjectWriter(newShopDataFile, Endianness.Big, Encoding.GetEncoding(932)))
            {
                FtdHeader.Write(writer, FtdHeader);

                FtdList.Write(writer, FtdList);

                foreach (var shopData in asSpan)
                {
                    shopData.Write(writer, shopData);
                }

                writer.WriteArray(EndOfFile);
            }
        }

        public void Add(int insertIndex, int copyIndex)
        {
            ShopData.Insert(insertIndex, ShopData[copyIndex]);
            FtdList.EntryCount += 1;
            FtdList.DataSize += (uint)FtdList.EntrySize;
            FtdHeader.FileSize += (uint)FtdList.EntrySize;
        }

        public void Add(int insertIndex)
        {
            var genericShopData = new ShopData();

            genericShopData.BannerId = 99;
            genericShopData.HideNametag = false;
            genericShopData.ShopMode = 0;

            ShopData.Insert(insertIndex, genericShopData);
            FtdList.EntryCount += 1;
            FtdList.DataSize += (uint)FtdList.EntrySize;
            FtdHeader.FileSize += (uint)FtdList.EntrySize;
        }

        public void Remove(int removeIndex)
        {
            ShopData.Remove(ShopData[removeIndex]);
            FtdList.EntryCount -= 1;
            FtdList.DataSize -= (uint)FtdList.EntrySize;
            FtdHeader.FileSize -= (uint)FtdList.EntrySize;
        }
    }
}
