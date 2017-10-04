using System;
using System.Runtime.InteropServices;
using ZzukBot.Constants;
using ZzukBot.ExtensionMethods;
using ZzukBot.Server.AuthClient;

namespace ZzukBot.Mem.Hooks
{
    [App.Singleton]
    internal sealed class SignalEventHook
    {
        /// <summary>
        ///     Delegate to our C# function
        /// </summary>
        private readonly SignalEventDelegate _SignalEventDelegate;

        private bool Applied;

        /// <summary>
        ///     Init the hook and set enabled to true
        /// </summary>
        private SignalEventHook()
        {
            Console.WriteLine("SignalEventHook loaded");
            // Pointer the delegate to our c# function
            _SignalEventDelegate = _EventSignalHook;
            // get PTR for our c# function
            var addrToDetour = Marshal.GetFunctionPointerForDelegate(_SignalEventDelegate);
            // Alloc space for the ASM part of our detour
            string[] asmCode =
            {
                SendOvers.EventSignal[0], 
                SendOvers.EventSignal[1], 
                SendOvers.EventSignal[2], 
                SendOvers.EventSignal[3], 
                SendOvers.EventSignal[4], 
                SendOvers.EventSignal[5], 
                SendOvers.EventSignal[6], 
                SendOvers.EventSignal[7],        
                SendOvers.EventSignal[8], 
                SendOvers.EventSignal[9], 
                SendOvers.EventSignal[10],     
                SendOvers.EventSignal[11], 
                SendOvers.EventSignal[12].Replace("[|addr|]", "0x" + ((uint) addrToDetour).ToString("X")),
                SendOvers.EventSignal[13],
                SendOvers.EventSignal[14],
                SendOvers.EventSignal[15].Replace("[|addr|]", "0x" + ((uint) (Offsets.Hooks.SignalEvent + 7)).ToString("X"))
            };
            // Inject the asm code which calls our c# function
            var codeCave = Memory.InjectAsm(asmCode, "EventSignalDetour");
            // set the jmp from WoWs code to my injected code
            Memory.InjectAsm((uint) Offsets.Hooks.SignalEvent, "jmp " + codeCave, "EventSignalDetourJmp");
        }

        internal static SignalEventHook Instance { get; } = new SignalEventHook();
        internal event SignalEventEventHandler OnNewEventSignal;

        private void OnNewEventSignalEvent(string parEvent, params object[] parList)
        {
            OnNewEventSignal?.Invoke(parEvent, parList);
        }

        /// <summary>
        ///     Will be called from the ASM stub
        ///     parErrorCode contains the red message popping up on the
        ///     interface for the error
        /// </summary>
        private void _EventSignalHook(string parEvent, string parFormat, uint parFirstArg)
        {
            var format = parFormat.TrimStart('%').Split('%');
            var list = new object[format.Length];
            for (var i = 0; i < format.Length; i++)
            {
                var tmpPtr = parFirstArg + (uint) i * 4;
                if (format[i] == "s")
                {
                    var ptr = tmpPtr.ReadAs<int>();
                    var str = ptr.ReadString();
                    list[i] = str;
                }
                else if (format[i] == "f")
                {
                    var val = tmpPtr.ReadAs<float>();
                    ;
                    list[i] = val;
                }
                else if (format[i] == "u")
                {
                    var val = tmpPtr.ReadAs<uint>();
                    list[i] = val;
                }
                else if (format[i] == "d")
                {
                    var val = tmpPtr.ReadAs<int>();
                    list[i] = val;
                }
                else if (format[i] == "b")
                {
                    var val = tmpPtr.ReadAs<int>();
                    list[i] = Convert.ToBoolean(val);
                }
            }
            OnNewEventSignalEvent(parEvent, list);
        }

        internal delegate void SignalEventEventHandler(string parEvent, params object[] parArgs);

        /// <summary>
        ///     Delegate for our c# function
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void SignalEventDelegate(string parEvent, string parFormat, uint parFirstArg);
    }
}