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
        public MainWindow()
        {
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

        private void Reset_Button_Click(object sender, RoutedEventArgs e)
        {
            ResetStuff();
            ShowMessage("Changes to the selected Item have been reverted to their last saved state!");
        }

        private void ResetAllButton_Click(object sender, RoutedEventArgs e)
        {
            string gameVersion;
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            if (gameVersionIndex == 0) gameVersion = "Royal";
            else gameVersion = "Vanilla";

            MessageBoxButton button = MessageBoxButton.YesNo;
            string resetAllPrompt = $"This will overwrite the '{gameVersion}\\Output' ftds with the ones from '{gameVersion}\\Original'. Proceed?";
            string resetAllPromptCaption = "Reset All";
            var result = MessageBox.Show(resetAllPrompt, resetAllPromptCaption, button);

            if (result == MessageBoxResult.No) return;

            string shopItemftdOriginal = (Directory.GetCurrentDirectory() + $"\\Original\\{gameVersion}\\fclPublicShopItemTable.ftd");
            string shopNameftdOriginal = (Directory.GetCurrentDirectory() + $"\\Original\\{gameVersion}\\fclPublicShopName.ftd");

            string shopItemftdOutput = (Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopItemTable.ftd");
            string shopNameftdOutput = (Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopName.ftd");

            File.Copy(shopItemftdOriginal, shopItemftdOutput, true);
            File.Copy(shopNameftdOriginal, shopNameftdOutput, true);

            ResetStuff();
            ShowMessage($"All {gameVersion} items have been reverted to their original state!");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string gameVersion = GameVersionComboBox.Text;
            bool isStructureIntact = CompareEntryCounts();

            if (!CheckForChanges())
            {
                ShowMessage("No Changes have been made!");
                return;
            }

            SaveItemftd("Overwrite");
            SaveNameftd();

            if (isStructureIntact)
            {
                ShowMessage($"Created Binary Patch file (.bp) and Saved Changes to Output\\{gameVersion}!");

                if (!File.Exists(Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\shop_changes.bp"))
                {
                    File.Create(Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\shop_changes.bp");
                }
            }
            else
            {
                ShowMessage($"Changes Saved to Output\\{gameVersion}!");

                if (File.Exists(Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\shop_changes.bp"))
                {
                    File.Delete(Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\shop_changes.bp");
                }
            }

            ResetStuff();
        }

        private void AddNewItemButton_Click(object sender, RoutedEventArgs e)
        {
            bool isNewItemDuplicate = Convert.ToInt32(NumOfItemsTextBox.Text) > 0;
            SaveItemftd("Add");
            ItemSelectionComboBox_SelectionChanged(null, null);
            PopulateShopInformation();
            PopulateShopItemComboBox();
            if (isNewItemDuplicate)
            {
                string gameVersion = GameVersionComboBox.Text;
                string itemAddMessage = $"Created duplicate of selected shop item! Changes have been saved to Original\\{gameVersion}.";
                ShowMessage(itemAddMessage);
            }
            else
            {
                string gameVersion = GameVersionComboBox.Text;
                string itemAddMessage = $"Created New item! Changes have been saved to Original\\{gameVersion}.";
                ShowMessage(itemAddMessage);

            }
        }

        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (NumOfItemsTextBox.Text == "1")
            {
                string deletePrompt = "Item not removed. Each Shop Needs at least one Item!";
                ShowMessage(deletePrompt);
                return;
            }
            string gameVersion = GameVersionComboBox.Text;
            SaveItemftd("Remove");
            ItemSelectionComboBox_SelectionChanged(null, null);
            PopulateShopInformation();
            PopulateShopItemComboBox();
            ShowMessage($"Selected item has been removed! Changes have been saved to Original\\{gameVersion}.");
        }
        private void ItemSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string gameVersion = GameVersionComboBox.Text;
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;
            string shopItemftd = (Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopItemTable.ftd");

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

            if (gameVersionIndex == 0)
            {
                gameVersion = "Royal";
            }
            else
            {
                gameVersion = "Vanilla";
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
            string shopItemftd = (Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopItemTable.ftd");
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
            string gameVersion = GameVersionComboBox.Text;
            string shopItemftd = (Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopItemTable.ftd");

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
                QuantityTextBox.Background = new SolidColorBrush(Color.FromArgb(0xff, 0xee, 0xee, 0xee));

            }
            else
            {
                QuantityTextBox.IsReadOnly = false;
                QuantityTextBox.Background = Brushes.White;
            }
        }

        private void PopulateShopNameComboBox()
        {
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            List<string> ShopNameList = NameParse.MakeShopNameList(gameVersionIndex);

            var shopIndex = ShopSelectionComboBox.SelectedIndex;
            ShopSelectionComboBox.ItemsSource = ShopNameList;
            if (shopIndex == -1) shopIndex = 0;
            ShopSelectionComboBox.SelectedIndex = shopIndex;

            PopulateShopInformation();
        }

        private void PopulateShopInformation()
        {
            string gameVersion = GameVersionComboBox.Text;
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;
            string shopItemftd = (Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopItemTable.ftd");

            var shopIndex = ShopSelectionComboBox.SelectedIndex;

            List<int> itemCount = FtdParse.FindShopOffsetsandCount(shopItemftd, "ItemCount");

            NumOfItemsTextBox.Text = itemCount[shopIndex].ToString();
            ShopNameTextBox.Text = NameParse.PrintShopName(shopIndex, gameVersionIndex);
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
                if (!File.Exists(Directory.GetCurrentDirectory() + "\\Output\\Royal\\fclPublicShopItemTable.ftd"))
                {
                    File.Copy(Directory.GetCurrentDirectory() + "\\Original\\Royal\\fclPublicShopItemTable.ftd", Directory.GetCurrentDirectory() + "\\Output\\Royal\\fclPublicShopItemTable.ftd");
                }

                if (!File.Exists(Directory.GetCurrentDirectory() + "\\Output\\Royal\\fclPublicShopName.ftd"))
                {
                    File.Copy(Directory.GetCurrentDirectory() + "\\Original\\Royal\\fclPublicShopName.ftd", Directory.GetCurrentDirectory() + "\\Output\\Royal\\fclPublicShopName.ftd");
                }

            }

            if (File.Exists(Directory.GetCurrentDirectory() + "\\Original\\Vanilla\\fclPublicShopItemTable.ftd") 
                && File.Exists(Directory.GetCurrentDirectory() + "\\Original\\Vanilla\\fclPublicShopName.ftd"))
            {
                if (!File.Exists(Directory.GetCurrentDirectory() + "\\Output\\Vanilla\\fclPublicShopItemTable.ftd"))
                {
                    File.Copy(Directory.GetCurrentDirectory() + "\\Original\\Vanilla\\fclPublicShopItemTable.ftd", Directory.GetCurrentDirectory() + "\\Output\\Vanilla\\fclPublicShopItemTable.ftd");
                }

                if (!File.Exists(Directory.GetCurrentDirectory() + "\\Output\\Vanilla\\fclPublicShopName.ftd"))
                {
                    File.Copy(Directory.GetCurrentDirectory() + "\\Original\\Vanilla\\fclPublicShopName.ftd", Directory.GetCurrentDirectory() + "\\Output\\Vanilla\\fclPublicShopName.ftd");
                }
            }

            if (GameVersionComboBox.SelectedIndex == 0) ShopNameTextBox.MaxLength = 48;
            else ShopNameTextBox.MaxLength = 32;
        }

        private void SaveItemftd(string writeMode) //"Add", "Remove", or "Overwrite"
        {
            string gameVersion;
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            if (gameVersionIndex == 0) gameVersion = "Royal";
            else gameVersion = "Vanilla";

            string shopItemftd = (Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopItemTable.ftd");
            string tempshopItemftd = (Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopItemTabletemp.ftd");

            List<int> shopOffsets = FtdParse.FindShopOffsetsandCount(shopItemftd, "ShopOffsets");

            int shopID = Convert.ToInt32(ShopIDTextBox.Text);
            int shopItemIndex = ItemSelectionComboBox.SelectedIndex;

            int itemOffset = shopOffsets[shopID] + (shopItemIndex * 40);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (BinaryObjectReader P5FTDFile = new (shopItemftd, Endianness.Big, Encoding.GetEncoding(932)))
            {
                using (BinaryObjectWriter P5NewFTDFile = new (tempshopItemftd, Endianness.Big, Encoding.GetEncoding(932)))
                {
                    for (int i = 0; i < itemOffset; i++)
                    {
                        P5NewFTDFile.Write(P5FTDFile.ReadByte());
                    }

                    if (writeMode == "Remove")
                    {
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
                    }
                    else if (writeMode == "Add")
                    {
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
                    }
                    else
                    {
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
                    }

                    long remainingSpace = P5FTDFile.Length - P5FTDFile.Position;

                    for (int i = 0; i < (int)remainingSpace; i++) //write to end of file
                    {
                        P5NewFTDFile.Write(P5FTDFile.ReadByte());
                    }
                }
            }
            File.Copy(tempshopItemftd, shopItemftd, true);
            File.Delete(tempshopItemftd);
        }

        private void SaveNameftd()
        {
            string gameVersion;
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            if (gameVersionIndex == 0) gameVersion = "Royal";
            else gameVersion = "Vanilla";

            string tempshopNameftd = (Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopItemTabletemp.ftd");
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
            File.Delete(tempshopNameftd);
        }
        private int GetItemNum()
        {
            string gameVersion;
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            if (gameVersionIndex == 0) gameVersion = "Royal";
            else gameVersion = "Vanilla";

            string shopItemftd = (Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopItemTable.ftd");

            List<int> shopOffsets = FtdParse.FindShopOffsetsandCount(shopItemftd, "ShopOffsets");

            int shopID = Convert.ToInt32(ShopIDTextBox.Text);
            int shopItemIndex = ItemSelectionComboBox.SelectedIndex;

            int itemOffset = shopOffsets[shopID] + ((shopItemIndex * 40) + 2);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (BinaryObjectReader P5FTDFile = new (shopItemftd, Endianness.Big, Encoding.GetEncoding(932)))
            {
                P5FTDFile.AtOffset(itemOffset);
                ushort itemIndex = P5FTDFile.ReadUInt16();

                int itemCategory = itemIndex / 0x1000;
                int itemID = itemIndex - (itemCategory * 0x1000);

                return itemID;
            }
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
                MessageBox.Show("New Directories Created!", "Created Directories");
            }
        }

        private bool CompareEntryCounts()
        {
            string gameVersion;
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            if (gameVersionIndex == 0) gameVersion = "Royal";
            else gameVersion = "Vanilla";

            string shopItemftdOutput = (Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopItemTable.ftd");
            string shopItemftdOriginal = (Directory.GetCurrentDirectory() + $"\\Original\\{gameVersion}\\fclPublicShopItemTable.ftd");

            int outputEntryCount;
            int originalEntryCount;

            List<int> outputOffsets = FtdParse.FindShopOffsetsandCount(shopItemftdOutput, "ShopOffsets");
            List<int> originalOffsets = FtdParse.FindShopOffsetsandCount(shopItemftdOriginal, "ShopOffsets");

            using (BinaryObjectReader P5FTDFile = new(shopItemftdOutput, Endianness.Big, Encoding.GetEncoding(932)))
            {
                P5FTDFile.AtOffset(40);
                outputEntryCount = P5FTDFile.ReadInt32();
            }

            using (BinaryObjectReader P5FTDFile = new(shopItemftdOriginal, Endianness.Big, Encoding.GetEncoding(932)))
            {
                P5FTDFile.AtOffset(40);
                originalEntryCount = P5FTDFile.ReadInt32();
            }

            if (outputEntryCount == originalEntryCount && Enumerable.SequenceEqual(outputOffsets, originalOffsets))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void ShowMessage(string message)
        {
            MessageTextBox.Text = message;
        }

        private bool CheckForChanges()
        {
            bool changes = CheckItemChanges();
            return changes;
        }

        private bool CheckItemChanges()
        {
            string gameVersion;
            int gameVersionIndex = GameVersionComboBox.SelectedIndex;

            if (gameVersionIndex == 0) gameVersion = "Royal";
            else gameVersion = "Vanilla";

            string shopItemftd = (Directory.GetCurrentDirectory() + $"\\Output\\{gameVersion}\\fclPublicShopItemTable.ftd");

            List<int> shopOffsets = FtdParse.FindShopOffsetsandCount(shopItemftd, "ShopOffsets");

            int shopID = ShopSelectionComboBox.SelectedIndex;
            int shopItemIndex = ItemSelectionComboBox.SelectedIndex;

            int itemOffset = shopOffsets[shopID] + (shopItemIndex * 40);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (BinaryObjectReader P5FTDFile = new BinaryObjectReader(shopItemftd, Endianness.Big, Encoding.GetEncoding(932)))
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
                bool quantity = P5FTDFile.ReadByte() == Convert.ToByte(QuantityTextBox.Text);
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
                    bitflagRequirement = convertedFlag == Convert.ToInt32(RequiredBitflagTextBox.Text);
                }
                else
                {
                    bitflagRequirement = P5FTDFile.ReadInt32() == Convert.ToInt32(RequiredBitflagTextBox.Text);
                }

                P5FTDFile.ReadInt32();

                bool pricePercentage = P5FTDFile.ReadInt32() == Convert.ToInt32(PricePercentageTextBox.Text); //Price Percentage

                P5FTDFile.ReadInt32();
                //Console.WriteLine((unlimQuantity + " " + itemID + " " + amountPerUnit + " " + startMonth + " " + startDay + " " + endMonth + " " + endDay + " " + quantity + " " + bitflagRequirement + " " + pricePercentage));
                if (!unlimQuantity || !itemID || !amountPerUnit || !startMonth || !startDay || !endMonth || !endDay || !quantity || !bitflagRequirement || !pricePercentage) return true;
                else return false;
            }
        }
    }  

}
