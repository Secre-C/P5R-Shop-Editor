using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        enum Category
        {
            Melee = 0,
            Armor = 1,
            Accessories = 2,
            Consumables = 3,
            KeyItems = 4,
            Materials = 5,
            SkillCards = 6,
            Outfits = 7,
            Ranged = 8
        }
    }
}
