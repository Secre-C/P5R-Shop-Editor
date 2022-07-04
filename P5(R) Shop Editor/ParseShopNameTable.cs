using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Amicitia.IO;
using Amicitia.IO.Binary;

namespace Shop_Editor
{
    public class NameParse
    {
        public static string PrintShopName(int shopInput, int gameVersionIndex)
        {
            int nameLength;
            string gameVersion;
            string tempName;

            if (gameVersionIndex == 0)
            {
                nameLength = 48;
                tempName = MainWindow.tempNameR;
            }
            else
            {
                nameLength = 32;
                tempName = MainWindow.tempNameV;
            }

            string shopNameftd = tempName;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (BinaryObjectReader P5FTDFile = new BinaryObjectReader(shopNameftd, Endianness.Big, Encoding.GetEncoding(932)))
            {
                uint entryCount = FtdParse.FtdRead(shopNameftd);
                long nameOffset = 48 + shopInput * nameLength;
                P5FTDFile.AtOffset(nameOffset);
                string shopName = P5FTDFile.ReadString(StringBinaryFormat.FixedLength, nameLength);
                return shopName;
            }
        }

        public static List<string> MakeShopNameList(int gameVersionIndex)
        {
            int nameLength;
            string tempName;

            if (gameVersionIndex == 0)
            {
                nameLength = 48;
                tempName = MainWindow.tempNameR;
            }
            else
            {
                nameLength = 32;
                tempName = MainWindow.tempNameV;
            }

            List<string> ShopNameList = new List<string>();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (BinaryObjectReader P5FTDFile = new BinaryObjectReader(tempName, Endianness.Big, Encoding.GetEncoding(932)))
            {
                uint entryCount = FtdParse.FtdRead(tempName);
                for (int i = 0; i < entryCount; i++)
                {
                    long nameOffset = 48 + i * nameLength;
                    P5FTDFile.AtOffset(nameOffset);
                    string shopName = P5FTDFile.ReadString(StringBinaryFormat.FixedLength, nameLength);
                    ShopNameList.Add($"{shopName} ({i})");
                }
            }
            return ShopNameList;
        }
    }
}
