using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using ZzukBot.Constants;
using ZzukBot.ExtensionMethods;
using ZzukBot.Helpers;
using ZzukBot.Properties;

namespace ZzukBot.Mem
{
    [App.Singleton(Order = 0)]
    internal sealed class Libs
    {
        internal const string FastCall = "FastCall.dll";
        internal const string Pathfinder = "038.mmap";
        internal readonly IntPtr PathfinderPtr = IntPtr.Zero;
        private readonly IntPtr FastCallPtr = IntPtr.Zero;


        private readonly string subPath = Assembly.GetExecutingAssembly().ExtJumpUp(1);

        private Libs()
        {
            File.Delete(pathFastCall);
            if (FastCallPtr == IntPtr.Zero)
            {
                FastCallPtr = Load(pathFastCall, Resources.FastCall);
            }
            if (PathfinderPtr == IntPtr.Zero)
                PathfinderPtr = Load(pathPathfinder, Resources.PathFinder);
        }

        internal static Libs Instance { get; } = new Libs();
        private string pathFastCall => subPath + "\\" + FastCall;
        private string pathPathfinder => subPath + "\\mmaps\\038.mmap";

        private IntPtr Load(string parPath, byte[] parBytes)
        {
            parPath.FileCreate(parBytes);
            return WinImports.LoadLibrary(parPath);
        }
    }
}