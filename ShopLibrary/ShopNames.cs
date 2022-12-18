using Amicitia.IO.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopLibrary
{
    public class ShopNames
    {
        public string Name { get; set; }
        public ShopNames Read(BinaryObjectReader reader)
        {
            Name = reader.ReadString(StringBinaryFormat.FixedLength, 48);
            return this;
        }

        public void Write(BinaryObjectWriter writer, ShopNames shopNames)
        {

        }
    }
}
