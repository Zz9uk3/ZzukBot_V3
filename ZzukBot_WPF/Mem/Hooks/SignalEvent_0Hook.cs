using System;
using System.Runtime.InteropServices;
using ZzukBot.Constants;
using ZzukBot.Server.AuthClient;

namespace ZzukBot.Mem.Hooks
{
    [App.Singleton]
    internal sealed class SignalEvent_0Hook
    {
        /// <summary>
        ///     Delegate to our C# function
        /// </summary>
        private readonly SignalEvent_0Delegate _SignalEvent_0Delegate;

        /// <summary>
        ///     Init the hook and set enabled to true
        /// </summary>
        private SignalEvent_0Hook()
        {
            Console.WriteLine("SignalEvent_0Hook loaded");
            // Pointer the delegate to our c# function
            _SignalEvent_0Delegate = _EventSignalHook;
            // get PTR for our c# function
            var addrToDetour = Marshal.GetFunctionPointerForDelegate(_SignalEvent_0Delegate);
            // Alloc space for the ASM part of our detour
            string[] asmCode =
            {
                SendOvers.EventSignal0[0],
                SendOvers.EventSignal0[1], 
                SendOvers.EventSignal0[2], 
                SendOvers.EventSignal0[3], 
                SendOvers.EventSignal0[4], 
                SendOvers.EventSignal0[5], 
                SendOvers.EventSignal0[6].Replace("[|addr|]", "0x" + ((uint) addrToDetour).ToString("X")),
                SendOvers.EventSignal0[7],
                SendOvers.EventSignal0[8],
                SendOvers.EventSignal0[9].Replace("[|addr|]", "0x" + ((uint) Offsets.Hooks.SignalEvent_0 + 6).ToString("X")),
            };
            // Inject the asm code which calls our c# function
            var codeCave = Memory.InjectAsm(asmCode, "EventSignal_0Detour");
            // set the jmp from WoWs code to my injected code
            Memory.InjectAsm((uint) Offsets.Hooks.SignalEvent_0, "jmp " + codeCave, "EventSignal_0DetourJmp");
        }

        internal static SignalEvent_0Hook Instance { get; } = new SignalEvent_0Hook();
        internal event SignalEvent_0EventHandler OnNewEventSignal;

        private void OnNewEventSignalEvent(string parEvent)
        {
            OnNewEventSignal?.Invoke(parEvent);
        }

        /// <summary>
        ///     Will be called from the ASM stub
        ///     parErrorCode contains the red message popping up on the
        ///     interface for the error
        /// </summary>
        private void _EventSignalHook(string parEvent)
        {
            OnNewEventSignalEvent(parEvent);
        }

        internal delegate void SignalEvent_0EventHandler(string parEvent, params object[] parArgs);

        /// <summary>
        ///     Delegate for our c# function
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void SignalEvent_0Delegate(string parEvent);
    }
}