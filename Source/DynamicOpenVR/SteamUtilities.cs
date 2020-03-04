﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace DynamicOpenVR
{
    public static class SteamUtilities
    {
        public static string GetSteamHomeDirectory()
        {
            string steamPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", string.Empty).ToString();

            if (!string.IsNullOrEmpty(steamPath) && Directory.Exists(steamPath))
            {
                if (TryGetExactPath(steamPath, out string exactPath))
                {
                    return exactPath;
                }
                else
                {
                    return steamPath;
                }
            }

            Process steamProcess = Process.GetProcessesByName("Steam").FirstOrDefault();

            if (steamProcess == null)
            {
                throw new Exception("Steam process could not be found.");
            }

            var stringBuilder = new StringBuilder(2048);
            int capacity = stringBuilder.Capacity + 1;

            if (NativeMethods.QueryFullProcessImageName(steamProcess.Handle, 0, stringBuilder, ref capacity) == 0)
            {
                throw new Exception("QueryFullProcessImageName returned 0");
            }

            string exePath = stringBuilder.ToString();

            if (string.IsNullOrEmpty(exePath))
            {
                throw new Exception("Steam path could not be found.");
            }

            steamPath = Path.GetDirectoryName(exePath);

            return steamPath;
        }

        /// <summary>
        /// Gets the exact case used on the file system for an existing file or directory.
        /// From https://stackoverflow.com/a/29578292/3133529
        /// </summary>
        /// <param name="path">A relative or absolute path.</param>
        /// <param name="exactPath">The full path using the correct case if the path exists.  Otherwise, null.</param>
        /// <returns>True if the exact path was found.  False otherwise.</returns>
        /// <remarks>
        /// This supports drive-lettered paths and UNC paths, but a UNC root
        /// will be returned in title case (e.g., \\Server\Share).
        /// </remarks>
        private static bool TryGetExactPath(string path, out string exactPath)
        {
            bool result = false;
            exactPath = null;

            // DirectoryInfo accepts either a file path or a directory path, and most of its properties work for either.
            // However, its Exists property only works for a directory path.
            DirectoryInfo directory = new DirectoryInfo(path);
            if (File.Exists(path) || directory.Exists)
            {
                List<string> parts = new List<string>();

                DirectoryInfo parentDirectory = directory.Parent;
                while (parentDirectory != null)
                {
                    FileSystemInfo entry = parentDirectory.EnumerateFileSystemInfos(directory.Name).First();
                    parts.Add(entry.Name);

                    directory = parentDirectory;
                    parentDirectory = directory.Parent;
                }

                // Handle the root part (i.e., drive letter or UNC \\server\share).
                string root = directory.FullName;
                if (root.Contains(':'))
                {
                    root = root.ToUpper();
                }
                else
                {
                    string[] rootParts = root.Split('\\');
                    root = string.Join("\\", rootParts.Select(part => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(part)));
                }

                parts.Add(root);
                parts.Reverse();
                exactPath = Path.Combine(parts.ToArray());
                result = true;
            }

            return result;
        }
    }
}
