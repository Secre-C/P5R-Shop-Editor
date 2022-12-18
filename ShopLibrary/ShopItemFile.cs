using Amicitia.IO.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public ShopItemFile Read(string shopItemFile)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var reader = new BinaryObjectReader(shopItemFile, Endianness.Big, Encoding.GetEncoding(932));

            FtdHeader = new FtdHeader().Read(reader);
            FtdList = new FtdList().Read(reader);
            Shops = new List<Shop>();

            for (int i = 0; i < FtdList.EntryCount; i += Shops[Shops.Count - 1].ItemQuantity)
            {
                Shops.Add(new Shop().Read(reader));
            }

            return this;
        }
    }
}
