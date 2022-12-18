using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;

namespace ShopLibrary
{
    public class Shop
    {
        public int ItemQuantity { get; private set; }
        public ShopNames ShopName { get; set; }
        public List<ShopItem> Items { get; set; }
        public int ShopOffset { get; set; }
        public Shop Read(BinaryObjectReader reader)
        {
            bool endOfShop = false;
            ItemQuantity = 0;
            Items = new List<ShopItem>();

            while (!endOfShop)
            {
                Items.Add(new ShopItem().Read(reader));

                if (Items[ItemQuantity].ShopEndIndicator == 157)
                    endOfShop = true;

                ItemQuantity++;
            }

            return this;
        }

        public void Write(BinaryObjectWriter writer, Shop shop)
        {

        }
    }
}
