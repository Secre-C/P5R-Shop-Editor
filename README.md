# Shop-Editor
A GUI tool for editing shops in Persona 5 (Royal)

This is a tool for editing all of the regular shops in P5/P5R (not including takemi or Iwai).

[.Net 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) is required to run this program.

To use, run the .exe once to create the necessary folders to put your shop ftds in. Once that's done, grab fclPublicShopItemTable.ftd and fclPublicShopName.ftd from data/init/facility.pak/fclTable.bin in your game's files, and drag them into Original/(gameversion).

![image](https://user-images.githubusercontent.com/89033534/177061986-92a73779-747e-4c17-b0cb-6113812ca273.png)

the program should now open, and you'll be able to edit the shops as you want. Changes made will automatically be saved to a temp file, so you don't need to worry about losing progress when switching shops or even closing the program.

![image](https://user-images.githubusercontent.com/89033534/177225170-04129987-797b-4c2a-8a78-8f0b013fd1f3.png)

When you're done editing stuff, click "Save" in order to save the ftd to the output folder. If the amount of items in each shop is the same as in the original file, the program will also generate a Binary patch file (.bp) which you should use *instead* of the output ftd.

You can also edit the names of the shop, which for some shops show a white textbox in game. A .bp file will always be generated for shop name edits.

![image](https://user-images.githubusercontent.com/89033534/177062306-634e2bf2-f589-47db-a52b-98e8b4a3966c.png)
