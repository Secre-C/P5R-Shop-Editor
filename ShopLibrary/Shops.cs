using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;

namespace ShopLibrary
{
    public class Shop
    {
        public List<ShopItem> Items { get; set; }
        public Shop Read(BinaryObjectReader reader)
        {
            bool endOfShop = false;

            Items = new List<ShopItem>();

            while (!endOfShop)
            {
                Items.Add(new ShopItem().Read(reader));

                if (Items[Items.Count - 1].ShopEndIndicator == 157)
                    endOfShop = true;
            }

            Items.RemoveAt(Items.Count - 1);
            return this;
        }

        public void Write(BinaryObjectWriter writer, Shop shop)
        {
            CreateEndEntry(out var endEntry);
            shop.Items.Add(endEntry);

            var asSpan = CollectionsMarshal.AsSpan(shop.Items);

            foreach (var item in asSpan)
            {
                item.Write(writer, item);
            }
        }

        public void CreateEndEntry(out ShopItem endShopItem)
        {
            endShopItem = new();
            endShopItem.GenerateEndEntry(ref endShopItem);
        }

        public void CreateDummyEntry(out ShopItem endShopItem)
        {
            endShopItem = new();
            endShopItem.GenerateDummyEntry(ref endShopItem);
        }
    }
}
