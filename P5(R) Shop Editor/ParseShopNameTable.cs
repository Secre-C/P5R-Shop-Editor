using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Amicitia.IO;
using Amicitia.IO.Binary;
using shopItems;
using Shop_Editor;

namespace shoplogic
{
    public class NameParse
    {
        public static string PrintShopName(int shopInput, int gameVersionIndex)
        {
            int nameLength;
            if (gameVersionIndex == 0) nameLength = 48;
            else nameLength = 32;

            string gameVersion;
            if (gameVersionIndex == 0) gameVersion = "Royal";
            else gameVersion = "Vanilla";
            string shopNameftd = (Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopName.ftd"); ;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (BinaryObjectReader P5FTDFile = new BinaryObjectReader(shopNameftd, Endianness.Big, Encoding.GetEncoding(932)))
            {
                uint entryCount = FtdParse.FtdRead(shopNameftd);
                long nameOffset = 48 + (shopInput * nameLength);
                P5FTDFile.AtOffset(nameOffset);
                string shopName = P5FTDFile.ReadString(StringBinaryFormat.FixedLength, nameLength);
                return shopName;
            }
        }

        public static List<string> MakeShopNameList(int gameVersionIndex)
        {
            int nameLength;
            if (gameVersionIndex == 0) nameLength = 48;
            else nameLength = 32;

            string gameVersion;
            if (gameVersionIndex == 0) gameVersion = "Royal";
            else gameVersion = "Vanilla";
            string shopNameftd = (Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopName.ftd");

            List<string> ShopNameList = new List<string>();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (BinaryObjectReader P5FTDFile = new BinaryObjectReader(shopNameftd, Endianness.Big, Encoding.GetEncoding(932)))
            {
                uint entryCount = FtdParse.FtdRead(shopNameftd);
                for (int i = 0; i < entryCount; i++)
                {
                    long nameOffset = 48 + (i * nameLength);
                    P5FTDFile.AtOffset(nameOffset);
                    string shopName = P5FTDFile.ReadString(StringBinaryFormat.FixedLength, nameLength);
                    ShopNameList.Add(shopName + " (" + i + ")");
                }
            }
            return ShopNameList;
        }
    }
}
