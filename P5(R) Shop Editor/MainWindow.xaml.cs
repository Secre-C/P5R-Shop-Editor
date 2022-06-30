using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using shoplogic;
using Amicitia.IO.Binary;
using Amicitia.IO;
using shopItems;

namespace Shop_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool windowShowFirstTime = true;
        public static string tempFileV = "";
        public static string tempFileR = "";
        public static string tempNameV = "";
        public static string tempNameR = "";

        public MainWindow()
        {
            CreateTempFiles();
            CreateDirectories();
            InitializeComponent();
            CheckForFtdsOnStartup();
            PopulateShopItemComboBox();
            PopulateItemIdComboBox();
            PopulateShopNameComboBox();
            windowShowFirstTime = false;
            ResetStuff();
        }
        private void StartDayValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsDayValid(((TextBox)sender).Text + e.Text, StartMonthBox);
        }
        private void EndDayValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsDayValid(((TextBox)sender).Text + e.Text, EndMonthBox);
        }
        private void UByteValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsValidUByte(((TextBox)sender).Text + e.Text);
        }
        private void IntValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsValidInt(((TextBox)sender).Text + e.Text);
        }

        public static bool IsValidInt(string str)
        {
            if (str == "-") return true;
            return int.TryParse(str, out int i) && i >= -2147483647 && i <= 2147483647;
        }

        public static bool IsValidUByte(string str)
        {
            return int.TryParse(str, out int i) && i >= 0 && i <= 255;
        }

        public static bool IsDayValid(string str, object sender)
        {
            int[] dayArray = GetDayArray();
            var combo = (ComboBox)sender;
            var monthIndex = combo.SelectedIndex;
            return int.TryParse(str, out int i) && i >= 1 && i <= dayArray[monthIndex];
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;
            string gameVersion;
            string tempFile;
            string tempName;
            int nameLength;

            if (gameVersionIndex == 0)
            {
                gameVersion = "Royal";
                tempFile = tempFileR;
                tempName = tempNameR;
                nameLength = 48;
            }
            else
            {
                gameVersion = "Vanilla";
                tempFile = tempFileV;
                tempName = tempNameV;
                nameLength = 32;
            }

            if (!CheckForChanges() && !CompareFiles(40, 1) && !CompareFiles(nameLength, 1))
            {
                ShowMessage("No new changes have been made!");
                return;
            }

            if (File.Exists(Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopItemTable.ftd"))
            {
                File.Copy(Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopItemTable.ftd", tempFile, true);
            }

            if (File.Exists(Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopName.ftd"))
            {
                File.Copy(Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopName.ftd", tempName, true);
            }

            ResetStuff();

            ShowMessage("Unsaved Changes have been reverted!");
        }

        private void ResetAllButton_Click(object sender, RoutedEventArgs e)
        {

            string gameVersion;
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            string tempFile;
            string tempName;

            int nameLength;

            if (gameVersionIndex == 0)
            {
                gameVersion = "Royal";
                tempFile = tempFileR;
                tempName = tempNameR;
                nameLength = 48;
            }
            else
            {
                gameVersion = "Vanilla";
                tempFile = tempFileV;
                tempName = tempNameV;
                nameLength = 32;
            }

            if (!CheckForChanges() && !CompareFiles(40, 0) && !CompareFiles(nameLength, 0))
            {
                ShowMessage("No new changes have been made!");
                return;
            }

            string shopItemftdOriginal = (Directory.GetCurrentDirectory() + $"\\Original\\{gameVersion}\\fclPublicShopItemTable.ftd");
            string shopNameftdOriginal = (Directory.GetCurrentDirectory() + $"\\Original\\{gameVersion}\\fclPublicShopName.ftd");

            File.Copy(shopItemftdOriginal, tempFile, true);
            File.Copy(shopNameftdOriginal, tempName, true);

            ResetStuff();
            ShowMessage($"All items have been reverted to their original state!");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            int nameLength = 48;

            string tempFile = tempFileR;
            string gameVersion = "Royal";
            
            if (gameVersionIndex == 1)
            {
                nameLength = 32;
                tempFile = tempFileV;
                gameVersion = "Vanilla";
            }


            bool isOriginalFileDifferent = CompareFiles(40, 0);
            bool isOutputFileDifferent = CompareFiles(40, 1);

            if (!CheckForChanges())
            {
                if (!isOutputFileDifferent && AreEntryCountsEqual(1))
                {
                    ShowMessage("No new changes have been made!");
                    return;
                }
                else if (isOutputFileDifferent && !isOriginalFileDifferent && AreEntryCountsEqual(0))
                {
                    File.Copy(tempFile, Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopItemTable.ftd", true);
                    ShowMessage($"Changes Saved to 'Output\\{gameVersion}'!");
                    return;
                }
            }

            if (CheckItemChanges() || !AreEntryCountsEqual(1))
            {
                SaveItemftd();
            }

            SaveNameftd();

            if (AreEntryCountsEqual(0) && CompareFiles(40, 2))
            {
                ShowMessage($"Created Binary Patch file (.bp) and saved changes to 'Output\\{gameVersion}'!");
            }
            else
            {
                ShowMessage($"Changes Saved to 'Output\\{gameVersion}'!");

                if (File.Exists(Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\Shop_Items.bp"))
                {
                    File.Delete(Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\Shop_Items.bp");
                }
            }

            bool isNameChanged = CheckNameChanges();

            if (isNameChanged)
            {
                CompareFiles(nameLength, 2);
            }

            ResetStuff();
        }

        private void AddNewItemButton_Click(object sender, RoutedEventArgs e)
        {
            AddItemSlot();

            ItemSelectionComboBox.SelectedIndex += 1;

            ItemSelectionComboBox_SelectionChanged(null, null);
            PopulateShopInformation();
            PopulateShopItemComboBox();

            ShowMessage("Created New item!");
        }

        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (NumOfItemsTextBox.Text == "1")
            {
                string deletePrompt = "Item not removed. Each Shop Needs at least one Item!";
                ShowMessage(deletePrompt);
                return;
            }

            string itemDeleteMessage = $"Selected item has been removed!";

            RemoveItemSlot();
            ItemSelectionComboBox.SelectedIndex -= 1;

            ItemSelectionComboBox_SelectionChanged(null, null);

            PopulateShopInformation();
            PopulateShopItemComboBox();

            ShowMessage(itemDeleteMessage);
        }
        private void ItemSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            string tempFile;

            if (gameVersionIndex == 0)
            {
                tempFile = tempFileR;
            }
            else
            {
                tempFile = tempFileV;
            }

            string shopItemftd = tempFile;

            int shopInput = ShopSelectionComboBox.SelectedIndex;

            List<int> shopOffsets = FtdParse.FindShopOffsetsandCount(shopItemftd, "ShopOffsets");

            int shopItemIndex = ItemSelectionComboBox.SelectedIndex;
            if (shopItemIndex <= -1 || shopInput <= -1)
            {
                return;
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (BinaryObjectReader P5FTDFile = new (shopItemftd, Endianness.Big, Encoding.GetEncoding(932)))
            {
                int shopItemIndexOffset = shopItemIndex * 40;
                long startOffset = shopItemIndexOffset + shopOffsets[shopInput];
                P5FTDFile.AtOffset(startOffset);

                byte field0 = P5FTDFile.ReadByte();
                if (field0 == 0xFF) UnlimitedQuantityCheckBox.IsChecked = true;
                else UnlimitedQuantityCheckBox.IsChecked = false;

                P5FTDFile.ReadByte(); 
                ushort itemIndex = P5FTDFile.ReadUInt16();
                AmountPerUnitTextBox.Text = P5FTDFile.ReadByte().ToString();
                StartMonthBox.SelectedIndex = P5FTDFile.ReadByte() - 1;
                StartDayTextBox.Text = P5FTDFile.ReadByte().ToString();
                EndMonthBox.SelectedIndex = P5FTDFile.ReadByte() - 1;
                EndDayTextBox.Text = P5FTDFile.ReadByte().ToString();
                P5FTDFile.ReadByte();
                QuantityTextBox.Text = P5FTDFile.ReadByte().ToString();
                P5FTDFile.ReadByte();
                P5FTDFile.ReadByte();
                P5FTDFile.ReadByte();
                P5FTDFile.ReadInt16();
                P5FTDFile.ReadInt32();
                P5FTDFile.ReadInt32();
                short bitflagSection = P5FTDFile.ReadInt16();
                short bitflagRequirement = P5FTDFile.ReadInt16();
                P5FTDFile.ReadInt32();
                PricePercentageTextBox.Text = P5FTDFile.ReadInt32().ToString();
                P5FTDFile.ReadInt32();

                int Bitflag;
                if (gameVersionIndex == 0)
                {
                    Bitflag = FtdParse.SumRoyalFlag(bitflagSection, bitflagRequirement);
                }
                else
                {
                    Bitflag = bitflagRequirement;
                }

                int itemCategory = itemIndex / 0x1000;
                int itemID = itemIndex - (itemCategory * 0x1000);
                ItemCategoryComboBox.SelectedIndex = itemCategory;
                ItemIDComboBox.SelectedIndex = itemID;

                RequiredBitflagTextBox.Text = Bitflag.ToString();

                if (Bitflag == 65536) RequiredBitflagTextBox.Text = "-1";
            }
        }

        private void ItemCategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (windowShowFirstTime)
            {
                return;
            }

            int[] itemSectionArray = { 256, 301, 256, 512, 256, 256, 387, 182, 256 };
            int[] itemSectionArrayR = { 296, 301, 512, 696, 256, 256, 651, 286, 256 };

            if (GameVersionComboBox.SelectedIndex == 0)
            {
                if (Convert.ToInt32(ItemIDComboBox.SelectedIndex) > itemSectionArrayR[ItemCategoryComboBox.SelectedIndex] - 1)
                {
                    ItemIDComboBox.SelectedIndex = (itemSectionArrayR[ItemCategoryComboBox.SelectedIndex] - 1);
                }
            }
            else
            {
                if (Convert.ToInt32(ItemIDComboBox.SelectedIndex) > itemSectionArray[ItemCategoryComboBox.SelectedIndex] - 1)
                {
                    ItemIDComboBox.SelectedIndex = (itemSectionArray[ItemCategoryComboBox.SelectedIndex] - 1);
                }
            }

            PopulateItemIdComboBox();
        }

        private void GameVersionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (windowShowFirstTime) //prevent function from running at launch
            {
                return;
            }

            string gameVersion;
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            string tempFile;

            if (gameVersionIndex == 0)
            {
                gameVersion = "Royal";
                tempFile = tempFileR;
            }
            else
            {
                gameVersion = "Vanilla";
                tempFile = tempFileV;
            }

            if (!File.Exists(Directory.GetCurrentDirectory() + $"\\Original\\{gameVersion}\\fclPublicShopItemTable.ftd") || !File.Exists(Directory.GetCurrentDirectory() + $"\\Original\\{gameVersion}\\fclPublicShopName.ftd"))
            {
                string missingftdMessage = "Missing Ftds! Make sure both fclPublicShopItemTable.ftd and fclPublicShopName.ftd have been copied to the Original\\{gameVersion} Folder!";
                MessageBox.Show(missingftdMessage, "Missing Ftds");
                if (gameVersion == "Royal") gameVersionIndex = 1;
                else gameVersionIndex = 0;

                GameVersionComboBox.SelectedIndex = gameVersionIndex;

                return;
            }
            //get some variables from other ftd
            string shopItemftd = tempFile;
            List<int> shopCountList = FtdParse.FindShopOffsetsandCount(shopItemftd, "shopCount");
            int shopCount = shopCountList[0];

            //reread shop name list
            List<string> ShopNameList = NameParse.MakeShopNameList(gameVersionIndex);
            int selectedShop = ShopSelectionComboBox.SelectedIndex;
            ShopSelectionComboBox.ItemsSource = ShopNameList;
            if (selectedShop > shopCount) ShopSelectionComboBox.SelectedIndex = shopCount;
            else if (selectedShop == 0) ShopSelectionComboBox.SelectedIndex = 0;

            //change shop name character limit
            if (GameVersionComboBox.SelectedIndex == 0) ShopNameTextBox.MaxLength = 48;
            else ShopNameTextBox.MaxLength = 32;

            //reread shop item list
            var shopIndex = ShopSelectionComboBox.SelectedIndex;
            List<string> ShopItemList = ItemParse.MakeShopItemList(shopIndex, gameVersionIndex);
            ItemSelectionComboBox.ItemsSource = ShopItemList;
            ItemSelectionComboBox.SelectedIndex = 0;
            ResetStuff();

            //reread Item Name list
            PopulateItemIdComboBox();
        }

        private void StartMonthBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = (ComboBox)sender;
            var monthIndex = combo.SelectedIndex;
            int[] dayArray = GetDayArray();
            if (StartDayTextBox.Text == "")
            {
                StartDayTextBox.Text = "1";
            }

            if (dayArray[monthIndex] < Convert.ToInt32(StartDayTextBox.Text))
            {
              StartDayTextBox.Text = Convert.ToString(dayArray[monthIndex]);
            }
        }
        private void EndMonthBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = (ComboBox)sender;
            var monthIndex = combo.SelectedIndex;
            int[] dayArray = GetDayArray();
            if (EndDayTextBox.Text == "")
            {
                EndDayTextBox.Text = "1";
            }
            if (dayArray[monthIndex] < Convert.ToInt32(EndDayTextBox.Text))
            {
                EndDayTextBox.Text = Convert.ToString(dayArray[monthIndex]);
            }
        }

        public static int[] GetDayArray()
        {
            int[] dayArray = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            return dayArray;
        }

        private void ShopSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            string tempFile;

            if (gameVersionIndex == 0)
            {
                tempFile = tempFileR;
            }
            else
            {
                tempFile = tempFileV;
            }

            string shopItemftd = tempFile;

            var shopIndex = ShopSelectionComboBox.SelectedIndex;

            if (shopIndex == -1) return;
            List<int> itemCount = FtdParse.FindShopOffsetsandCount(shopItemftd, "ItemCount");

            NumOfItemsTextBox.Text = itemCount[shopIndex].ToString();
            ShopNameTextBox.Text = NameParse.PrintShopName(shopIndex, GameVersionComboBox.SelectedIndex);
            ShopIDTextBox.Text = ShopSelectionComboBox.SelectedIndex.ToString();

            List<string> ShopItemList = ItemParse.MakeShopItemList(shopIndex, GameVersionComboBox.SelectedIndex);
            ItemSelectionComboBox.ItemsSource = ShopItemList;
            ItemSelectionComboBox.SelectedIndex = 0;
        }

        private void UnlimitedQuantity_Checked(object sender, RoutedEventArgs e)
        {
            var check = (CheckBox)sender;
            if (check.IsChecked == true)
            {
                QuantityTextBox.IsReadOnly = true;
                QuantityTextBox.Text = "99";
                QuantityTextBox.Background = new SolidColorBrush(Color.FromArgb(0xff, 0x20, 0x20, 0x20));
                QuantityTextBox.Foreground = Brushes.White;

            }
            else
            {
                QuantityTextBox.IsReadOnly = false;
                QuantityTextBox.Background = Brushes.White;
                QuantityTextBox.Foreground = Brushes.Black;
            }
        }

        private void PopulateShopNameComboBox()
        {
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            List<string> ShopNameList = NameParse.MakeShopNameList(gameVersionIndex);

            var shopIndex = ShopSelectionComboBox.SelectedIndex;
            ShopSelectionComboBox.ItemsSource = ShopNameList;

            if (shopIndex == -1)
            {
                shopIndex = 0;
            }

            ShopSelectionComboBox.SelectedIndex = shopIndex;

            PopulateShopInformation();
        }

        private void PopulateShopInformation()
        {
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            string tempFile;

            if (gameVersionIndex == 0)
            {
                tempFile = tempFileR;
            }
            else
            {
                tempFile = tempFileV;
            }

            string shopItemftd = tempFile;

            var shopIndex = ShopSelectionComboBox.SelectedIndex;

            List<int> itemCount = FtdParse.FindShopOffsetsandCount(shopItemftd, "ItemCount");

            NumOfItemsTextBox.Text = itemCount[shopIndex].ToString();
            ShopIDTextBox.Text = ShopSelectionComboBox.SelectedIndex.ToString();
        }
        private void PopulateShopItemComboBox()
        {
            int shopID = ShopSelectionComboBox.SelectedIndex;
            
            if (shopID == -1) shopID = 0;

            List<string> ShopItemList = ItemParse.MakeShopItemList(shopID, GameVersionComboBox.SelectedIndex);

            int prevIndex = ItemSelectionComboBox.SelectedIndex;
            if (prevIndex == -1) prevIndex = 0;

            ItemSelectionComboBox.ItemsSource = ShopItemList;
            ItemSelectionComboBox.SelectedIndex = prevIndex;
        }
        private void PopulateItemIdComboBox()
        {
            ThreadStart PopulateItemBox = new (GetItemList);
            Thread ItemBoxThread = new (PopulateItemBox);
            ItemBoxThread.Start();
        }

        private void GetItemList()
        {
            this.Dispatcher.Invoke(() =>
            {
                string[] itemList;
                List<string> numberedItemList = new();

                if (GameVersionComboBox.SelectedIndex == 0)
                {
                    itemList = P5RItems.ItemLists((ushort)(ItemCategoryComboBox.SelectedIndex * 0x1000));
                }
                else
                {
                    itemList = P5Items.ItemLists((ushort)(ItemCategoryComboBox.SelectedIndex * 0x1000));
                }

                for (int i = 0; i < itemList.Length; i++)
                {
                    numberedItemList.Add(itemList[i] + " (" + i + ")");
                }

                int prevItem = GetItemNum();
                
                ItemIDComboBox.ItemsSource = numberedItemList;
                ItemIDComboBox.SelectedIndex = prevItem;
            });
        }

        private void ResetStuff()
        {
            int prevItem = ItemSelectionComboBox.SelectedIndex;
            ShopSelectionComboBox_SelectionChanged(null, null);
            ItemSelectionComboBox_SelectionChanged(null, null);
            PopulateShopNameComboBox();
            ItemSelectionComboBox.SelectedIndex = prevItem;
        }
        private void CheckForFtdsOnStartup()
        {
            if (!File.Exists(Directory.GetCurrentDirectory() + "\\Original\\Royal\\fclPublicShopItemTable.ftd") || !File.Exists(Directory.GetCurrentDirectory() + "\\Original\\Royal\\fclPublicShopName.ftd"))
            {
                if (!File.Exists(Directory.GetCurrentDirectory() + "\\Original\\Vanilla\\fclPublicShopItemTable.ftd") || !File.Exists(Directory.GetCurrentDirectory() + "\\Original\\Vanilla\\fclPublicShopName.ftd"))
                {
                    string missingftdMessage = "Missing Ftds! Make sure both your fclPublicShopItemTable.ftd and fclPublicShopName.ftd files from your game of choice have been copied to the appropriate folder inside the 'Original' folder";
                    MessageBox.Show(missingftdMessage, "Missing Ftds");
                    Environment.Exit(1);
                }
                else
                {
                    GameVersionComboBox.SelectedIndex = 1;
                }
            }
            else
            {
                if (File.Exists(Directory.GetCurrentDirectory() + "\\Output\\Royal\\fclPublicShopItemTable.ftd"))
                {
                    File.Copy(Directory.GetCurrentDirectory() + "\\Output\\Royal\\fclPublicShopItemTable.ftd", tempFileR, true);
                }
                else
                {
                    File.Copy(Directory.GetCurrentDirectory() + "\\Original\\Royal\\fclPublicShopItemTable.ftd", tempFileR, true);
                }

                if (File.Exists(Directory.GetCurrentDirectory() + "\\Output\\Royal\\fclPublicShopName.ftd"))
                {
                    File.Copy(Directory.GetCurrentDirectory() + "\\Output\\Royal\\fclPublicShopName.ftd", tempNameR, true);
                }
                else
                {
                    File.Copy(Directory.GetCurrentDirectory() + "\\Original\\Royal\\fclPublicShopName.ftd", tempNameR, true);
                }
            }

            if (File.Exists(Directory.GetCurrentDirectory() + "\\Original\\Vanilla\\fclPublicShopItemTable.ftd") 
                && File.Exists(Directory.GetCurrentDirectory() + "\\Original\\Vanilla\\fclPublicShopName.ftd"))
            {
                if (File.Exists(Directory.GetCurrentDirectory() + "\\Output\\Vanilla\\fclPublicShopItemTable.ftd"))
                {
                    File.Copy(Directory.GetCurrentDirectory() + "\\Output\\Vanilla\\fclPublicShopItemTable.ftd", tempFileV, true);
                }
                else
                {
                    File.Copy(Directory.GetCurrentDirectory() + "\\Original\\Vanilla\\fclPublicShopItemTable.ftd", tempFileV, true);
                }

                if (File.Exists(Directory.GetCurrentDirectory() + "\\Output\\Vanilla\\fclPublicShopName.ftd"))
                {
                    File.Copy(Directory.GetCurrentDirectory() + "\\Output\\Vanilla\\fclPublicShopName.ftd", tempNameV, true);
                }
                else
                {
                    File.Copy(Directory.GetCurrentDirectory() + "\\Original\\Vanilla\\fclPublicShopName.ftd", tempNameV, true);
                }
            }

            if (GameVersionComboBox.SelectedIndex == 0) ShopNameTextBox.MaxLength = 48;
            else ShopNameTextBox.MaxLength = 32;
        }

        private void SaveItemftd()
        {
            string gameVersion;
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;
            string tempFile;

            if (gameVersionIndex == 0)
            {
                gameVersion = "Royal";
                tempFile = tempFileR;
            }
            else
            {
                gameVersion = "Vanilla";
                tempFile = tempFileV;
            }

            string shopItemftd = (Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopItemTable.ftd");

            List<int> shopOffsets = FtdParse.FindShopOffsetsandCount(tempFile, "ShopOffsets");

            int shopID = Convert.ToInt32(ShopIDTextBox.Text);
            int shopItemIndex = ItemSelectionComboBox.SelectedIndex;

            int itemOffset = shopOffsets[shopID] + (shopItemIndex * 40);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (BinaryObjectReader P5FTDFile = new (tempFile, Endianness.Big, Encoding.GetEncoding(932)))
            {
                using (BinaryObjectWriter P5NewFTDFile = new (shopItemftd, Endianness.Big, Encoding.GetEncoding(932)))
                {
                    for (int i = 0; i < itemOffset; i++)
                    {
                        P5NewFTDFile.Write(P5FTDFile.ReadByte());
                    }
                    P5FTDFile.ReadByte(); //field0
                    if (UnlimitedQuantityCheckBox.IsChecked == true) P5NewFTDFile.Write((byte)0xFF);
                    else P5NewFTDFile.Write((byte)0);

                    P5NewFTDFile.Write(P5FTDFile.ReadByte()); //field1

                    P5FTDFile.ReadUInt16(); //item id
                    ushort newItemID = (ushort)((ItemCategoryComboBox.SelectedIndex * 0x1000) + ItemIDComboBox.SelectedIndex);
                    P5NewFTDFile.Write(newItemID);

                    P5FTDFile.ReadByte(); //Amount acquired per unit purchased
                    P5NewFTDFile.Write(Convert.ToByte(AmountPerUnitTextBox.Text));

                    P5FTDFile.ReadByte(); //start month
                    P5NewFTDFile.Write((byte)(StartMonthBox.SelectedIndex + 1));

                    P5FTDFile.ReadByte(); //start day
                    P5NewFTDFile.Write(Convert.ToByte(StartDayTextBox.Text));

                    P5FTDFile.ReadByte(); //end month
                    P5NewFTDFile.Write((byte)(EndMonthBox.SelectedIndex + 1));

                    P5FTDFile.ReadByte(); //end day
                    P5NewFTDFile.Write(Convert.ToByte(EndDayTextBox.Text));

                    byte newQuantity = Convert.ToByte(QuantityTextBox.Text); //quantity
                    P5FTDFile.ReadByte();
                    P5FTDFile.ReadByte();
                    P5FTDFile.ReadByte();
                    P5NewFTDFile.Write(newQuantity);
                    P5NewFTDFile.Write(newQuantity);
                    P5NewFTDFile.Write(newQuantity);

                    P5NewFTDFile.Write(P5FTDFile.ReadInt32());
                    P5NewFTDFile.Write(P5FTDFile.ReadInt32());
                    P5NewFTDFile.Write(P5FTDFile.ReadInt32());

                    P5FTDFile.ReadUInt32(); //Bitflag requirement
                    P5NewFTDFile.Write(Convert.ToInt32(RequiredBitflagTextBox.Text));

                    P5NewFTDFile.Write(P5FTDFile.ReadInt32());

                    P5FTDFile.ReadInt32(); //Price Percentage
                    P5NewFTDFile.Write(Convert.ToInt32(PricePercentageTextBox.Text));

                    P5NewFTDFile.Write(P5FTDFile.ReadInt32());

                    long remainingSpace = P5FTDFile.Length - P5FTDFile.Position;

                    for (int i = 0; i < (int)remainingSpace; i++) //write to end of file
                    {
                        P5NewFTDFile.Write(P5FTDFile.ReadByte());
                    }
                }
            }
            File.Copy(shopItemftd, tempFile, true);
        }

        private void AddItemSlot()
        {
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;
            string tempFile;

            if (gameVersionIndex == 0)
            {
                tempFile = tempFileR;
            }
            else
            {
                tempFile = tempFileV;
            }

            List<int> shopOffsets = FtdParse.FindShopOffsetsandCount(tempFile, "ShopOffsets");

            int shopID = Convert.ToInt32(ShopIDTextBox.Text);
            int shopItemIndex = ItemSelectionComboBox.SelectedIndex;

            int itemOffset = shopOffsets[shopID] + (shopItemIndex * 40);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (BinaryObjectReader P5FTDFile = new(tempFile, Endianness.Big, Encoding.GetEncoding(932)))
            {
                using (BinaryObjectWriter P5NewFTDFile = new(tempFile, Endianness.Big, Encoding.GetEncoding(932)))
                {
                    for (int i = 0; i < itemOffset; i++)
                    {
                        P5NewFTDFile.Write(P5FTDFile.ReadByte());
                    }
                    long currentPosition = P5FTDFile.Position;
                    int numOfItems = Convert.ToInt32(NumOfItemsTextBox.Text);
                    if (numOfItems < 1)
                    {
                        for (int i = 0; i < 40; i++) //add in end of last shop
                        {
                            P5NewFTDFile.Write(P5FTDFile.ReadByte());
                        }

                        P5FTDFile.AtOffset(48);

                        for (int i = 0; i < 40; i++)
                        {
                            P5NewFTDFile.Write(P5FTDFile.ReadByte());
                        }
                        currentPosition += 40;
                    }
                    else
                    {
                        for (int i = 0; i < 40; i++)
                        {
                            P5NewFTDFile.Write(P5FTDFile.ReadByte());
                        }
                    }

                    P5NewFTDFile.At(8, SeekOrigin.Begin);
                    P5FTDFile.AtOffset(8); //Filesize
                    P5NewFTDFile.Write(P5FTDFile.ReadUInt32() + 40); //Subtract Filesize field by 40

                    P5NewFTDFile.At(36, SeekOrigin.Begin);
                    P5FTDFile.AtOffset(36); //Datasize
                    P5NewFTDFile.Write(P5FTDFile.ReadUInt32() + 40); //Subtract Datasize field by 40
                    P5NewFTDFile.Write(P5FTDFile.ReadUInt32() + 1); //Subtract Entry Count field by 1

                    P5NewFTDFile.At(0, SeekOrigin.End);
                    P5FTDFile.AtOffset(currentPosition);

                    long remainingSpace = P5FTDFile.Length - P5FTDFile.Position;

                    for (int i = 0; i < (int)remainingSpace; i++) //write to end of file
                    {
                        P5NewFTDFile.Write(P5FTDFile.ReadByte());
                    }
                }
            }
        }

        private void RemoveItemSlot()
        {
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;
            string tempFile;

            if (gameVersionIndex == 0)
            {
                tempFile = tempFileR;
            }
            else
            {
                tempFile = tempFileV;
            }

            List<int> shopOffsets = FtdParse.FindShopOffsetsandCount(tempFile, "ShopOffsets");

            int shopID = Convert.ToInt32(ShopIDTextBox.Text);
            int shopItemIndex = ItemSelectionComboBox.SelectedIndex;

            int itemOffset = shopOffsets[shopID] + (shopItemIndex * 40);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (BinaryObjectReader P5FTDFile = new(tempFile, Endianness.Big, Encoding.GetEncoding(932)))
            {
                using (BinaryObjectWriter P5NewFTDFile = new(tempFile, Endianness.Big, Encoding.GetEncoding(932)))
                {
                    for (int i = 0; i < itemOffset; i++)
                    {
                        P5NewFTDFile.Write(P5FTDFile.ReadByte());
                    }
                    for (int i = 0; i < 40; i++)
                    {
                        P5FTDFile.ReadByte();
                    }

                    long currentPosition = P5FTDFile.Position;

                    P5NewFTDFile.At(8, SeekOrigin.Begin);
                    P5FTDFile.AtOffset(8); //Filesize
                    P5NewFTDFile.Write(P5FTDFile.ReadUInt32() - 40); //Subtract Filesize field by 40

                    P5NewFTDFile.At(36, SeekOrigin.Begin);
                    P5FTDFile.AtOffset(36); //Datasize
                    P5NewFTDFile.Write(P5FTDFile.ReadUInt32() - 40); //Subtract Datasize field by 40
                    P5NewFTDFile.Write(P5FTDFile.ReadUInt32() - 1); //Subtract Entry Count field by 1

                    P5FTDFile.AtOffset(currentPosition);
                    P5NewFTDFile.At(0, SeekOrigin.End);

                    long remainingSpace = P5FTDFile.Length - P5FTDFile.Position;

                    for (int i = 0; i < (int)remainingSpace; i++) //write to end of file
                    {
                        P5NewFTDFile.Write(P5FTDFile.ReadByte());
                    }
                }
            }
        }
        private void SaveNameftd()
        {
            string gameVersion;
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            string tempName;

            if (gameVersionIndex == 0)
            {
                gameVersion = "Royal";
                tempName = tempNameR;
            }
            else
            {
                gameVersion = "Vanilla";
                tempName = tempNameV;
            }

            string tempshopNameftd = tempName;
            string shopNameftd = (Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopName.ftd");

            int shopID = Convert.ToInt32(ShopIDTextBox.Text);

            int nameLength;
            if (gameVersionIndex == 0) nameLength = 48;
            else nameLength = 32;

            int itemOffset = 48 + (shopID * nameLength);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (BinaryObjectReader P5FTDFile = new (shopNameftd, Endianness.Big, Encoding.GetEncoding(932)))
            {
                using (BinaryObjectWriter P5NewFTDFile = new (tempshopNameftd, Endianness.Big, Encoding.GetEncoding(932)))
                {
                    for (int i = 0; i < itemOffset; i++)
                    {
                        P5NewFTDFile.Write(P5FTDFile.ReadByte());
                    }

                    P5FTDFile.ReadString(StringBinaryFormat.FixedLength, nameLength);
                    P5NewFTDFile.WriteString(StringBinaryFormat.FixedLength, ShopNameTextBox.Text, nameLength);

                    long remainingSpace = P5FTDFile.Length - P5FTDFile.Position;

                    for (int i = 0; i < (int)remainingSpace; i++) //write to end of file
                    {
                        P5NewFTDFile.Write(P5FTDFile.ReadByte());
                    }
                }
            }
            File.Copy(tempshopNameftd, shopNameftd, true);
        }

        private bool CompareFiles(int structSize, int mode) //0 for original, 1 for output, 2 for output and write bp
        {
            string gameVersion;
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            if (gameVersionIndex == 0) gameVersion = "Royal";
            else gameVersion = "Vanilla";

            string ftdName = "fclPublicShopItemTable";
            string bpName = (Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\Shop_Items.bp");

            if (structSize == 32 || structSize == 48)
            {
                ftdName = "fclPublicShopName";
                bpName = (Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\Shop_Names.bp");

            }

            string shopftd = (Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\{ftdName}.ftd");

            if (mode == 0)
            {
                shopftd = (Directory.GetCurrentDirectory() + $"\\Original\\{gameVersion}\\{ftdName}.ftd");
            }

            List<int> changeOffsetList = GetChangeOffsets(structSize, gameVersionIndex, ftdName, mode);

            if (changeOffsetList.Count <= 0)
            {
                if (File.Exists(bpName))
                {
                    File.Delete(bpName);
                }
                return false;
            }
            else if (mode == 0 || mode == 1 || !AreEntryCountsEqual(0))
            {
                return true;
            }
            else
            {
                WriteBinaryPatch(structSize, bpName, changeOffsetList, ftdName, shopftd);
            }
            return true;
        }
        private void WriteBinaryPatch(int structSize, string bpName, List<int>changeOffsetList, string ftdName, string shopftd)
        {
            string shopByteString;
            string shopValues;

            using StreamWriter binaryPatch = new(bpName);

            string startBP = "{\n  \"Version\": 1,\n  \"Patches\": [";
            binaryPatch.WriteLine(startBP);

            for (int i = 0; i < changeOffsetList.Count; i++)
            {
                int offset = changeOffsetList[i];
                using (BinaryObjectReader P5FTDFile = new(shopftd, Endianness.Big, Encoding.GetEncoding(932)))
                {
                    shopValues = "";
                    P5FTDFile.AtOffset(offset);

                    for (int j = 0; j < structSize; j++)
                    {
                        if (j == structSize - 1)
                        {
                            shopByteString = $"{P5FTDFile.ReadByte():X2}";
                        }
                        else
                        {
                            shopByteString = $"{P5FTDFile.ReadByte():X2} ";
                        }
                        shopValues += shopByteString;
                    }
                }

                string bpString = CreateBPString(shopValues, offset, ftdName);
                binaryPatch.WriteLine(bpString);
            }

            string endBP = "  ]\n}";
            binaryPatch.WriteLine(endBP);
        }

        private static List<int> GetChangeOffsets(int structSize, int gameVersionIndex, string ftdName, int mode)
        {
            string gameVersion;
            string tempFile;

            if (gameVersionIndex == 0)
            {
                gameVersion = "Royal";
                tempFile = tempFileR;
            }
            else
            {
                gameVersion = "Vanilla";
                tempFile = tempFileV;
            }

            string shopftdOriginal = Directory.GetCurrentDirectory() + $"\\Original\\{gameVersion}\\{ftdName}.ftd";
            string shopftdOutput = Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\{ftdName}.ftd";

            if (structSize == 40)
            {
                shopftdOutput = tempFile;
                if (mode == 1)
                {
                    shopftdOriginal = Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\{ftdName}.ftd";
                }
            }

            uint entryCount = FtdParse.FtdRead(shopftdOriginal);
            uint entryCountOutput = FtdParse.FtdRead(shopftdOutput);

            List<int> changeOffsets = new();

            if (entryCount != entryCountOutput)
            {
                changeOffsets.Add(0);
                return changeOffsets;
            }

            List<int> originalValues = new();
            List<int> outputValues = new();

            for (uint i = 0; i < entryCount; i++)
            {
                originalValues.Clear();
                outputValues.Clear();

                long offset = 48 + (i * structSize);

                using (BinaryObjectReader OriginalP5FTDFile = new(shopftdOriginal, Endianness.Big, Encoding.GetEncoding(932)))
                {
                    OriginalP5FTDFile.AtOffset(offset);
                    for (int j = 0; j < structSize; j++)
                    {
                        originalValues.Add(OriginalP5FTDFile.ReadByte());
                    }
                }

                using (BinaryObjectReader OutputP5FTDFile = new(shopftdOutput, Endianness.Big, Encoding.GetEncoding(932)))
                {
                    OutputP5FTDFile.AtOffset(offset);
                    for (int j = 0; j < structSize; j++)
                    {
                        outputValues.Add(OutputP5FTDFile.ReadByte());
                    }
                }

                if (!Enumerable.SequenceEqual(originalValues, outputValues))
                {
                    changeOffsets.Add((int)offset);
                }
            }
            return changeOffsets;
        }
        private static string CreateBPString(string shopValueString, int offset, string ftdName)
        {
            string bpString = $"    {{\n      \"file\": \"init\\\\facility\\\\fclTable\\\\{ftdName}.ftd\",\n      \"offset\": {offset},\n      \"data\": \"{shopValueString}\"\n    }},";

            return bpString;
        }
        private void CreateDirectories()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            bool directoryCreated = false;

            if (!Directory.Exists($"{currentDirectory}\\Original\\Royal"))
            {
                Directory.CreateDirectory($"{currentDirectory}\\Original\\Royal");
                directoryCreated = true;
            }

            if (!Directory.Exists($"{currentDirectory}\\Output\\Royal"))
            {
                Directory.CreateDirectory($"{currentDirectory}\\Output\\Royal");
                directoryCreated = true;
            }

            if (!Directory.Exists($"{currentDirectory}\\Original\\Vanilla"))
            {
                Directory.CreateDirectory($"{currentDirectory}\\Original\\Vanilla");
                directoryCreated = true;
            }

            if (!Directory.Exists($"{currentDirectory}\\Output\\Vanilla"))
            {
                Directory.CreateDirectory($"{currentDirectory}\\Output\\Vanilla");
                directoryCreated = true;
            }

            if (directoryCreated)
            {
                ShowMessage("New Directories Created!");
            }
        }
        private int GetItemNum()
        {
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            string tempFile;

            if (gameVersionIndex == 0)
            {
                tempFile = tempFileR;
            }
            else
            {
                tempFile = tempFileV;
            }

            string shopItemftd = tempFile;

            List<int> shopOffsets = FtdParse.FindShopOffsetsandCount(shopItemftd, "ShopOffsets");

            int shopID = Convert.ToInt32(ShopIDTextBox.Text);
            int shopItemIndex = ItemSelectionComboBox.SelectedIndex;

            int itemOffset = shopOffsets[shopID] + ((shopItemIndex * 40) + 2);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (BinaryObjectReader P5FTDFile = new(shopItemftd, Endianness.Big, Encoding.GetEncoding(932)))
            {
                P5FTDFile.AtOffset(itemOffset);
                ushort itemIndex = P5FTDFile.ReadUInt16();

                int itemCategory = itemIndex / 0x1000;
                int itemID = itemIndex - (itemCategory * 0x1000);

                return itemID;
            }
        }

        private bool AreEntryCountsEqual(int mode) //0 for original, 1 for output
        {
            string gameVersion;
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            string tempFile;

            if (gameVersionIndex == 0)
            {
                gameVersion = "Royal";
                tempFile = tempFileR;
            }
            else
            {
                gameVersion = "Vanilla";
                tempFile = tempFileV;
            }

            string shopItemftd = (Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopItemTable.ftd");

            if (!File.Exists(shopItemftd) || mode == 0)
            {
                shopItemftd = (Directory.GetCurrentDirectory() + $"\\Original\\{gameVersion}\\fclPublicShopItemTable.ftd");
            }

            List<int> outputOffsets = FtdParse.FindShopOffsetsandCount(tempFile, "ShopOffsets");
            List<int> originalOffsets = FtdParse.FindShopOffsetsandCount(shopItemftd, "ShopOffsets");

            return (new FileInfo(tempFile).Length == new FileInfo(shopItemftd).Length) && Enumerable.SequenceEqual(outputOffsets, originalOffsets);
        }

        private bool CheckForChanges() //true if changes were made, false if none
        {
            return CheckItemChanges() || CheckNameChanges() == false;
        }

        private bool CheckNameChanges() //returns true if changes were made, false if not
        {
            string gameVersion;
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;
            int nameLength;

            if (gameVersionIndex == 0)
            {
                gameVersion = "Royal";
                nameLength = 48;
            }
            else
            {
                gameVersion = "Vanilla";
                nameLength = 32;
            }

            string shopNameftd = (Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopName.ftd");

            int shopID = ShopSelectionComboBox.SelectedIndex;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (BinaryObjectReader P5FTDFile = new (shopNameftd, Endianness.Big, Encoding.GetEncoding(932)))
            {
                P5FTDFile.AtOffset(48 + (shopID * nameLength));
                string originalName = P5FTDFile.ReadString(StringBinaryFormat.FixedLength, nameLength);
                return originalName == ShopNameTextBox.Text;
            }
        }

        private bool CheckItemChanges() //returns true if changes were made, false if not
        {
            string tempFile = GetTempFile();

            List<int> shopOffsets = FtdParse.FindShopOffsetsandCount(tempFile, "ShopOffsets");

            int shopID = ShopSelectionComboBox.SelectedIndex;
            int shopItemIndex = ItemSelectionComboBox.SelectedIndex;

            int itemOffset = shopOffsets[shopID] + (shopItemIndex * 40);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (BinaryObjectReader P5FTDFile = new (tempFile, Endianness.Big, Encoding.GetEncoding(932)))
            {
                P5FTDFile.AtOffset(itemOffset);
                byte field0 = P5FTDFile.ReadByte(); //field0
                bool unlimQuantity = field0 == 0xff;
                unlimQuantity = unlimQuantity == UnlimitedQuantityCheckBox.IsChecked;

                P5FTDFile.ReadByte(); //field1

                ushort item = (ushort)((ItemCategoryComboBox.SelectedIndex * 0x1000) + ItemIDComboBox.SelectedIndex);
                ushort ftditem = P5FTDFile.ReadUInt16();
                bool itemID = ftditem == item; //item id

                bool amountPerUnit = P5FTDFile.ReadByte() == Convert.ToByte(AmountPerUnitTextBox.Text); //Amount acquired per unit purchased

                bool startMonth = P5FTDFile.ReadByte() == StartMonthBox.SelectedIndex + 1; //start month

                bool startDay = P5FTDFile.ReadByte() == Convert.ToByte(StartDayTextBox.Text); //start day

                bool endMonth = P5FTDFile.ReadByte() == EndMonthBox.SelectedIndex + 1; //end month

                bool endDay = P5FTDFile.ReadByte() == Convert.ToByte(EndDayTextBox.Text); //end day

                P5FTDFile.ReadByte();
                bool quantity = P5FTDFile.ReadByte() == Convert.ToByte(QuantityTextBox.Text); //quantity
                P5FTDFile.ReadByte();

                P5FTDFile.ReadInt32();
                P5FTDFile.ReadInt32();
                P5FTDFile.ReadInt32();

                bool bitflagRequirement;
                if (GameVersionComboBox.SelectedIndex == 0)
                {
                    short bitflagSection = P5FTDFile.ReadInt16();
                    short bitflag = P5FTDFile.ReadInt16();
                    int convertedFlag = FtdParse.SumRoyalFlag(bitflagSection, bitflag);
                    bitflagRequirement = convertedFlag == Convert.ToInt32(RequiredBitflagTextBox.Text); //Royal Bitflag
                }
                else
                {
                    bitflagRequirement = P5FTDFile.ReadInt32() == Convert.ToInt32(RequiredBitflagTextBox.Text); //Vanilla Bitflag
                }

                P5FTDFile.ReadInt32();

                bool pricePercentage = P5FTDFile.ReadInt32() == Convert.ToInt32(PricePercentageTextBox.Text); //Price Percentage

                P5FTDFile.ReadInt32();
                //Console.WriteLine((unlimQuantity + " " + itemID + " " + amountPerUnit + " " + startMonth + " " + startDay + " " + endMonth + " " + endDay + " " + quantity + " " + bitflagRequirement + " " + pricePercentage));
                if (!unlimQuantity || !itemID || !amountPerUnit || !startMonth || !startDay || !endMonth || !endDay || !quantity || !bitflagRequirement || !pricePercentage) return true;
                else return false;
            }
        }

        private void CreateTempFiles()
        {
            tempFileV = System.IO.Path.GetTempFileName();
            tempFileR = System.IO.Path.GetTempFileName();
            tempNameV = System.IO.Path.GetTempFileName();
            tempNameR = System.IO.Path.GetTempFileName();

            string tempFilePath = System.IO.Path.GetDirectoryName(tempFileV);

            File.Move(tempFileV, tempFilePath + "\\TempShopV.ftd", true);
            File.Move(tempFileR, tempFilePath + "\\TempShopR.ftd", true);
            File.Move(tempNameV, tempFilePath + "\\TempNameV.ftd", true);
            File.Move(tempNameR, tempFilePath + "\\TempNameR.ftd", true);

            tempFileV = tempFilePath + "\\TempShopV.ftd";
            tempFileR = tempFilePath + "\\TempShopR.ftd";
            tempNameV = tempFilePath + "\\TempNameV.ftd";
            tempNameR = tempFilePath + "\\TempNameR.ftd";
        }

        private string GetTempFile()
        {
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            if (gameVersionIndex == 0)
            {
                return tempFileR;
            }

            return tempFileV;
        }

        private void ShowMessage(string message)
        {
            MessageTextBox.Text = message;
        }
    }  
}
