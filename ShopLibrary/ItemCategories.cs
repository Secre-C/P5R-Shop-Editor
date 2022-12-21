using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShopLibrary
{
    public class ItemCategories
    {
        public string[] MeleeWeapons { get; set; }
        public string[] Armors { get; set; }
        public string[] Accessories { get; set; }
        public string[] Consumables { get; set; }
        public string[] KeyItems { get; set; }
        public string[] Materials { get; set; }
        public string[] SkillCards { get; set; }
        public string[] Outfits { get; set; }
        public string[] RangedWeapons { get; set; }

        public static List<string[]> GetItemList(string gameVer)
        {
            List<string[]> ItemList = new List<string[]>();
            ItemCategories itemCategories = JsonSerializer.Deserialize<ItemCategories>(File.ReadAllText($"C:\\Users\\Gabriel Premore\\source\\repos\\Shop Editor\\ShopLibrary\\P5R Items\\{gameVer}Items.Json"));

            ItemList.Add( itemCategories.MeleeWeapons);
            ItemList.Add( itemCategories.Armors);
            ItemList.Add( itemCategories.Accessories);
            ItemList.Add( itemCategories.Consumables);
            ItemList.Add( itemCategories.KeyItems);
            ItemList.Add( itemCategories.Materials);
            ItemList.Add( itemCategories.SkillCards);
            ItemList.Add( itemCategories.Outfits);
            ItemList.Add( itemCategories.RangedWeapons);
            return ItemList;
        }
    }
}
