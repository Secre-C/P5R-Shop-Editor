﻿using Amicitia.IO.Binary;
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
        public string ShopFileDirectory { get; private set; }
        public bool IsShopLoaded { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            FullItemList = ItemCategories.GetItemList("P5R");
        }

        private void StartupPopulate(string inputDir)
        {
            DebugLog("StartupPopulate");

            ShopItemFile = new ShopItemFile().Read($"{inputDir}/fclPublicShopItemTable.ftd");
            ShopNameFile = new ShopNameFile().Read($"{inputDir}/fclPublicShopName.ftd");
            ShopDataFile = new ShopDataFile().Read($"{inputDir}/fclPublicShopDataTable.ftd");

            PopulateShopNameComboBox();
            ClearAllFields();
        }
        private void PopulateShopData()
        {
            var shopId = ShopDataFile.ShopData[SelectedShop];

            BannerIDTextBox.Text = shopId.BannerId.ToString();
            HideNameTagCheckBox.IsChecked = shopId.HideNametag;
            ShopModeComboBox.SelectedIndex = shopId.ShopMode;

            ShopNameTextBox.Text = ShopNameFile.ShopNames[SelectedShop].Name;
        }
        private void PopulateShopNameComboBox()
        {
            DebugLog("PopulateShopNameComboBox");

            ShopSelectionComboBox.ItemsSource = new ShopComboBox(ShopNameFile).NameIndex;
        }
        private void PopulateShopItemComboBox()
        {
            DebugLog("PopulateShopItemComboBox");

            var storeItemSelection = SelectedItem;

            DebugLog($"PopulateShopItemComboBox Category {ItemCategoryComboBox.SelectedIndex}");
            DebugLog($"PopulateShopItemComboBox Index {ItemIDComboBox.SelectedIndex}");

            try
            {
                ShopItemBoxList = new ShopItemListBox(ShopItemFile.Shops[SelectedShop], FullItemList).ItemEntries;
                ItemSelectionComboBox.ItemsSource = ShopItemBoxList;
                ItemSelectionComboBox.SelectedIndex = storeItemSelection;
            }
            catch
            {
                DebugLog("Oopsie");
            }

        }
        private void PopulateItemValues()
        {
            if (SelectedItem == -1 || ShopItemFile.Shops[SelectedShop].Items.Count < 1) return;

            DebugLog("PopulateItemValues");

            int shopInput = ShopSelectionComboBox.SelectedIndex;

            int shopItemIndex = ItemSelectionComboBox.SelectedIndex;

            var selectedShopItem = ShopItemFile.Shops[shopInput].Items[shopItemIndex];

            var ItemId = selectedShopItem.ItemId.ItemIndex;

            UnlimitedQuantityCheckBox.IsChecked = selectedShopItem.ShopEndIndicator == 0xFF ? true : false;
            ItemCategoryComboBox.SelectedIndex = selectedShopItem.ItemId.ItemCategory;
            ItemIDComboBox.SelectedIndex = ItemId;
            AmountPerUnitTextBox.Text = selectedShopItem.AmountPerUnit.ToString();
            StartMonthBox.SelectedIndex = selectedShopItem.AvailabilityStartMonth - 1;
            StartDayTextBox.Text = selectedShopItem.AvailabilityStartDay.ToString();
            EndMonthBox.SelectedIndex = selectedShopItem.AvailabilityEndMonth - 1;
            EndDayTextBox.Text = selectedShopItem.AvailabilityEndDay.ToString();
            QuantityTextBox.Text = selectedShopItem.Quantity.ToString();
            RequiredBitflagTextBox.Text = selectedShopItem.Bitflag.ConvertedBitflag.ToString();
            PricePercentageTextBox.Text = selectedShopItem.PercentageOfPrice.ToString();

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
        private void ClearAllFields()
        {
            ItemSelectionComboBox.SelectedIndex = -1;

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

        private void MenuItem_Open_Click(object sender, RoutedEventArgs e)
        {
            DebugLog("MenuItem_Open_Click");

            var saveDialog = new CommonOpenFileDialog();
            saveDialog.IsFolderPicker = true;
            saveDialog.Title = "Select Directory with files.";

            if (saveDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                ShopFileDirectory = saveDialog.FileName;
                StartupPopulate(ShopFileDirectory);

                ShopSelectionComboBox.SelectedIndex = 0;
                PopulateShopItemComboBox();
            }

            IsShopLoaded = true;
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DebugLog("SaveButton_Click");

            if (!IsShopLoaded)
                return;

            var saveDialog = new CommonOpenFileDialog();
            saveDialog.IsFolderPicker = true;
            saveDialog.Title = "Select Directory to save files to.";

            if (saveDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                ShopItemFile.Write($"{saveDialog.FileName}/fclPublicShopItemTable.ftd", ShopItemFile);
                ShopNameFile.Write($"{saveDialog.FileName}/fclPublicShopName.ftd", ShopNameFile);
                ShopDataFile.Write($"{saveDialog.FileName}/fclPublicShopDataTable.ftd", ShopDataFile);

                StartupPopulate(saveDialog.FileName);
                ShopFileDirectory = saveDialog.FileName;
            }
        }
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            DebugLog("ResetButton_Click");

            if (!IsShopLoaded)
                return;

            ShopItemFile = new ShopItemFile().Read($"{ShopFileDirectory}/fclPublicShopItemTable.ftd");
            ShopNameFile = new ShopNameFile().Read($"{ShopFileDirectory}/fclPublicShopName.ftd");
            ShopDataFile = new ShopDataFile().Read($"{ShopFileDirectory}/fclPublicShopDataTable.ftd");

            PopulateShopItemComboBox();

            PopulateItemValues();
        }
        private void AddNewItemButton_Click(object sender, RoutedEventArgs e)
        {
            DebugLog("AddNewItemButton_Click");

            if (!IsShopLoaded || SelectedShop < 0)
                return;

            var shopItemFile = ShopItemFile;
            var shopItemQuantity = ShopItemFile.Shops[SelectedShop].Items.Count;

            object itemToCopy;

            if (SelectedItem >= 0)
            {
                ShopItemFile.AddItem(ref shopItemFile, SelectedShop, shopItemQuantity, SelectedItem);
            }
            else
            {
                ShopItemFile.AddItem(ref shopItemFile, SelectedShop, shopItemQuantity);
            }

            ShopItemFile = shopItemFile;

            PopulateShopItemComboBox();

            ItemSelectionComboBox.SelectedItem = shopItemQuantity;

            return;
        }
        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            DebugLog("RemoveItemButton_Click");

            if (!IsShopLoaded || SelectedShop < 0 || SelectedItem < 0)
                return;

            var shopItemFile = ShopItemFile;

            ShopItemFile.RemoveItem(ref shopItemFile, SelectedShop, SelectedItem);
            ShopItemFile = shopItemFile;

            PopulateShopItemComboBox();

            ItemSelectionComboBox.SelectedItem = SelectedItem - 1;

            return;
        }
        private void AddNewShopButton_Click(object sender, RoutedEventArgs e)
        {
            DebugLog("AddNewShopButton_Click");

            if (!IsShopLoaded)
                return;

            var shopItemFile = ShopItemFile;
            var shopNameFile = ShopNameFile;
            var shopDataFile = ShopDataFile;

            var newShopName = new ShopNames();
            newShopName.Name = "New Shop";

            ShopItemFile.AddBlankShop(ref shopItemFile, ShopItemFile.Shops.Count);
            ShopNameFile.Add(ref shopNameFile, ShopNameFile.ShopNames.Count);
            ShopDataFile.Add(ref shopDataFile, ShopDataFile.ShopData.Count);

            ShopItemFile = shopItemFile;
            ShopNameFile = shopNameFile;
            ShopDataFile = shopDataFile;

            PopulateShopNameComboBox();
            ShopSelectionComboBox.SelectedIndex = ShopItemFile.Shops.Count - 1;
        }
        private void RemoveShopButton_Click(object sender, RoutedEventArgs e)
        {
            DebugLog("RemoveShopButton_Click");

            if (!IsShopLoaded || SelectedShop < 0)
                return;

            var selectedShop = SelectedShop;

            var shopItemFile = ShopItemFile;
            var shopNameFile = ShopNameFile;
            var shopDataFile = ShopDataFile;

            ShopItemFile.RemoveShop(ref shopItemFile, selectedShop);
            ShopNameFile.Remove(ref shopNameFile, selectedShop);
            shopDataFile.Remove(ref shopDataFile, selectedShop);

            ShopItemFile = shopItemFile;
            ShopNameFile = shopNameFile;
            ShopDataFile = shopDataFile;

            PopulateShopNameComboBox();

            ShopSelectionComboBox.SelectedIndex = selectedShop;
        }

        private void ShopSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedShop = ShopSelectionComboBox.SelectedIndex;

            if (SelectedShop == -1)
                return;

            DebugLog("ShopSelectionComboBox_SelectionChanged");

            ClearAllFields();
            PopulateShopItemComboBox();

            PopulateShopData();

            return;
        }

        private void ShopNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectedShop == -1)
                return;

            DebugLog("ShopNameTextBox_TextChanged");

            ShopNameFile.ShopNames[SelectedShop].Name = ShopNameTextBox.Text;
        }

        private void BannerIDTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectedShop == -1)
                return;

            DebugLog("BannerIDTextBox_TextChanged");

            ShopDataFile.ShopData[SelectedShop].BannerId = Convert.ToInt16(((TextBox)sender).Text);
        }
        private void HideNameTagCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (SelectedShop == -1)
                return;

            DebugLog("HideNameTagCheckBox_Checked");

            ShopDataFile.ShopData[SelectedShop].HideNametag = (bool)((CheckBox)sender).IsChecked;
        }
        private void ShopModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedShop == -1)
                return;

            DebugLog("ShopModeComboBox_SelectionChanged");

            ShopDataFile.ShopData[SelectedShop].ShopMode = (byte)((ComboBox)sender).SelectedIndex;
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
        private void UnlimitedQuantity_Checked(object sender, RoutedEventArgs e)
        {
            if (SelectedItem == -1)
                return;

            DebugLog("UnlimitedQuantity_Checked");

            ShopItemFile.Shops[SelectedShop].Items[SelectedItem].ShopEndIndicator = (byte)(UnlimitedQuantityCheckBox.IsChecked == true ? 0xFF : 0);
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
        private void BannerIdValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !TextValidation.IsBannerValid(((TextBox)sender).Text + e.Text);
        }

        private void KofiLink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://ko-fi.com/secrec9802") { UseShellExecute = true });
        }

        internal static void DebugLog(object message)
        {
#if DEBUG
            Console.WriteLine(message);
#endif
        }
    }  
}
