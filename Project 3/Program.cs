using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Win32;

namespace Project_3
{
    internal static class Program
    {
        private static List<string> _steamGameDirs = new List<string>();
        private static string _svenCoopDir;
        private static string _bShiftDir;
        private static string _opForDir;

        public static void Main()
        {
            Console.Clear();
            foreach (var value in FindSteamDir.find())
            {
                Console.WriteLine(value);
                Console.ReadKey();
            }

            Console.Title = "Half-Life Map Support Tool";
            Console.Write(
                "\n\\\\Half-Life Spinoff map support tool for Sven Co-Op by Matthew Findlay//\n\nThe required space for this tool to function is over 80MB of free space.\nInstallation can take a few minutes to complete.\nAlso, please stay focused on this application while it is running\n\nImportant: To install the support for these games you must have them installed on Steam\n\nPress any key to proceed or ESC to quit...");
            var info = Console.ReadKey();
            switch (info.Key)
            {
                case ConsoleKey.Escape:
                    Environment.Exit(0);
                    break;
                default:
                    Processor();
                    break;
            }
        }

        private static void Processor()
        {
            Console.Clear();
            Console.Title = "Processing";
            Console.Write("\n\\\\Detecting Steam Directories//");
            try
            {
                string steam32 = "SOFTWARE\\VALVE\\Steam",
                    steam64 = "SOFTWARE\\Wow6432Node\\Valve\\Steam",
                    config32path,
                    config64path,
                    steam32path,
                    steam64path,
                    steampath = "";
                RegistryKey key32 = Registry.LocalMachine.OpenSubKey(steam32), key64 = Registry.LocalMachine.OpenSubKey(steam64);
                var bit64 = false;
                if (key64 != null && key64.ToString() != "")
                {
                    steam64path = (string) key64.GetValue("InstallPath");
                    config64path = steam64path + @"\steamapps\libraryfolders.vdf";
                    var driveRegex = @"[A-Z]:\\";
                    if (File.Exists(config64path))
                    {
                        var configLines = File.ReadAllLines(config64path);
                        foreach (var item in configLines)
                        {
                            var match = Regex.Match(item, driveRegex);
                            if (item != string.Empty && match.Success)
                            {
                                var matched = match.ToString();
                                var item2 = item.Substring(item.IndexOf(matched, StringComparison.Ordinal));
                                item2 = item2.Replace("\\\\", "\\");
                                item2 = item2.Replace("\"", "\\steamapps\\common\\");
                                if (Directory.Exists(item2))
                                {
                                    bit64 = true;
                                    _steamGameDirs.Add(item2);
                                    Console.WriteLine("\n\n" + item2);
                                }
                            }
                        }

                        _steamGameDirs.Add(steam64path + "\\steamapps\\common\\");
                        steampath = steam64path;
                    }
                }

                if (key32 != null && !bit64)
                {
                    steam32path = key32.GetValue("InstallPath").ToString();
                    config32path = steam32path + @"\steamapps\libraryfolders.vdf";
                    var driveRegex = @"[A-Z]:\\";
                    if (File.Exists(config32path))
                    {
                        var configFile = File.ReadAllLines(config32path);
                        foreach (var item in configFile)
                        {
                            Console.WriteLine("32: " + item);
                            var match32 = Regex.Match(item, driveRegex);
                            if (item != string.Empty && match32.Success)
                            {
                                var matched32 = match32.ToString();
                                var item2 = item.Substring(item.IndexOf(matched32, StringComparison.Ordinal));
                                item2 = item2.Replace("\\\\", "\\");
                                item2 = item2.Replace("\"", @"\steamapps\common\");
                                Console.Write("\n" + item2);
                                _steamGameDirs.Add(item2);
                            }
                        }

                        _steamGameDirs.Add("\n" + steam32path + @"\steamapps\common\");
                    }
                }

                if (_steamGameDirs.Any())
                {
                    Console.WriteLine(steampath + "\\steamapps\\common\\");
                    Console.Write("\n\n" + _steamGameDirs.Count + " Steam game libraries found.");
                    Thread.Sleep(400);
                    GameFinder();
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine(
                        "ERROR: STEAM NOT FOUND\n\nThis application could not find your steam install\nUnfortunately this requires Steam to work.\nPlease install it before using this tool...");
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    Process.Start("https://store.steampowered.com/about/");
                    Environment.Exit(1209302022);
                }
            }
            catch
            {
                Console.Title = "Error";
                Console.WriteLine("A Serious error has occurred and the process had to be stopped: ");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        private static void GameFinder()
        {
            try
            {
                var gameInstalled = false;
                var voidlist = new List<Action>();
                for (var i = 0; i < _steamGameDirs.Count; i++)
                {
                    var svenCoop = _steamGameDirs[i] + @"Sven Co-op\svencoop";
                    if (Directory.Exists(svenCoop))
                    {
                        _svenCoopDir = svenCoop;
                    }

                    var bShift = _steamGameDirs[i] + @"Half-Life\bshift";
                    if (File.Exists(bShift + "\\maps\\ba_xen1.bsp") && Directory.GetDirectories(bShift).Length > 0)
                    {
                        _bShiftDir = bShift;
                        gameInstalled = true;
                        Console.WriteLine("Half Life: Blue Shift has been detected...");
                    }

                    var opFor = _steamGameDirs[i] + @"Half-Life\gearbox";
                    if (File.Exists(opFor + "\\maps\\of1a5.bsp") && Directory.GetDirectories(opFor).Length > 0)
                    {
                        _opForDir = opFor;
                        gameInstalled = true;
                        Console.WriteLine("Half Life: Opposing Force has been detected...");
                    }
                }

                if (string.IsNullOrEmpty(_svenCoopDir))
                {
                    Console.Clear();
                    Console.Write(
                        "You do not have the game \"Sven Co-Op\" installed. \nThis necessary for this tool function. \n\nThis tool can install it directly on Steam. \n\nPress Y to continue");
                    var info = Console.ReadKey();
                    if (info.Key == ConsoleKey.Y)
                    {
                        Process.Start("steam://run/225840");
                        Environment.Exit(0);
                    }
                }

                if (!gameInstalled)
                {
                    NoGames();
                }

                if (string.IsNullOrEmpty(_bShiftDir))
                {
                    SteamInstall("Half Life: Blue Shift", "steam://run/130");
                }

                if (string.IsNullOrEmpty(_opForDir))
                {
                    SteamInstall("Half Life: Opposing Force", "steam://run/50");
                }

                Finalization();
                Console.WriteLine("Flushing temporary data...");
                Console.Title = "Complete!";
                Console.Write("\n\nProcess complete! Press anything to close...");
                Console.ReadKey();
                Environment.Exit(69);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static void Finalization()
        {
            if (!string.IsNullOrEmpty(_bShiftDir))
            {
                InstallBlueShift();
            }

            if (!string.IsNullOrEmpty(_opForDir))
            {
                InstallOpposingForce();
            }
        }


        private static void NoGames()
        {
            while (true)
            {
                Console.Clear();
                Console.Title = "Error";
                Console.Write(
                    "You do not have either \"Half Life : Blue Shift\" nor \"Half Life : Opposing Force\" installed.\n\nThis application serves no purpose without them. \n\n1 - Install Blue Shift\n2 - Install Opposing Force\nAny other button - Exit this Application\n\nPlease Choose a number... ");

                Console.Write("\b");

                var into = Console.ReadKey();
                switch (into.Key)
                {
                    case ConsoleKey.D1:
                        Process.Start(
                            "steam://run/130");
                        continue;
                    case ConsoleKey.D2:
                        Process.Start(
                            "steam://run/50");
                        continue;
                    case ConsoleKey.NumPad2:
                        Process.Start(
                            "steam://run/50");
                        continue;
                    case ConsoleKey.NumPad1:
                        Process.Start(
                            "steam://run/130");
                        continue;
                    default:
                        Environment.Exit(0);
                        break;
                }
            }
        }

        private static void InstallBlueShift()
        {
            try
            {
                Console.Title = "Installing Blue Shift Function";
                if (!File.Exists(_svenCoopDir + "\\maps\\Ripent.exe"))
                {
                    Console.WriteLine("Fatal Error: Ripent.exe Missing");
                    Error("Half Life: Blue Shift");
                }

                var files = Directory.GetFiles(_bShiftDir + "\\maps\\", "*.bsp");
                Console.WriteLine("Copying official maps from installed game...");
                for (int i = 0; i < files.Length; i++)
                {
                    if (File.Exists(_svenCoopDir + "\\maps\\" + Path.GetFileName(files[i]))) continue;
                    Console.WriteLine("Copying: " + files[i]);

                    File.Copy(files[i], _svenCoopDir + "\\maps\\" + Path.GetFileName(files[i]));
                }

                var files2 = Directory.GetFiles(_svenCoopDir + "\\maps", "ba*.bsp");
                Console.WriteLine("Copying resources from installed game...");
                FolderCopier(_bShiftDir + "\\gfx\\env", _svenCoopDir + "\\gfx\\env");
                Console.WriteLine("-= Gearbox's original Half-Life Blue Shift maps have been copied. =-");
                Console.WriteLine("\nPreparing Files...");
                var info = new ProcessStartInfo {FileName = _svenCoopDir + @"\unzip.exe"};
                var svenDir2 = _svenCoopDir;
                svenDir2 = svenDir2.Replace("\\", "/");
                info.Arguments = "-o \"" + svenDir2 + "/bshift_support.sven\" -d \"" + svenDir2 + "/maps\"";
                var p = Process.Start(info);
                p?.WaitForExit();
                Console.WriteLine("Converting BSP format...");
                for (var i = 0;
                    i < files2.Length;
                    i++)
                {
                    if (File.Exists(_svenCoopDir + "\\maps" + Path.GetFileName(files[i]))) continue;
                    Console.WriteLine("Converting: " + files2[i]);
                    var info2 = new ProcessStartInfo
                    {
                        FileName = _svenCoopDir + "\\BShiftBSPConverter.exe", Arguments = "\"" + files2[i] + "\"",
                        UseShellExecute = false
                    };

                    var p2 = Process.Start(info2);
                    p2?.WaitForExit();
                }

                Console.WriteLine("Importing Entity Data...");
                for (var i = 0;
                    i < files.Length;
                    i++)
                {
                    if (File.Exists(_svenCoopDir + "\\maps" + Path.GetFileName(files[i]))) continue;
                    Console.WriteLine("Importing Data From: " + files2[i]);
                    var info2 = new ProcessStartInfo
                    {
                        FileName = _svenCoopDir + "\\maps\\Ripent.exe",
                        Arguments = "-import -noinfo \"" + files2[i] + "\"",
                        UseShellExecute = false
                    };
                    var p2 = Process.Start(info2);
                    p2?.WaitForExit();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(0);
            }
        }

        private static void InstallOpposingForce()
        {
            try
            {
                Console.Title = "Installing Opposing Force Function";
                if (!File.Exists(_svenCoopDir + "\\maps\\Ripent.exe"))
                {
                    Console.WriteLine("Fatal Error: Ripent.exe Missing");
                    Error("Half Life: Opposing Force");
                }

                var files = Directory.GetFiles(_opForDir + "\\maps\\", "*.bsp");
                Console.WriteLine("Copying official maps from installed game...");
                for (int i = 0; i < files.Length; i++)
                {
                    if (File.Exists(_svenCoopDir + "\\maps\\" + Path.GetFileName(files[i]))) continue;
                    Console.WriteLine("Copying: " + files[i]);
                    File.Copy(files[i], _svenCoopDir + "\\maps\\" + Path.GetFileName(files[i]));
                }

                var file2 = Directory.GetFiles(_svenCoopDir + "\\maps\\", "of*.bsp");
                Console.WriteLine("Copying resources from installed game...");
                FolderCopier(_opForDir + "\\gfx\\env", _svenCoopDir + "\\gfx\\env");
                Console.WriteLine("-= Gearbox's original Half-Life Opposing Force maps have been copied. =-");
                Console.WriteLine("Preparing Files...");
                var info = new ProcessStartInfo
                {
                    FileName = _svenCoopDir + @"\unzip.exe"
                };
                var svenDir2 = _svenCoopDir;
                svenDir2 = svenDir2.Replace("\\", "/");
                info.Arguments = " -o \"" + svenDir2 + "/opfor_support.sven\" -d \"" + svenDir2 + "/maps\"";
                var p = Process.Start(info);
                p?.WaitForExit();
                Console.WriteLine("Importing Entity Data...");
                for (var i = 0;
                    i < file2.Length;
                    i++)
                {
                    if (File.Exists(_svenCoopDir + "\\maps" + Path.GetFileName(files[i]))) continue;
                    Console.WriteLine("Importing Data From: " + file2[i]);
                    var info2 = new ProcessStartInfo
                    {
                        FileName = _svenCoopDir + "\\maps\\Ripent.exe",
                        Arguments = "-import -noinfo \"" + file2[i] + "\"",
                        UseShellExecute = false
                    };
                    var p2 = Process.Start(info2);
                    p2?.WaitForExit();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(0);
            }
        }

        private static void FolderCopier(string sourcePath, string finalPath)
        {
            foreach (var directory in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Console.WriteLine("1 Directory(s) Created");
                Directory.CreateDirectory(directory.Replace(sourcePath, finalPath));
            }

            /*foreach (var item in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(item, item.Replace(sourcePath, finalPath), true);
                Console.WriteLine($"Copied 1 item(s) {item.Replace(sourcePath, finalPath)}");
            }*/
        }

        private static void Error(string game)
        {
            Console.Clear();
            Console.Title = "Error";
            Console.Write("Error: " + game +
                          "support installation files not found!\n\nEnsure Sven Co-Op is correctly installed before running this installer.\n\nPress any button..."
            );
            Console.ReadKey();
            Environment.Exit(0);
        }

        private static void SteamInstall(string title, string steamUrl)
        {
            Console.Clear();
            Console.Write(
                "You do not have the game \"" + title +
                "\" installed.\nThis application can work without it though.\n\nDo you with for this to open steam and install it if you happen to own it? \n\nOtherwise it will take you to the purchase page\n\nPress Y for yes or any other key for no..."
            );

            var info = Console.ReadKey();
            if (info.Key == ConsoleKey.Y)
            {
                Process.Start(steamUrl);
            }
        }
    }
}