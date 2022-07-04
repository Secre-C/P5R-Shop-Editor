using Amicitia.IO.Binary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Shop_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool windowShowFirstTime = true;
        public static string tempShopV = "";
        public static string tempShopR = "";
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
            PopulateItemValues();
            ResetAllFields();
            windowShowFirstTime = false;
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
            return int.TryParse(str, out int i) && i >= -1 && i <= 2147483647;
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

        private void KofiLink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://ko-fi.com/secrec9802") { UseShellExecute = true });
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;
            string gameVersion;
            string tempFile;
            string tempName;
            int nameLength;
            int original = 0;
            int output = 1;
            bool filesReset = false;

            if (gameVersionIndex == 0)
            {
                gameVersion = "Royal";
                tempFile = tempShopR;
                tempName = tempNameR;
                nameLength = 48;
            }
            else
            {
                gameVersion = "Vanilla";
                tempFile = tempShopV;
                tempName = tempNameV;
                nameLength = 32;
            }

            string shopftd = Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopItemTable.ftd";
            string nameftd = Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopName.ftd";

            if (!File.Exists(shopftd))
            {
                shopftd = Directory.GetCurrentDirectory() + $"\\Original\\{gameVersion}\\fclPublicShopItemTable.ftd";
            }

            if (!File.Exists(nameftd))
            {
                nameftd = Directory.GetCurrentDirectory() + $"\\Original\\{gameVersion}\\fclPublicShopName.ftd";
            }

            if (!AreFilesTheSame(40, output))
            {
                File.Copy(shopftd, tempFile, true);
                filesReset = true;
            }

            if (!AreFilesTheSame(nameLength, output))
            {
                File.Copy(nameftd, tempName, true);
                filesReset = true;
            }

            if (filesReset)
            {
                ResetAllFields();
                ShowMessage("Unsaved Changes have been reverted!");
            }
            else
            {
                ShowMessage("No changes have been made!");
            }
        }

        private void ResetAllButton_Click(object sender, RoutedEventArgs e)
        {
            string gameVersion;
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;
            bool itemsReset = false;
            string tempFile;
            string tempName;
            int nameLength;

            if (gameVersionIndex == 0)
            {
                gameVersion = "Royal";
                tempFile = tempShopR;
                tempName = tempNameR;
                nameLength = 48;
            }
            else
            {
                gameVersion = "Vanilla";
                tempFile = tempShopV;
                tempName = tempNameV;
                nameLength = 32;
            }

            string shopftd = Directory.GetCurrentDirectory() + $"\\Original\\{gameVersion}\\fclPublicShopItemTable.ftd";
            string nameftd = Directory.GetCurrentDirectory() + $"\\Original\\{gameVersion}\\fclPublicShopName.ftd";

            if (!AreFilesTheSame(40, 0))
            {
                File.Copy(shopftd, tempFile, true);
                itemsReset = true;
            }

            if (!AreFilesTheSame(nameLength, 0))
            {
                File.Copy(nameftd, tempName, true);
                itemsReset = true;
            }

            if (itemsReset)
            {
                ResetAllFields();
                ShowMessage($"All items have been reverted to their original state!");
            }
            else
            {
                ShowMessage("No changes have been made!");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            int original = 0;
            int output = 1;
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;
            bool changesSaved = false;
            bool bpWritten = false;
            int nameLength = 48;

            string tempFile = tempShopR;
            string tempName = tempNameR;
            string gameVersion = "Royal";
            
            if (gameVersionIndex == 1)
            {
                nameLength = 32;
                tempFile = tempShopV;
                tempName = tempNameV;
                gameVersion = "Vanilla";
            }

            if (!AreFilesTheSame(40, output))
            {
                File.Copy(tempFile, Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopItemTable.ftd", true);

                if (AreEntryCountsEqual(original) && !AreFilesTheSame(40, original))
                {
                    WriteBinaryPatch(40);
                    bpWritten = true;
                }
                else
                {
                    if (File.Exists(Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\Shop_Items.bp"))
                    {                                               
                        File.Delete(Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\Shop_Items.bp");
                    }
                }
                changesSaved = true;
            }

            if (!AreFilesTheSame(nameLength, output))
            {
                File.Copy(tempName, Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopName.ftd", true);

                if (!AreFilesTheSame(nameLength, original))
                {
                    WriteBinaryPatch(nameLength);
                    bpWritten = true;
                }
                else
                {
                    if(File.Exists(Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\Shop_Names.bp"))
                    {
                        File.Delete(Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\Shop_Names.bp");
                    }
                }
                changesSaved = true;
            }

            if (bpWritten)
            {
                ShowMessage($"Created Binary Patch file (.bp) and saved changes to 'Output\\{gameVersion}'!");
                ResetAllFields();
            }
            else if (changesSaved)
            {
                ShowMessage($"Changes Saved to 'Output\\{gameVersion}'!");
                ResetAllFields();
            }
            else
            {
                ShowMessage("No new changes have been made!");
            }
        } 

        private void AddNewItemButton_Click(object sender, RoutedEventArgs e)
        {
            AddItemSlot();

            ItemSelectionComboBox.SelectedIndex += 1;

            PopulateItemValues();
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

            PopulateItemValues();

            PopulateShopInformation();
            PopulateShopItemComboBox();

            ShowMessage(itemDeleteMessage);
        }
        private void ItemSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (windowShowFirstTime)
            {
                return;
            }

            Console.WriteLine("ItemSelectionComboBox_SelectionChanged");
            PopulateItemValues();
        }

        private void ItemCategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (windowShowFirstTime)
            {
                return;
            }

            windowShowFirstTime = true;
            Console.WriteLine("ItemCategoryComboBox_SelectionChanged");
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
            windowShowFirstTime = false;
            byte categoryByte = (byte)((ItemCategoryComboBox.SelectedIndex * 0x1000 + ItemIDComboBox.SelectedIndex) / 0x100);
            byte[] shopItemBytes = { categoryByte };
            WriteChangestoTemp(2, 40, shopItemBytes);

            PopulateItemIdComboBox();
        }
        private void ItemIDComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (windowShowFirstTime)
            {
                return;
            }

            Console.WriteLine("ItemIDComboBox_SelectionChanged");
            short itemID = (short)((ItemCategoryComboBox.SelectedIndex * 0x1000) + ItemIDComboBox.SelectedIndex);
            byte[] shopItemBytes = BitConverter.GetBytes(itemID);
            WriteChangestoTemp(2, 40, shopItemBytes);
            PopulateShopItemComboBox();
        }

        private void GameVersionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (windowShowFirstTime) //prevent function from running at launch
            {
                return;
            }
            Console.WriteLine("GameVersionComboBox_SelectionChanged");

            string gameVersion;
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            string tempFile;

            if (gameVersionIndex == 0)
            {
                gameVersion = "Royal";
                tempFile = tempShopR;
            }
            else
            {
                gameVersion = "Vanilla";
                tempFile = tempShopV;
            }

            if (!File.Exists(Directory.GetCurrentDirectory() + $"\\Original\\{gameVersion}\\fclPublicShopItemTable.ftd") || !File.Exists(Directory.GetCurrentDirectory() + $"\\Original\\{gameVersion}\\fclPublicShopName.ftd"))
            {
                string missingftdMessage = "Missing Ftds! Make sure both fclPublicShopItemTable.ftd and fclPublicShopName.ftd have been copied to the Original\\{gameVersion} Folder!";
                MessageBox.Show(missingftdMessage, "Missing Ftds");
                if (gameVersion == "Royal")
                {
                    gameVersionIndex = 1;
                }
                else
                {
                    gameVersionIndex = 0;
                }

                GameVersionComboBox.SelectedIndex = gameVersionIndex;
                return;
            }

            List<int> shopOffsets = FtdParse.FindShopOffsetsandCount(tempFile, "ShopOffsets");

            if (shopOffsets.Count < ShopSelectionComboBox.SelectedIndex)
            {
                ShopSelectionComboBox.SelectedIndex = shopOffsets.Count - 1;
            }

            //change shop name character limit
            if (GameVersionComboBox.SelectedIndex == 0)
            {
                ShopNameTextBox.MaxLength = 48;
            }
            else
            {
                ShopNameTextBox.MaxLength = 32;
            }

            PopulateShopNameComboBox();

            //reread shop values
            ShopSelectionComboBox_SelectionChanged(null, null);

            PopulateItemIdComboBox();

            return;
        }

        private void StartMonthBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (windowShowFirstTime)
            {
                return;
            }

            Console.WriteLine("StartMonthBox_SelectionChanged");
            var monthIndex = StartMonthBox.SelectedIndex;
            Console.WriteLine($"month {monthIndex}");
            int[] dayArray = GetDayArray();

            if (StartDayTextBox.Text == "")
            {
                StartDayTextBox.Text = "1";
            }

            if (monthIndex == -1) monthIndex = 0;

            if (dayArray[monthIndex] < Convert.ToInt32(StartDayTextBox.Text))
            {
                StartDayTextBox.Text = Convert.ToString(dayArray[monthIndex]);
            }

            byte[] startMonthByte = { (byte)(StartMonthBox.SelectedIndex + 1) };
            WriteChangestoTemp(5, 40, startMonthByte);
        }
        private void EndMonthBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (windowShowFirstTime)
            {
                return;
            }

            Console.WriteLine("EndMonthBox_SelectionChanged");
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

            byte[] endMonthByte = { (byte)(EndMonthBox.SelectedIndex + 1) };
            WriteChangestoTemp(7, 40, endMonthByte);
        }
        private void StartDayTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (windowShowFirstTime)
            {
                return;
            }

            string startDay = StartDayTextBox.Text;

            if (startDay == "") return;

            byte[] startDayByte = { Convert.ToByte(startDay) };
            WriteChangestoTemp(6, 40, startDayByte);
        }

        private void EndDayTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (windowShowFirstTime)
            {
                return;
            }

            string endDay = EndDayTextBox.Text;

            if (endDay == "") return;

            byte[] endDayByte = { Convert.ToByte(endDay) };
            WriteChangestoTemp(8, 40, endDayByte);
        }

        private void QuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (windowShowFirstTime)
            {
                return;
            }

            string quantity = QuantityTextBox.Text;
            if (quantity == "") return;
            byte[] quantityByte = { Convert.ToByte(quantity) };
            WriteChangestoTemp(10, 40, quantityByte);
        }

        private void PricePercentageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (windowShowFirstTime)
            {
                return;
            }

            string percentage = PricePercentageTextBox.Text;
            if (percentage == "") return;
            byte[] percentageInt = BitConverter.GetBytes(Convert.ToInt32(percentage));
            WriteChangestoTemp(32, 40, percentageInt);
        }

        private void AmountPerUnitTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (windowShowFirstTime)
            {
                return;
            }

            string amountPerUnit = AmountPerUnitTextBox.Text;
            if (amountPerUnit == "") return;
            byte[] amountPerUnitByte = { Convert.ToByte(amountPerUnit) };
            WriteChangestoTemp(4, 40, amountPerUnitByte);
        }

        private void RequiredBitflagTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (windowShowFirstTime)
            {
                return;
            }

            string bitflag = RequiredBitflagTextBox.Text;
            if (bitflag == "" || (bitflag.Contains('-') && bitflag != "-1")) return;
            byte[] bitflagInt = BitConverter.GetBytes(Convert.ToInt32(bitflag));
            WriteChangestoTemp(24, 40, bitflagInt);
        }

        private void ShopSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (windowShowFirstTime) //prevent function from running at launch
            {
                return;
            }

            Console.WriteLine("ShopSelectionComboBox_SelectionChanged");
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            string tempFile;

            if (gameVersionIndex == 0)
            {
                tempFile = tempShopR;
            }
            else
            {
                tempFile = tempShopV;
            }

            string shopItemftd = tempFile;

            int shopIndex = ShopSelectionComboBox.SelectedIndex;

            if (shopIndex == -1) return;

            List<int> itemCount = FtdParse.FindShopOffsetsandCount(shopItemftd, "ItemCount");

            NumOfItemsTextBox.Text = itemCount[shopIndex].ToString();
            ShopNameTextBox.Text = NameParse.PrintShopName(shopIndex, GameVersionComboBox.SelectedIndex);
            ShopIDTextBox.Text = ShopSelectionComboBox.SelectedIndex.ToString();

            List<string> ShopItemList = ItemParse.MakeShopItemList(shopIndex, GameVersionComboBox.SelectedIndex);
            ItemSelectionComboBox.ItemsSource = ShopItemList;
            ItemSelectionComboBox.SelectedIndex = 0;
        }

        private void ShopNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Console.WriteLine(sender);
            if (windowShowFirstTime) //prevent function from running at launch
            {
                return;
            }
            int structSize = 32;

            if (GameVersionComboBox.SelectedIndex == 0)
            {
                structSize = 48;
            }

            byte[] nameBytes = Encoding.ASCII.GetBytes(ShopNameTextBox.Text);
            Array.Reverse(nameBytes);
            WriteChangestoTemp(0, structSize, nameBytes);
            PopulateShopNameComboBox();
        }

        private void UnlimitedQuantity_Checked(object sender, RoutedEventArgs e)
        {
            if (windowShowFirstTime)
            {
                return;
            }

            var check = (CheckBox)sender;
            if (check.IsChecked == true)
            {
                QuantityTextBox.IsReadOnly = true;
                QuantityTextBox.Background = new SolidColorBrush(Color.FromArgb(0xff, 0x20, 0x20, 0x20));
                byte[] unlimitedItems = { 0xff };
                WriteChangestoTemp(0, 40, unlimitedItems);
            }
            else
            {
                QuantityTextBox.IsReadOnly = false;
                QuantityTextBox.Background = new SolidColorBrush(Color.FromArgb(0xff, 0x2a, 0x2a, 0x2a));
                byte[] unlimitedItems = { 0x0 };
                WriteChangestoTemp(0, 40, unlimitedItems);
            }
        }

        private void PopulateShopNameComboBox()
        {
            Console.WriteLine("PopulateShopNameComboBox");
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            List<string> ShopNameList = NameParse.MakeShopNameList(gameVersionIndex);

            windowShowFirstTime = true;
            var shopIndex = ShopSelectionComboBox.SelectedIndex;
            ShopSelectionComboBox.ItemsSource = ShopNameList;

            if (shopIndex == -1)
            {
                shopIndex = 0;
            }

            ShopSelectionComboBox.SelectedIndex = shopIndex;
            ShopNameTextBox.Text = NameParse.PrintShopName(shopIndex, GameVersionComboBox.SelectedIndex);
            windowShowFirstTime = false;
        }

        private void PopulateShopInformation()
        {
            Console.WriteLine("PopulateShopInformation");
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            string tempFile;

            if (gameVersionIndex == 0)
            {
                tempFile = tempShopR;
            }
            else
            {
                tempFile = tempShopV;
            }

            string shopItemftd = tempFile;

            var shopIndex = ShopSelectionComboBox.SelectedIndex;

            List<int> itemCount = FtdParse.FindShopOffsetsandCount(shopItemftd, "ItemCount");


            NumOfItemsTextBox.Text = itemCount[shopIndex].ToString();
            ShopIDTextBox.Text = ShopSelectionComboBox.SelectedIndex.ToString();
        }
        private void PopulateShopItemComboBox()
        {
            Console.WriteLine("PopulateShopItemComboBox");
            int shopID = ShopSelectionComboBox.SelectedIndex;
            
            if (shopID == -1) shopID = 0;

            List<string> ShopItemList = ItemParse.MakeShopItemList(shopID, GameVersionComboBox.SelectedIndex);

            int prevIndex = ItemSelectionComboBox.SelectedIndex;
            if (prevIndex == -1) prevIndex = 0;

            windowShowFirstTime = true;
            ItemSelectionComboBox.ItemsSource = ShopItemList;
            ItemSelectionComboBox.SelectedIndex = prevIndex;
            windowShowFirstTime = false;
        }

        private void PopulateItemValues()
        {
            Console.WriteLine("PopulateItemValues");
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            string tempFile;

            if (gameVersionIndex == 0)
            {
                tempFile = tempShopR;
            }
            else
            {
                tempFile = tempShopV;
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
            using (BinaryObjectReader P5FTDFile = new(shopItemftd, Endianness.Big, Encoding.GetEncoding(932)))
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
        private void PopulateItemIdComboBox()
        {
            Console.WriteLine("PopulateItemIdComboBox");
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

            int prevItem = ItemIDComboBox.SelectedIndex;

            if (prevItem == -1) prevItem = 0;

            windowShowFirstTime = true;
            ItemIDComboBox.ItemsSource = numberedItemList;
            windowShowFirstTime = false;
            ItemIDComboBox.SelectedIndex = prevItem;
        }

        public static int[] GetDayArray()
        {
            int[] dayArray = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            return dayArray;
        }
        private void ResetAllFields()
        {
            int prevItem = ItemSelectionComboBox.SelectedIndex;
            ShopSelectionComboBox_SelectionChanged(null, null);
            PopulateItemValues();
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
                    if (new FileInfo(tempShopR).Length == 0)
                    {
                        File.Copy(Directory.GetCurrentDirectory() + "\\Output\\Royal\\fclPublicShopItemTable.ftd", tempShopR, true);
                    }
                }
                else
                {
                    if (new FileInfo(tempShopR).Length == 0)
                    {
                        File.Copy(Directory.GetCurrentDirectory() + "\\Original\\Royal\\fclPublicShopItemTable.ftd", tempShopR, true);
                    }
                }

                if (File.Exists(Directory.GetCurrentDirectory() + "\\Output\\Royal\\fclPublicShopName.ftd"))
                {
                    if (new FileInfo(tempNameR).Length == 0)
                    {
                        File.Copy(Directory.GetCurrentDirectory() + "\\Output\\Royal\\fclPublicShopName.ftd", tempNameR, true);
                    }
                }
                else
                {
                    if (new FileInfo(tempNameR).Length == 0)
                    {
                        File.Copy(Directory.GetCurrentDirectory() + "\\Original\\Royal\\fclPublicShopName.ftd", tempNameR, true);
                    }
                }
            }

            if (File.Exists(Directory.GetCurrentDirectory() + "\\Original\\Vanilla\\fclPublicShopItemTable.ftd") 
                && File.Exists(Directory.GetCurrentDirectory() + "\\Original\\Vanilla\\fclPublicShopName.ftd"))
            {
                if (File.Exists(Directory.GetCurrentDirectory() + "\\Output\\Vanilla\\fclPublicShopItemTable.ftd"))
                {
                    if (new FileInfo(tempShopV).Length == 0)
                    {
                        File.Copy(Directory.GetCurrentDirectory() + "\\Output\\Vanilla\\fclPublicShopItemTable.ftd", tempShopV, true);
                    }
                }
                else
                {
                    if (new FileInfo(tempShopV).Length == 0)
                    {
                        File.Copy(Directory.GetCurrentDirectory() + "\\Original\\Vanilla\\fclPublicShopItemTable.ftd", tempShopV, true);
                    }
                }

                if (File.Exists(Directory.GetCurrentDirectory() + "\\Output\\Vanilla\\fclPublicShopName.ftd"))
                {
                    if (new FileInfo(tempNameV).Length == 0)
                    {
                        File.Copy(Directory.GetCurrentDirectory() + "\\Output\\Vanilla\\fclPublicShopName.ftd", tempNameV, true);
                    }
                }
                else
                {
                    if (new FileInfo(tempNameV).Length == 0)
                    {
                        File.Copy(Directory.GetCurrentDirectory() + "\\Original\\Vanilla\\fclPublicShopName.ftd", tempNameV, true);
                    }
                }
            }

            if (GameVersionComboBox.SelectedIndex == 0)
            {
                ShopNameTextBox.MaxLength = 48;
            }
            else
            {
                ShopNameTextBox.MaxLength = 32;
            }
        }

        private void AddItemSlot()
        {
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;
            string tempFile;

            if (gameVersionIndex == 0)
            {
                tempFile = tempShopR;
            }
            else
            {
                tempFile = tempShopV;
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
                tempFile = tempShopR;
            }
            else
            {
                tempFile = tempShopV;
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

        private bool AreFilesTheSame(int structSize, int mode) //0 for original, 1 for output
        {
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;
            
            string ftdName = "fclPublicShopItemTable";

            if (structSize == 32 || structSize == 48)
            {
                ftdName = "fclPublicShopName";
            }

            if (!AreEntryCountsEqual(mode))
            {
                return false;
            }

            List<int> changeOffsetList = GetChangeOffsets(structSize, gameVersionIndex, ftdName, mode);

            if (changeOffsetList.Count > 0)
            {
                return false;
            }

            return true;
        }

        private void WriteBinaryPatch(int structSize)
        {
            string shopByteString;
            string shopValues;
            string gameVersion;
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            if (gameVersionIndex == 0)
            {
                gameVersion = "Royal";
            }
            else
            {
                gameVersion = "Vanilla";
            }

            string ftdName = "fclPublicShopItemTable";
            string bpName = (Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\Shop_Items.bp");
            string shopftd = (Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopItemTable.ftd");

            if (structSize == 32 || structSize == 48)
            {
                ftdName = "fclPublicShopName";
                bpName = (Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\Shop_Names.bp");
                shopftd = (Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopName.ftd");
            }

            List<int> changeOffsetList = GetChangeOffsets(structSize, gameVersionIndex, ftdName, 2);

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
                tempFile = tempShopR;
            }
            else
            {
                gameVersion = "Vanilla";
                tempFile = tempShopV;
            }

            if (structSize == 48 || structSize == 32)
            {
                if (gameVersionIndex == 0)
                {
                    tempFile = tempNameR;
                }
                else
                {
                    tempFile = tempNameV;
                }
            }

            string shopftdOriginal = Directory.GetCurrentDirectory() + $"\\Original\\{gameVersion}\\{ftdName}.ftd";
            string shopftdOutput = Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\{ftdName}.ftd";

            if (mode != 2)
            {
                shopftdOutput = tempFile;
            }

            if (mode == 1 && File.Exists(Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\{ftdName}.ftd"))
            {
                shopftdOriginal = Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\{ftdName}.ftd";
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
        private static void CreateDirectories()
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
                MessageBox.Show("New Directories Created!");
            }
        }

        private bool AreEntryCountsEqual(int mode) //0 for original, 1 for output. returns false if entry counts are not equal, or if any shop has a different amount of items than og
        {
            string gameVersion;
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            string tempFile;

            if (gameVersionIndex == 0)
            {
                gameVersion = "Royal";
                tempFile = tempShopR;
            }
            else
            {
                gameVersion = "Vanilla";
                tempFile = tempShopV;
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

        private void WriteChangestoTemp(int offset, int structSize, byte[] newBytes)
        {
            int shopID = ShopSelectionComboBox.SelectedIndex;
            int shopItemIndex = ItemSelectionComboBox.SelectedIndex;
            int itemOffset;

            if (windowShowFirstTime || shopID == -1)
            {
                return;
            }
            Array.Reverse(newBytes);
            string tempFile = tempShopV;
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;
            int byteArrayLength = newBytes.Length;

            if (structSize != 40)
            {
                tempFile = tempNameV;
                byteArrayLength = 32;
            }

            if (gameVersionIndex == 0)
            {
                tempFile = tempShopR;
                if (structSize != 40)
                {
                    tempFile = tempNameR;
                    byteArrayLength = 48;
                }
            }

            List<int> shopOffsets = FtdParse.FindShopOffsetsandCount(tempFile, "ShopOffsets");

            if (structSize != 40)
            {
                itemOffset = 48 + (shopID * structSize);
            }
            else
            {
                itemOffset = shopOffsets[shopID] + (shopItemIndex * 40);
            }


            itemOffset += offset;

            ReplaceBytes(tempFile, itemOffset, newBytes, byteArrayLength);

            string newName = System.Text.Encoding.UTF8.GetString(newBytes, 0, newBytes.Length);

            if (structSize != 40)
            {
                ShopNameTextBox.Text = newName;
            }
        }

        public static void ReplaceBytes(string filename, int position, byte[] data, int byteArrayLength)
        {
            using (Stream stream = File.Open(filename, FileMode.Open))
            {
                stream.Position = position;
                stream.Write(data, 0, data.Length);

                if (data.Length != byteArrayLength)
                {
                    int extraByteCount = byteArrayLength - data.Length;
                    for (int i = 0; i < extraByteCount; i++)
                    {
                        stream.WriteByte(0);
                    }
                }
            }
        }

        private static void CreateTempFiles()
        {
            string tempFilePath = System.IO.Path.GetTempPath();

            tempShopV = tempFilePath + "TempShopV.ftd";
            tempShopR = tempFilePath + "TempShopR.ftd";
            tempNameV = tempFilePath + "TempNameV.ftd";
            tempNameR = tempFilePath + "TempNameR.ftd";

            if (!File.Exists(tempShopV))
            {
                File.Move(System.IO.Path.GetTempFileName(), tempShopV);
            }

            if (!File.Exists(tempShopR))
            {
                File.Move(System.IO.Path.GetTempFileName(), tempShopR);
            }

            if (!File.Exists(tempNameV))
            {
                File.Move(System.IO.Path.GetTempFileName(), tempNameV);
            }

            if (!File.Exists(tempNameR))
            {
                File.Move(System.IO.Path.GetTempFileName(), tempNameR);
            }
        }

        private void ShowMessage(string message)
        {
            MessageTextBox.Text = message;
        }
    }  
}
