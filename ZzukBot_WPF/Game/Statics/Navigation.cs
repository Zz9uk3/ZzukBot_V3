using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ZzukBot.Constants;
using ZzukBot.Mem;
using ZzukBot.Objects;

namespace ZzukBot.Game.Statics
{
    /// <summary>
    /// Helps the bot generate paths through the world
    /// </summary>
    [App.Singleton]
    public unsafe class Navigation
    {
        /// <summary>
        /// Access to the pathfinder
        /// </summary>
        public static Navigation Instance = new Navigation();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate _XYZ* CalculatePathDelegate(
            uint mapId, _XYZ start, _XYZ end, bool parSmooth,
            out int length);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void FreePathArr(
            _XYZ* pathArr);

        private static CalculatePathDelegate _calculatePath;
        private static FreePathArr _freePathArr;

        /// <summary>
        /// Generate a path from start to end
        /// </summary>
        /// <param name="mapId">The map ID the player is on</param>
        /// <param name="start">Start</param>
        /// <param name="end">End</param>
        /// <param name="parSmooth">Smooth path</param>
        /// <returns>An array of points</returns>
        public Location[] CalculatePath(
            uint mapId, Location start, Location end, bool parSmooth)
        {
            int length;
            var ret = _calculatePath(mapId, start.ToStruct, end.ToStruct, parSmooth, out length);
            var list = new Location[length];
            for (var i = 0; i < length; i++)
            {
                list[i] = new Location(ret[i]);
            }
            _freePathArr(ret);
            return list;
        }

        private Navigation()
        {
            var calculatePathPtr = WinImports.GetProcAddress(Libs.Instance.PathfinderPtr,
                       "CalculatePath");
            _calculatePath = Memory.Reader.RegisterDelegate<CalculatePathDelegate>(calculatePathPtr);

            var freePathPtr = WinImports.GetProcAddress(Libs.Instance.PathfinderPtr,
                        "FreePathArr");
            _freePathArr = Memory.Reader.RegisterDelegate<FreePathArr>(freePathPtr);

            //Memory.ErasePeHeader(Libs.Instance.PathfinderPtr);
            //Memory.UnlinkFromPeb("038.mmap");
        }


    }
}

//using System;
//using System.CodeDom.Compiler;
//using System.Collections;
//using System.Collections.Generic;
//using System.Runtime.InteropServices;
//using System.Threading;
//using ZzukBot.Constants;
//using ZzukBot.Mem;
//using ZzukBot.Objects;
//using ZzukBot.Settings;
//using File = System.IO.File;

//namespace ZzukBot.Game.Statics
//{
//    /// <summary>
//    /// Class that handles path generation
//    /// </summary>
//    internal static class Navigation
//    {
//        [DllImport(Libs.Pathfinder)]
//        private static extern unsafe _XYZ* CalculatePath(uint mapId, _XYZ start, _XYZ end, bool parSmooth,
//            out int length);

//        [DllImport(Libs.Pathfinder)]
//        private static extern unsafe void FreePathArr(_XYZ* pathArr);

//        //[DllImport(Libs.NavigationPath)]
//        //private static extern int GetPathArray([Out] _XYZ[] path, int length);

//        /// <summary>
//        /// CalculatePath
//        /// </summary>
//        /// <param name="parStart"></param>
//        /// <param name="parEnd"></param>
//        /// <param name="parSmooth"></param>
//        /// <returns></returns>
//        public static unsafe Location[] CalculatePath(Location parStart, Location parEnd, bool parSmooth)
//        {
//            Location[] ret;
//            try
//            {
//                int length;
//                var pathArr = CalculatePath((uint)ObjectManager.Instance.Player.MapId, parStart.ToStruct, parEnd.ToStruct,
//                    parSmooth, out length);
//                ret = new Location[length];
//                for (var i = 0; i < length; i++)
//                {
//                    ret[i] = new Location(pathArr[i]);
//                }
//                FreePathArr(pathArr);
//            }
//            catch
//            {
//                ret = new[] { parStart, parEnd };
//            }
//            return ret;
//        }

//        /// <summary>
//        /// CalculatePathAsync
//        /// </summary>
//        /// <param name="parStart"></param>
//        /// <param name="parEnd"></param>
//        /// <param name="parSmooth"></param>
//        /// <param name="parCallback"></param>
//        public static void CalculatePathAsync(Location parStart, Location parEnd, bool parSmooth,
//            CalculatePathAsyncCallBack parCallback)
//        {
//            var list = new ArrayList { parStart, parEnd, parSmooth, parCallback };

//            ParameterizedThreadStart pts = CalculatePathProxy;
//            var thr = new Thread(pts) { IsBackground = true };
//            thr.Start(list);
//        }

//        private static void CalculatePathProxy(object parDetails)
//        {
//            var list = (ArrayList)parDetails;
//            var res = CalculatePath((Location)list[0], (Location)list[1], (bool)list[2]);
//            ((CalculatePathAsyncCallBack)list[3])(res);
//        }

//        /// <summary>
//        /// GetRandomPoint
//        /// </summary>
//        /// <param name="parStart"></param>
//        /// <param name="parMaxDistance"></param>
//        /// <returns></returns>
//        public static Location GetRandomPoint(Location parStart, float parMaxDistance)
//        {
//            var random = new Random();
//            var end = new Location(parStart.X - parMaxDistance + random.Next((int)parMaxDistance * 2),
//                parStart.Y - parMaxDistance + random.Next((int)parMaxDistance * 2), parStart.Z);
//            return end;
//        }

//        /// <summary>
//        /// GetRandomPoint
//        /// </summary>
//        /// <param name="parStart"></param>
//        /// <param name="parDistanceToMove"></param>
//        /// <returns></returns>
//        public static Location GetPointBehindPlayer(Location parStart, float parDistanceToMove)
//        {
//            var newX = parStart.X + parDistanceToMove * (float)-Math.Cos(ObjectManager.Instance.Player.Facing);
//            var newY = parStart.Y + parDistanceToMove * (float)-Math.Sin(ObjectManager.Instance.Player.Facing);
//            var end = new Location(newX, newY, parStart.Z);
//            return end;
//        }

//        /// <summary>
//        /// CalculatePathAsyncCallBack
//        /// </summary>
//        /// <param name="parPath"></param>
//        public delegate void CalculatePathAsyncCallBack(Location[] parPath);
//    }
//}

