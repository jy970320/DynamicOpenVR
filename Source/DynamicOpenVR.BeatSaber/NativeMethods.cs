﻿using System;
using System.Runtime.InteropServices;

namespace DynamicOpenVR.BeatSaber
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string dllToLoad);
    }
}
