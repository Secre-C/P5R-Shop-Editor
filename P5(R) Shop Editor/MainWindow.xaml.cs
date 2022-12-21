using Amicitia.IO.Binary;
using Microsoft.Win32;
using ShopLibrary;
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
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Shop_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ShopItemFile ShopItemFile { get; set; }
        public ShopNameFile ShopNameFile { get; set; }
        public ShopDataFile ShopDataFile { get; set; }

        public List<string[]> FullItemList { get; set; }
        public List<string> ShopItemBoxList{ get; set; }
        public int SelectedShop { get; set; } = -1;
        public int SelectedItem { get; set; } = -1;

        public static string tempShopV;
        public static string tempShopR;
        public static string tempNameV;
        public static string tempNameR;

        public MainWindow()
        {
            InitializeComponent();
            CreateTempFiles();
            CreateDirectories();
            CheckForFtdsOnStartup();

            new ShopItemFile().Read(tempShopR, out var shopItemFile);
            ShopItemFile = shopItemFile;

            new ShopNameFile().Read(tempNameR, out var shopNameFile);
            ShopNameFile = shopNameFile;

            //new ShopDataFile().Read(tempShopR, out var shopDataFile);
            //ShopItemFile = shopItemFile;

            FullItemList = ItemCategories.GetItemList("P5R");

            StartupPopulate();
            //ResetAllFields();
        }
        private void StartupPopulate()
        {
            PopulateShopNameComboBox();
        }
        private void ClearAllFields()
        {
            ItemSelectionComboBox.SelectedIndex  = -1;

            ItemCategoryComboBox.SelectedIndex  = -1;
            ItemIDComboBox.SelectedIndex        = -1;
            AmountPerUnitTextBox.Text           = "";
            StartMonthBox.SelectedIndex         = -1;
            StartDayTextBox.Text                = "";
            EndMonthBox.SelectedIndex           = -1;
            EndDayTextBox.Text                  = "";
            QuantityTextBox.Text                = "";
            RequiredBitflagTextBox.Text         = "";
            PricePercentageTextBox.Text         = "";
        }
        private void StartDayValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !TextValidation.IsDayValid(((TextBox)sender).Text + e.Text, StartMonthBox);
        }
        private void EndDayValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !TextValidation.IsDayValid(((TextBox)sender).Text + e.Text, EndMonthBox);
        }
        private void UByteValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !TextValidation.IsValidUByte(((TextBox)sender).Text + e.Text);
        }
        private void IntValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !TextValidation.IsValidInt(((TextBox)sender).Text + e.Text);
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
            var saveDialog = new CommonOpenFileDialog();
            saveDialog.IsFolderPicker = true;
            saveDialog.Title = "Select Directory to save files to.";

            if (saveDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                ShopItemFile.Write($"{saveDialog.FileName}/fclPublicShopItemTable.ftd", ShopItemFile);
                ShopNameFile.Write($"{saveDialog.FileName}/fclPublicShopName.ftd", ShopNameFile);
            }
        } 

        private void AddNewItemButton_Click(object sender, RoutedEventArgs e)
        {
            AddItemSlot();

            SelectedItem += 1;

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
            SelectedItem -= 1;

            PopulateItemValues();

            PopulateShopInformation();
            PopulateShopItemComboBox();

            ShowMessage(itemDeleteMessage);
        }
        private void ItemSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItem = ItemSelectionComboBox.SelectedIndex;

            if (SelectedItem == -1)
                return;

            DebugLog("ItemSelectionComboBox_SelectionChanged");

            PopulateItemValues();

            return;
        }

        private void ItemCategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedItem == -1)
                return;

            DebugLog("ItemCategoryComboBox_SelectionChanged");

            ShopItemFile.Shops[SelectedShop].Items[SelectedItem].ItemId.ItemCategory = (ushort)ItemCategoryComboBox.SelectedIndex;

            DebugLog($"ItemCategoryComboBox Category{ShopItemFile.Shops[SelectedShop].Items[SelectedItem].ItemId.ItemCategory}");

            PopulateItemIdComboBox();
        }
        private void ItemIDComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedItem == -1 || ItemIDComboBox.SelectedIndex == -1)
                return;

            DebugLog("ItemIDComboBox_SelectionChanged");

            ShopItemFile.Shops[SelectedShop].Items[SelectedItem].ItemId.ItemCategory = (ushort)ItemCategoryComboBox.SelectedIndex;
            ShopItemFile.Shops[SelectedShop].Items[SelectedItem].ItemId.ItemIndex = (ushort)ItemIDComboBox.SelectedIndex;

            DebugLog($"ItemIDCombobox Category {ItemCategoryComboBox.SelectedIndex}");
            DebugLog($"ItemIDCombobox Index {ItemIDComboBox.SelectedIndex}");

            PopulateShopItemComboBox();
        }

        private void GameVersionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            return;

            if (ItemCategoryComboBox.SelectedIndex == -1) return;

            DebugLog("GameVersionComboBox_SelectionChanged");

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
            if (SelectedItem == -1)
                return;

            DebugLog("StartMonthBox_SelectionChanged");

            ShopItemFile.Shops[SelectedShop].Items[SelectedItem].AvailabilityStartMonth = (byte)(StartMonthBox.SelectedIndex + 1);
        }
        private void EndMonthBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedItem == -1)
                return;

            DebugLog("EndMonthBox_SelectionChanged");

            ShopItemFile.Shops[SelectedShop].Items[SelectedItem].AvailabilityEndMonth = (byte)(EndMonthBox.SelectedIndex + 1);
        }
        private void StartDayTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectedItem == -1)
                return;

            DebugLog("StartDayTextBox_TextChanged");

            try
            {
                ShopItemFile.Shops[SelectedShop].Items[SelectedItem].AvailabilityStartDay = Convert.ToByte(StartDayTextBox.Text);
            }
            catch
            {
                DebugLog("Invalid number");
            }
        }

        private void EndDayTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectedItem == -1)
                return;

            DebugLog("EndDayTextBox_TextChanged");

            try
            {
                ShopItemFile.Shops[SelectedShop].Items[SelectedItem].AvailabilityEndDay = Convert.ToByte(EndDayTextBox.Text);
            }
            catch
            {
                DebugLog("Invalid number");
            }
        }

        private void QuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectedItem == -1)
                return;

            DebugLog("QuantityTextBox_TextChanged");

            try
            {
                ShopItemFile.Shops[SelectedShop].Items[SelectedItem].Quantity = Convert.ToByte(QuantityTextBox.Text);
                ShopItemFile.Shops[SelectedShop].Items[SelectedItem].Quantity2 = Convert.ToByte(QuantityTextBox.Text);
                ShopItemFile.Shops[SelectedShop].Items[SelectedItem].Quantity3 = Convert.ToByte(QuantityTextBox.Text);
            }
            catch
            {
                DebugLog("Invalid number");
            }
        }

        private void PricePercentageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectedItem == -1)
                return;

            DebugLog("PricePercentageTextBox_TextChanged");

            try
            {
                ShopItemFile.Shops[SelectedShop].Items[SelectedItem].PercentageOfPrice = Convert.ToInt32(PricePercentageTextBox.Text);
            }
            catch
            {
                DebugLog("Invalid number");
            }
        }

        private void AmountPerUnitTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectedItem == -1)
                return;

            DebugLog("AmountPerUnitTextBox_TextChanged");

            try
            {
                ShopItemFile.Shops[SelectedShop].Items[SelectedItem].AmountPerUnit = Convert.ToByte(AmountPerUnitTextBox.Text);
            }
            catch
            {
                DebugLog("Invalid number");
            }
        }

        private void RequiredBitflagTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectedItem == -1)
                return;

            DebugLog("RequiredBitflagTextBox_TextChanged");

            try
            {
                ShopItemFile.Shops[SelectedShop].Items[SelectedItem].Bitflag.ConvertedBitflag = Convert.ToInt32(RequiredBitflagTextBox.Text);
            }
            catch
            {
                DebugLog("Invalid number");
            }
        }

        private void ShopSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedShop = ShopSelectionComboBox.SelectedIndex;

            DebugLog("ShopSelectionComboBox_SelectionChanged");

            ClearAllFields();
            PopulateShopItemComboBox();

            ShopNameTextBox.Text = ShopNameFile.ShopNames[SelectedShop].Name;
            return;
        }

        private void ShopNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectedShop == -1)
                return;

            DebugLog("ShopNameTextBox_TextChanged");

            ShopNameFile.ShopNames[SelectedShop].Name = ShopNameTextBox.Text;
        }

        private void UnlimitedQuantity_Checked(object sender, RoutedEventArgs e)
        {
            if (SelectedItem == -1)
                return;

            DebugLog("UnlimitedQuantity_Checked");

            ShopItemFile.Shops[SelectedShop].Items[SelectedItem].ShopEndIndicator = (byte)(UnlimitedQuantityCheckBox.IsChecked == true ? 0xFF : 0);
        }

        private void PopulateShopNameComboBox()
        {
            DebugLog("PopulateShopNameComboBox");

            ShopSelectionComboBox.ItemsSource = new ShopComboBox(ShopNameFile).NameIndex;
        }

        private void PopulateShopInformation()
        {
            DebugLog("PopulateShopInformation");
            
            NumOfItemsTextBox.Text = ShopItemFile.Shops[ShopSelectionComboBox.SelectedIndex].Items.Count.ToString();
            ShopIDTextBox.Text = ShopSelectionComboBox.SelectedIndex.ToString();
        }
        private void PopulateShopItemComboBox()
        {
            DebugLog("PopulateShopItemComboBox");

            var storeItemSelection = ItemSelectionComboBox.SelectedIndex;

            DebugLog($"PopulateShopItemComboBox Category {ItemCategoryComboBox.SelectedIndex}");
            DebugLog($"PopulateShopItemComboBox Index {ItemIDComboBox.SelectedIndex}");

            ShopItemBoxList = new ShopItemListBox(ShopItemFile.Shops[SelectedShop], FullItemList).ItemEntries;
            ItemSelectionComboBox.ItemsSource = ShopItemBoxList;

            ItemSelectionComboBox.SelectedIndex = storeItemSelection;
        }

        private void PopulateItemValues()
        {
            if (SelectedItem == -1) return;

            DebugLog("PopulateItemValues");

            int shopInput = ShopSelectionComboBox.SelectedIndex;

            int shopItemIndex = ItemSelectionComboBox.SelectedIndex;

            var selectedShopItem = ShopItemFile.Shops[shopInput].Items[shopItemIndex];

            var ItemId = selectedShopItem.ItemId.ItemIndex;

            UnlimitedQuantityCheckBox.IsChecked = selectedShopItem.ShopEndIndicator == 0xFF ? true : false;
            ItemCategoryComboBox.SelectedIndex  = selectedShopItem.ItemId.ItemCategory;
            ItemIDComboBox.SelectedIndex        = ItemId;
            AmountPerUnitTextBox.Text           = selectedShopItem.AmountPerUnit.ToString();
            StartMonthBox.SelectedIndex         = selectedShopItem.AvailabilityStartMonth - 1;
            StartDayTextBox.Text                = selectedShopItem.AvailabilityStartDay.ToString();
            EndMonthBox.SelectedIndex           = selectedShopItem.AvailabilityEndMonth - 1;
            EndDayTextBox.Text                  = selectedShopItem.AvailabilityEndDay.ToString();
            QuantityTextBox.Text                = selectedShopItem.Quantity.ToString();
            RequiredBitflagTextBox.Text         = selectedShopItem.Bitflag.ConvertedBitflag.ToString();
            PricePercentageTextBox.Text         = selectedShopItem.PercentageOfPrice.ToString();

            DebugLog($"item {ItemIDComboBox.SelectedIndex}");
        }
        private void PopulateItemIdComboBox()
        {
            DebugLog("PopulateItemIdComboBox");

            if (ItemCategoryComboBox.SelectedIndex == -1) return;

            var prevItemSelection = ItemIDComboBox.SelectedIndex;

            if (prevItemSelection > FullItemList[ItemCategoryComboBox.SelectedIndex].Length)
            {
                prevItemSelection = FullItemList[ItemCategoryComboBox.SelectedIndex].Length - 1;
            }

            DebugLog($"PopulateItemIdComboBox ItemIdIndex {prevItemSelection}");
            DebugLog($"PopulateItemIdComboBox ItemCategory {ItemCategoryComboBox.SelectedIndex}");

            var ItemIdList = new ItemIdComboBox(ItemCategoryComboBox.SelectedIndex, FullItemList).ItemsIndexes;
            ItemIDComboBox.ItemsSource = ItemIdList;
            ItemIDComboBox.SelectedIndex = prevItemSelection;

            DebugLog("Return PopulateItemIdComboBox");
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
            GameVersionComboBox.SelectedIndex = 0;
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
            return;
        }

        private void RemoveItemSlot()
        {
            return;
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

            return false;
        }

        private void WriteChangestoTemp(int offset, int structSize, byte[] newBytes)
        {
            return;
        }

        public static void ReplaceBytes(string filename, int position, byte[] data, int byteArrayLength)
        {
            return;
        }

        private static void CreateTempFiles()
        {
            string tempFilePath = System.IO.Path.GetTempPath();

            tempShopV = tempFilePath + "\\TempShopV.ftd";
            tempShopR = tempFilePath + "\\TempShopR.ftd";
            tempNameV = tempFilePath + "\\TempNameV.ftd";
            tempNameR = tempFilePath + "\\TempNameR.ftd";

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

        internal static void DebugLog(object message)
        {
#if DEBUG
            Console.WriteLine(message);
#endif
        }
    }  
}
