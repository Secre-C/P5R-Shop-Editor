using ShopLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop_Editor
{
    internal class ShopItemListBox
    {
        public List<string> ItemEntries { get; set; }

        public ShopItemListBox(Shop shop, List<string[]> fullItemList)
        {
            ItemEntries = new();
            for(int i = 0; i < shop.Items.Count; i++)
            {
                try
                {
                    ItemEntries.Add($"{fullItemList[shop.Items[i].ItemId.ItemCategory][shop.Items[i].ItemId.ItemIndex]} ({i})");
                }
                catch
                {
                    MainWindow.DebugLog(shop.Items[i].ItemId.ItemCategory);
                    MainWindow.DebugLog(shop.Items[i].ItemId.ItemIndex);
                }
            }
        }
    }
}
