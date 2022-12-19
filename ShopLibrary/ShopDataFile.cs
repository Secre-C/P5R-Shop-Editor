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

        public void Read(string shopDataFilePath, out ShopDataFile shopDataFileModel)
        {
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

            shopDataFileModel = this;
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

        public void Add(ref ShopDataFile shopDataFileModel, int insertIndex, int copyIndex)
        {
            shopDataFileModel.ShopData.Insert(insertIndex, ShopData[copyIndex]);
            FtdList.EntryCount += 1;
            FtdList.DataSize += (uint)FtdList.EntrySize;
            FtdHeader.FileSize += (uint)FtdList.EntrySize;

            shopDataFileModel = this;
        }

        public void Remove(ref ShopDataFile shopDataFileModel, int removeIndex)
        {
            shopDataFileModel.ShopData.Remove(shopDataFileModel.ShopData[removeIndex]);
            FtdList.EntryCount -= 1;
            FtdList.DataSize -= (uint)FtdList.EntrySize;
            FtdHeader.FileSize -= (uint)FtdList.EntrySize;

            shopDataFileModel = this;
        }
    }
}
