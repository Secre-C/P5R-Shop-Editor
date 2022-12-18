using Amicitia.IO.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace ShopLibrary
{
    public class ShopNameFile
    {
        public FtdHeader FtdHeader { get; set; }
        public FtdList FtdList { get; set; }
        public List<ShopNames> ShopNames{get; set;}

        public ShopNameFile Read(string shopItemFile)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var reader = new BinaryObjectReader(shopItemFile, Endianness.Big, Encoding.GetEncoding(932));

            FtdHeader = new FtdHeader().Read(reader);
            FtdList = new FtdList().Read(reader);
            ShopNames = new List<ShopNames>();

            for (int i = 0; i < FtdList.EntryCount; i ++)
            {
                ShopNames.Add(new ShopNames().Read(reader));
            }

            return this;
        }
    }
}
