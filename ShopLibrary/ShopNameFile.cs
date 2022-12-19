using Amicitia.IO.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace ShopLibrary
{
    public class ShopNameFile
    {
        public FtdHeader FtdHeader { get; set; }
        public FtdList FtdList { get; set; }
        public List<ShopNames> ShopNames{get; set; }
        public byte[] EndOfFile { get; set; }

        public void Read(string shopNameFilePath, out ShopNameFile shopNameFileModel)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var reader = new BinaryObjectReader(shopNameFilePath, Endianness.Big, Encoding.GetEncoding(932));

            FtdHeader = new FtdHeader().Read(reader);
            FtdList = new FtdList().Read(reader);
            ShopNames = new List<ShopNames>();

            for (int i = 0; i < FtdList.EntryCount; i ++)
            {
                ShopNames.Add(new ShopNames().Read(reader));
            }

            EndOfFile = reader.ReadArray<byte>((int)(FtdHeader.FileSize - reader.Position));

            shopNameFileModel = this;
        }

        public void Write(string newShopNameFile, ShopNameFile shopNameFile)
        {
            var asSpan = CollectionsMarshal.AsSpan(shopNameFile.ShopNames);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (var writer = new BinaryObjectWriter(newShopNameFile, Endianness.Big, Encoding.GetEncoding(932)))
            {
                FtdHeader.Write(writer, FtdHeader);
                FtdList.Write(writer, FtdList);

                foreach (var shopName in asSpan)
                {
                    shopName.Write(writer, shopName);
                }

                writer.WriteArray(EndOfFile);
            }
        }

        public void Add(ref ShopNameFile shopNameFileModel, int insertIndex, int copyIndex)
        {
            shopNameFileModel.ShopNames.Insert(insertIndex, ShopNames[copyIndex]);
            FtdList.EntryCount += 1;
            FtdList.DataSize += (uint)FtdList.EntrySize;
            FtdHeader.FileSize += (uint)FtdList.EntrySize;

            shopNameFileModel = this;
        }

        public void Remove(ref ShopNameFile shopNameFileModel, int removeIndex)
        {
            shopNameFileModel.ShopNames.Remove(shopNameFileModel.ShopNames[removeIndex]);
            FtdList.EntryCount -= 1;
            FtdList.DataSize -= (uint)FtdList.EntrySize;
            FtdHeader.FileSize -= (uint)FtdList.EntrySize;

            shopNameFileModel = this;
        }
    }
}
