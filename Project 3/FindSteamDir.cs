using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Project_3
{
    public class FindSteamDir
    {
        public static List<String> find()
        {
            List<string> libraryPaths = new List<string>();
            string steamPath, regexSearch = "[A-Z]:\\\\";
            RegistryKey steam64 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Valve\\Steam"),
                steam32 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Valve\\Steam");
            if (steam64 != null)
            {
                steamPath = (string) steam64.GetValue("InstallPath");
                var libraryLocation = steamPath + "\\steamapps\\libraryfolders.vdf";
                if (File.Exists(libraryLocation))
                {
                    var data = File.ReadAllLines(libraryLocation);
                    foreach (var value in data)
                    {
                        var result = Regex.Match(value, regexSearch);
                        if (result.Success && !string.IsNullOrEmpty(value))
                        {
                            var lib = value.Substring(value.IndexOf(result.ToString(), StringComparison.Ordinal));
                            lib = lib.Replace("\\\\", "\\");
                            lib = lib.Replace("\"", "\\steamapps\\common\\");
                            if (Directory.Exists(lib))
                                libraryPaths.Add(lib);
                        }
                    }
                }
            }
            else
            {
                if (steam32 != null)
                {
                    steamPath = (string) steam32.GetValue("InstallPath");
                    var libraryFolder = steamPath + "\\steamapps\\libraryfolders.vtf";
                    if (File.Exists(libraryFolder))
                    {
                        var data = File.ReadAllLines(libraryFolder);
                        foreach (var value in data)
                        {
                            var result = Regex.Match(value, regexSearch);
                            if (result.Success && !string.IsNullOrWhiteSpace(value))
                            {
                                var lib = value.Substring(value.IndexOf(result.ToString(), StringComparison.Ordinal));
                                lib = lib.Replace("\\\\", "\\");
                                lib = lib.Replace("\"", "\\steamapps\\common\\");
                                if (Directory.Exists(lib))
                                    libraryPaths.Add(lib);
                            }
                        }
                    }
                }
            }
            return libraryPaths;
        }
    }
}