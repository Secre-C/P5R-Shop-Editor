using Amicitia.IO.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace ShopLibrary
{
    public class ShopData
    {
        public short BannerId { get; set; }
        public bool HideBanner { get; set; }
        public byte ShopMode { get; set; }

        public ShopData Read(BinaryObjectReader reader)
        {
            BannerId = reader.ReadInt16();
            HideBanner = reader.ReadByte() == 1 ? true : false;
            ShopMode = reader.ReadByte();

            return this;
        }

        public void Write(BinaryObjectWriter writer, ShopData shopData)
        {

        }
    }
}
