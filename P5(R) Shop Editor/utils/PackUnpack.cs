using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop_Editor.utils
{
    internal class PackUnpack
    {
        public static void Unpack(string path)
        {
            var pakpack = new Process();

            string command = "unpack";
            string fclPath = $"{Path.GetTempPath()}{Path.GetFileName(path)}";
            string tempPath = $"{Path.GetTempPath()}FCLTABLE";
            string pakPath = $"{Directory.GetCurrentDirectory()}\\PAKPack\\PAKPack.exe";

            File.Copy(path, fclPath, true);
            string args = $"\"{command}\" \"{fclPath}\" \"{tempPath}\"";
            pakpack.StartInfo.FileName = pakPath;
            pakpack.StartInfo.Arguments = args;

            pakpack.Start();
            pakpack.WaitForExit();
            pakpack.Close();
        }

        public static void Pack(string originalPath, string outPath)
        {
            string command = "replace";
            string tempPath = $"{Path.GetTempPath()}FCLTABLE";
            string pakPath = $"{Directory.GetCurrentDirectory()}\\PAKPack\\PAKPack.exe";

            string args;
            args = $"\"{command}\" \"{originalPath}\" \"{tempPath}\" \"{outPath}\"";

            var pakpack = new Process();

            pakpack.StartInfo.FileName = pakPath;
            pakpack.StartInfo.Arguments = args;

            pakpack.Start();
            pakpack.WaitForExit();
        }
    }
}
