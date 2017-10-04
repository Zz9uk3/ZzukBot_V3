using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using Binarysharp.Assemblers.Fasm;
using ZzukBot.Constants;
using ZzukBot.ExtensionMethods;
using ZzukBot.Game.Statics;
using ZzukBot.Helpers.GreyMagic;
using ZzukBot.Mem.AntiWarden;
using ZzukBot.Settings;

namespace ZzukBot.Mem
{
    internal static class Memory
    {
        private static InProcessMemoryReader _Reader;
        private static FasmNet Asm;
        private static bool Applied;

        /// <summary>
        ///     Memory Reader Instance
        /// </summary>
        internal static InProcessMemoryReader Reader
            => _Reader ?? (_Reader = new InProcessMemoryReader(Process.GetCurrentProcess()));

        internal static void ErasePeHeader(string name)
        {
            var handle = WinImports.GetModuleHandle(name);
            ErasePeHeader(handle);
        }

        internal static void ErasePeHeader(IntPtr modulePtr)
        {
            if (modulePtr == IntPtr.Zero) return;
            WinImports.Protection prot;

            var dosHeader = modulePtr.ReadAs<WinImports.IMAGE_DOS_HEADER>();
            var sizeDosHeader = Marshal.SizeOf(typeof(WinImports.IMAGE_DOS_HEADER));
            var sizePeHeader = Marshal.SizeOf(typeof(WinImports.IMAGE_FILE_HEADER));

            var peHeaderPtr = modulePtr.Add(dosHeader.e_lfanew);
            var fileHeader = peHeaderPtr.ReadAs<WinImports.IMAGE_FILE_HEADER>();

            var optionalHeaderSize = fileHeader.mSizeOfOptionalHeader;
            if (optionalHeaderSize != 0)
            {
                var optionalHeaderPtr = modulePtr.Add(dosHeader.e_lfanew).Add(sizePeHeader);
                var optionalHeader = optionalHeaderPtr.ReadAs<WinImports.IMAGE_OPTIONAL_HEADER32>();

                WinImports.VirtualProtect(optionalHeaderPtr, (uint)optionalHeaderSize, WinImports.Protection.PAGE_EXECUTE_READWRITE, out prot);
                for (var i = 0; i < optionalHeaderSize; i++)
                {
                    optionalHeaderPtr.Add(i).WriteTo<byte>(0);
                }
                WinImports.VirtualProtect(optionalHeaderPtr, (uint)optionalHeaderSize, prot, out prot);
            }
            
            WinImports.VirtualProtect(modulePtr, (uint)sizeDosHeader, WinImports.Protection.PAGE_EXECUTE_READWRITE, out prot);
            for (var i = 0; i < sizeDosHeader; i++)
            {
                modulePtr.Add(i).WriteTo<byte>(0);
            }
            WinImports.VirtualProtect(modulePtr, (uint)sizeDosHeader, prot, out prot);

            WinImports.VirtualProtect(peHeaderPtr, (uint)sizePeHeader, WinImports.Protection.PAGE_EXECUTE_READWRITE, out prot);
            for (var i = 0; i < sizePeHeader; i++)
            {
                peHeaderPtr.Add(i).WriteTo<byte>(0);
            }
            WinImports.VirtualProtect(modulePtr, (uint)sizeDosHeader, prot, out prot);
        }

        internal static void UnlinkFromPeb(string moduleName)
        {
            var store = Reader.Alloc(4);
            var addrToAsm = Memory.InjectAsm(new[]
            {
                "push ebp",
                "mov ebp, esp",
                "pushad",
                "mov eax, [fs:48]",
                "mov [0x" + store.ToString("X") + "], eax",
                "popad",
                "mov esp, ebp",
                "pop ebp",
                "retn"
            }, "GetPeb");
            var callAsm = Reader.RegisterDelegate<NoParamFunc>(addrToAsm);
            callAsm();
            var pebPtr = store.ReadAs<IntPtr>();
            Reader.Dealloc(store);
            Reader.Dealloc(addrToAsm);
            var ldrData = pebPtr.Add(12).ReadAs<IntPtr>().ReadAs<WinImports.PEB_LDR_DATA>();

            var startModulePtr = ldrData.InInitOrderModuleListPtr;
            var curEntry = ldrData.InInitOrderModuleList;
            while (true)
            {
                var curEntryPtr = curEntry.Header.Flink;
                curEntry = curEntry.Header.Fwd;
                if (curEntryPtr == startModulePtr) break;
                var nextModule = curEntry.Header.Flink;
                var prevModule = curEntry.Header.Blink;
                if (curEntry.Body.BaseDllName != moduleName) continue;
                prevModule.WriteTo(nextModule);
                nextModule.Add(4).WriteTo(prevModule);
                break;
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void NoParamFunc();


        [DllImport("AntiQuery.dll")]
        internal static extern int SetupHideModules(IntPtr processId);

        [DllImport("AntiQuery.dll")]
        internal static extern int HideAdditionalModules();

        /// <summary>
        ///     Initialise InternalMemoryReader
        /// </summary>
        [Obfuscation(Feature = "virtualization", Exclude = false)]
        internal static void Init()
        {
            if (Applied) return;
            $"Initialising singletons".Log(LogFiles.InjectedLog, true);
            App.Singleton.Initialise();

            $"Hiding patches from warden".Log(LogFiles.InjectedLog, true);

            var DisableCollision = new Hack(Offsets.Hacks.DisableCollision,
                new byte[] {0x0F, 0x85, 0x1B, 0x01, 0x00, 0x00},
                "Collision");


            HookWardenMemScan.AddHack(DisableCollision);
            //DisableCollision.Apply();
            // Ctm Patch
            var CtmPatch = new Hack(Offsets.Hacks.CtmPatch,
                new byte[] {0x00, 0x00, 0x00, 0x00}, "Ctm");
            HookWardenMemScan.AddHack(CtmPatch);
            //CtmPatch.Apply();
            // wallclimb hack yay :)
            //float wc = 0.5f;
            //Hack Wallclimb = new Hack(Hacks.Wallclimb, BitConverter.GetBytes(wc), "Wallclimb");
            //HookWardenMemScan.AddHack(Wallclimb);
            //Wallclimb.Apply();

            var Collision3 = new Hack(Offsets.Hacks.Collision3, new byte[] {0xEB, 0x69}, "Collision3");
            HookWardenMemScan.AddHack(Collision3);

            // Loot patch
            var LootPatch = new Hack(Offsets.Hacks.LootPatch, new byte[] {0xEB}, "LootPatch");
            HookWardenMemScan.AddHack(LootPatch);
            LootPatch.Apply();

            var LootPatch2 = new Hack(Offsets.Hacks.LootPatch2, new byte[] {0xEB}, "LootPatch2");
            HookWardenMemScan.AddHack(LootPatch2);
            LootPatch2.Apply();

            // Ctm Hide
            var CtmHide = new Hack(Offsets.Player.CtmState, new byte[] {0x0, 0x0, 0x0, 0x0},
                new byte[] {0x0C, 0x00, 0x00, 0x00},
                "CtmHideHack") {DynamicHide = true};
            HookWardenMemScan.AddHack(CtmHide);

            var CtmHideX = new Hack(Offsets.Player.CtmX, new byte[] {0x0, 0x0, 0x0, 0x0},
                new byte[] {0x00, 0x00, 0x00, 0x00},
                "CtmHideHackX") {DynamicHide = true};
            HookWardenMemScan.AddHack(CtmHideX);

            var CtmHideY = new Hack(Offsets.Player.CtmY, new byte[] {0x0, 0x0, 0x0, 0x0},
                new byte[] {0x00, 0x00, 0x00, 0x00},
                "CtmHideHackY") {DynamicHide = true};
            HookWardenMemScan.AddHack(CtmHideY);

            var CtmHideZ = new Hack(Offsets.Player.CtmZ, new byte[] {0x0, 0x0, 0x0, 0x0},
                new byte[] {0x00, 0x00, 0x00, 0x00},
                "CtmHideHackZ") {DynamicHide = true};
            HookWardenMemScan.AddHack(CtmHideZ);


            // Lua Unlock
            var LuaUnlock = new Hack(Offsets.Hacks.LuaUnlock, new byte[] {0xB8, 0x01, 0x00, 0x00, 0x00, 0xc3},
                "LuaUnlock");
            HookWardenMemScan.AddHack(LuaUnlock);
            LuaUnlock.Apply();

            //ErasePeHeader("Loader.dll");
            //UnlinkFromPeb("Loader.dll");

            SetupHideModules((IntPtr) Process.GetCurrentProcess().Id);

            Hacks.Instance.AntiCtmStutter = true;
            Applied = true;
        }

        internal static Hack GetHack(string parName)
        {
            return HookWardenMemScan.GetHack(parName);
        }

        internal static IntPtr InjectAsm(string[] parInstructions, string parPatchName)
        {
            if (Asm == null) Asm = new FasmNet();
            Asm.Clear();
            Asm.AddLine("use32");
            foreach (var x in parInstructions)
                Asm.AddLine(x);

            var byteCode = new byte[0];
            try
            {
                byteCode = Asm.Assemble();
            }
            catch (FasmAssemblerException ex)
            {
                MessageBox.Show(
                    $"Error definition: {ex.ErrorCode}; Error code: {(int) ex.ErrorCode}; Error line: {ex.ErrorLine}; Error offset: {ex.ErrorOffset}; Mnemonics: {ex.Mnemonics}");
            }

            var start = Reader.Alloc(byteCode.Length);
            Asm.Clear();
            Asm.AddLine("use32");
            foreach (var x in parInstructions)
                Asm.AddLine(x);
            byteCode = Asm.Assemble(start);

            HookWardenMemScan.RemoveHack(start);
            HookWardenMemScan.RemoveHack(parPatchName);
            var originalBytes = Reader.ReadBytes(start, byteCode.Length);
            if (parPatchName != "")
            {
                var parHack = new Hack(start,
                    byteCode,
                    originalBytes, parPatchName);
                HookWardenMemScan.AddHack(parHack);
                parHack.Apply();
            }
            else
            {
                Reader.WriteBytes(start, byteCode);
            }
            return start;
        }

        internal static void InjectAsm(uint parPtr, string parInstructions, string parPatchName)
        {
            Asm.Clear();
            Asm.AddLine("use32");
            Asm.AddLine(parInstructions);
            var start = new IntPtr(parPtr);

            byte[] byteCode;
            try
            {
                byteCode = Asm.Assemble(start);
            }
            catch (FasmAssemblerException ex)
            {
                MessageBox.Show(
                    $"Error definition: {ex.ErrorCode}; Error code: {(int) ex.ErrorCode}; Error line: {ex.ErrorLine}; Error offset: {ex.ErrorOffset}; Mnemonics: {ex.Mnemonics}");
                return;
            }

            HookWardenMemScan.RemoveHack(start);
            HookWardenMemScan.RemoveHack(parPatchName);
            var originalBytes = Reader.ReadBytes(start, byteCode.Length);
            if (parPatchName != "")
            {
                var parHack = new Hack(start,
                    byteCode,
                    originalBytes, parPatchName);
                HookWardenMemScan.AddHack(parHack);
                parHack.Apply();
            }
            else
            {
                Reader.WriteBytes(start, byteCode);
            }
        }
    }
}