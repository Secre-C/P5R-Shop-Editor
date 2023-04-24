using ShopLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop_Editor
{
    internal class ItemIdComboBox
    {
        public List<string> ItemsIndexes { get; set; }

        public ItemIdComboBox(int itemCategory, List<string[]> fullItemList)
        {
            ItemsIndexes = new List<string>();

            for(int i = 0; i < fullItemList[itemCategory].Length; i++)
            {
                ItemsIndexes.Add($"{fullItemList[itemCategory][i]} ({i})");
            }
        }
    }
}
