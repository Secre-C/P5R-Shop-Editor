using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Shop_Editor
{
    internal class TextValidation
    {
        internal static bool IsValidInt(string str)
        {
            if (str == "-") return true;
            return int.TryParse(str, out int i) && i >= -1 && i <= 2147483647;
        }

        internal static bool IsValidUByte(string str)
        {
            return int.TryParse(str, out int i) && i >= 0 && i <= 255;
        }

        internal static bool IsDayValid(string str, object sender)
        {
            int[] dayArray = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            var combo = (ComboBox)sender;
            var monthIndex = combo.SelectedIndex;
            return int.TryParse(str, out int i) && i >= 1 && i <= dayArray[monthIndex];
        }
    }
}
