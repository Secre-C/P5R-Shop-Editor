using ShopLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop_Editor
{
    internal class ShopComboBox
    {
        public List<string> NameIndex{ get; set; }

        public ShopComboBox(ShopNameFile shopNameFile)
        {
            NameIndex = new List<string>();
            for(int i = 0; i < shopNameFile.ShopNames.Count; i++)
            {
                NameIndex.Add($"{shopNameFile.ShopNames[i].Name} ({i})");
            }
        }
    }
}
