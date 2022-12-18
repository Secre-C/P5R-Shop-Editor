using Amicitia.IO.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopLibrary
{
    public class ShopDataFile
    {
        public FtdHeader FtdHeader { get; set; }
        public FtdList FtdList { get; set; }
        public List<ShopData> ShopData { get; set; }

        public ShopDataFile Read(string shopDataFile)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var reader = new BinaryObjectReader(shopDataFile, Endianness.Big, Encoding.GetEncoding(932));

            FtdHeader = new FtdHeader().Read(reader);
            FtdList = new FtdList().Read(reader);
            ShopData = new List<ShopData>();

            for (int i = 0; i < FtdList.EntryCount; i++)
            {
                ShopData.Add(new ShopData().Read(reader));
            }

            return this;
        }

        public void Write(string newShopDataFile, ShopDataFile shopDataFile)
        {

        }
    }
}
