using Microsoft.Win32;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Miner_WPF.Commons
{
    /// <summary>
    /// The CallWndProc hook procedure is an application-defined or library-defined 
    /// callback function used with the SetWindowsHookEx function. The HOOKPROC type 
    /// defines a pointer to this callback function. CallWndProc is a placeholder for 
    /// the application-defined or library-defined function name.
    /// </summary>
    /// <param name="nCode">
    /// Specifies whether the hook procedure must process the message. 
    /// </param>
    /// <param name="wParam">
    /// Specifies whether the message was sent by the current thread. 
    /// </param>
    /// <param name="lParam">
    /// Pointer to a CWPSTRUCT structure that contains details about the message.
    /// </param>
    /// <returns>
    /// If nCode is less than zero, the hook procedure must return the value returned 
    /// by CallNextHookEx. If nCode is greater than or equal to zero, it is highly 
    /// recommended that you call CallNextHookEx and return the value it returns; 
    /// otherwise, other applications that have installed WH_CALLWNDPROC hooks will 
    /// not receive hook notifications and may behave incorrectly as a result. If the 
    /// hook procedure does not call CallNextHookEx, the return value should be zero. 
    /// </returns>
    internal delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

    internal class NativeMethods
    {
        /// <summary>
        /// The SetWindowsHookEx function installs an application-defined hook 
        /// procedure into a hook chain. You would install a hook procedure to monitor 
        /// the system for certain types of events. These events are associated either 
        /// with a specific thread or with all threads in the same desktop as the 
        /// calling thread. 
        /// </summary>
        /// <param name="hookType">
        /// Specifies the type of hook procedure to be installed
        /// </param>
        /// <param name="callback">Pointer to the hook procedure.</param>
        /// <param name="hMod">
        /// Handle to the DLL containing the hook procedure pointed to by the lpfn 
        /// parameter. The hMod parameter must be set to NULL if the dwThreadId 
        /// parameter specifies a thread created by the current process and if the 
        /// hook procedure is within the code associated with the current process. 
        /// </param>
        /// <param name="dwThreadId">
        /// Specifies the identifier of the thread with which the hook procedure is 
        /// to be associated.
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is the handle to the hook 
        /// procedure. If the function fails, the return value is 0.
        /// </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowsHookEx(HookType hookType,
            HookProc callback, IntPtr hMod, uint dwThreadId);

        /// <summary>
        /// The UnhookWindowsHookEx function removes a hook procedure installed in 
        /// a hook chain by the SetWindowsHookEx function. 
        /// </summary>
        /// <param name="hhk">Handle to the hook to be removed.</param>
        /// <returns>
        /// If the function succeeds, the return value is true.
        /// If the function fails, the return value is false.
        /// </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        /// <summary>
        /// The CallNextHookEx function passes the hook information to the next hook 
        /// procedure in the current hook chain. A hook procedure can call this 
        /// function either before or after processing the hook information. 
        /// </summary>
        /// <param name="idHook">Handle to the current hook.</param>
        /// <param name="nCode">
        /// Specifies the hook code passed to the current hook procedure.
        /// </param>
        /// <param name="wParam">
        /// Specifies the wParam value passed to the current hook procedure.
        /// </param>
        /// <param name="lParam">
        /// Specifies the lParam value passed to the current hook procedure.
        /// </param>
        /// <returns>
        /// This value is returned by the next hook procedure in the chain.
        /// </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);
    }

    internal static class HookCodes
    {
        public const int HC_ACTION = 0;
        public const int HC_GETNEXT = 1;
        public const int HC_SKIP = 2;
        public const int HC_NOREMOVE = 3;
        public const int HC_NOREM = HC_NOREMOVE;
        public const int HC_SYSMODALON = 4;
        public const int HC_SYSMODALOFF = 5;
    }

    internal enum HookType
    {
        WH_KEYBOARD = 2,
        WH_MOUSE = 7,
        WH_KEYBOARD_LL = 13,
        WH_MOUSE_LL = 14
    }

    [StructLayout(LayoutKind.Sequential)]
    internal class POINT
    {
        public int x;
        public int y;
    }

    /// <summary>
    /// The MSLLHOOKSTRUCT structure contains information about a low-level keyboard 
    /// input event. 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct MOUSEHOOKSTRUCT
    {
        public POINT pt;        // The x and y coordinates in screen coordinates
        public int hwnd;        // Handle to the window that'll receive the mouse message
        public int wHitTestCode;
        public int dwExtraInfo;
    }

    /// <summary>
    /// The MOUSEHOOKSTRUCT structure contains information about a mouse event passed 
    /// to a WH_MOUSE hook procedure, MouseProc. 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct MSLLHOOKSTRUCT
    {
        public POINT pt;        // The x and y coordinates in screen coordinates. 
        public int mouseData;   // The mouse wheel and button info.
        public int flags;
        public int time;        // Specifies the time stamp for this message. 
        public IntPtr dwExtraInfo;
    }

    internal enum MouseMessage
    {
        WM_MOUSEMOVE = 0x0200,
        WM_LBUTTONDOWN = 0x0201,
        WM_LBUTTONUP = 0x0202,
        WM_LBUTTONDBLCLK = 0x0203,
        WM_RBUTTONDOWN = 0x0204,
        WM_RBUTTONUP = 0x0205,
        WM_RBUTTONDBLCLK = 0x0206,
        WM_MBUTTONDOWN = 0x0207,
        WM_MBUTTONUP = 0x0208,
        WM_MBUTTONDBLCLK = 0x0209,

        WM_MOUSEWHEEL = 0x020A,
        WM_MOUSEHWHEEL = 0x020E,

        WM_NCMOUSEMOVE = 0x00A0,
        WM_NCLBUTTONDOWN = 0x00A1,
        WM_NCLBUTTONUP = 0x00A2,
        WM_NCLBUTTONDBLCLK = 0x00A3,
        WM_NCRBUTTONDOWN = 0x00A4,
        WM_NCRBUTTONUP = 0x00A5,
        WM_NCRBUTTONDBLCLK = 0x00A6,
        WM_NCMBUTTONDOWN = 0x00A7,
        WM_NCMBUTTONUP = 0x00A8,
        WM_NCMBUTTONDBLCLK = 0x00A9
    }

    /// <summary>
    /// The structure contains information about a low-level keyboard input event. 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct KBDLLHOOKSTRUCT
    {
        public int vkCode;      // Specifies a virtual-key code
        public int scanCode;    // Specifies a hardware scan code for the key
        public int flags;
        public int time;        // Specifies the time stamp for this message
        public int dwExtraInfo;
    }

    internal enum KeyboardMessage
    {
        WM_KEYDOWN = 0x0100,
        WM_KEYUP = 0x0101,
        WM_SYSKEYDOWN = 0x0104,
        WM_SYSKEYUP = 0x0105
    }

    public class Win32Native
    {
        [DllImport("User32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO Dummy);
        internal struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }
        public static uint GetIdleTime()
        {
            LASTINPUTINFO LastUserAction = new LASTINPUTINFO();
            LastUserAction.cbSize = (uint)Marshal.SizeOf(LastUserAction);
            GetLastInputInfo(ref LastUserAction);
            uint idleTime = (uint)Environment.TickCount - LastUserAction.dwTime;
            return idleTime;
        }

        private static event EventHandler MouseEventHandler;

        // Handle to the global low-level mouse hook procedure
        private static IntPtr hGlobalLLMouseHook = IntPtr.Zero;
        private static HookProc globalLLMouseHookCallback = null;

        /// <summary>
        /// Set global low-level mouse hook
        /// </summary>
        /// <returns></returns>
        public static bool SetGlobalLLMouseHook(EventHandler handler)
        {
            MouseEventHandler += handler;
            // Create an instance of HookProc.
            globalLLMouseHookCallback = new HookProc(LowLevelMouseProc);

            hGlobalLLMouseHook = NativeMethods.SetWindowsHookEx(
                HookType.WH_MOUSE_LL,  // Must be LL for the global hook
                globalLLMouseHookCallback,
                // Get the handle of the current module
                Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]),
                // The hook procedure is associated with all existing threads running 
                // in the same desktop as the calling thread.
                0);
            return hGlobalLLMouseHook != IntPtr.Zero;
        }

        /// <summary>
        /// Remove the global low-level mouse hook
        /// </summary>
        /// <returns></returns>
        public static bool RemoveGlobalLLMouseHook()
        {
            if (hGlobalLLMouseHook != IntPtr.Zero)
            {
                // Unhook the low-level mouse hook
                if (!NativeMethods.UnhookWindowsHookEx(hGlobalLLMouseHook))
                    return false;

                hGlobalLLMouseHook = IntPtr.Zero;
            }
            return true;
        }

        /// <summary>
        /// Low-level mouse hook procedure
        /// The system call this function every time a new mouse input event is 
        /// about to be posted into a thread input queue. The mouse input can come 
        /// from the local mouse driver or from calls to the mouse_event function. 
        /// If the input comes from a call to mouse_event, the input was 
        /// "injected". However, the WH_MOUSE_LL hook is not injected into another 
        /// process. Instead, the context switches back to the process that 
        /// installed the hook and it is called in its original context. Then the 
        /// context switches back to the application that generated the event. 
        /// </summary>
        /// <param name="nCode">
        /// The hook code passed to the current hook procedure.
        /// When nCode equals HC_ACTION, the wParam and lParam parameters contain 
        /// information about a mouse message.
        /// </param>
        /// <param name="wParam">
        /// This parameter can be one of the following messages: 
        /// WM_LBUTTONDOWN, WM_LBUTTONUP, WM_MOUSEMOVE, WM_MOUSEWHEEL, 
        /// WM_MOUSEHWHEEL, WM_RBUTTONDOWN, or WM_RBUTTONUP. 
        /// </param>
        /// <param name="lParam">Pointer to an MSLLHOOKSTRUCT structure.</param>
        /// <returns></returns>
        /// <see cref="http://msdn.microsoft.com/en-us/library/ms644986.aspx"/>
        private static int LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                // Marshal the MSLLHOOKSTRUCT data from the callback lParam
                MSLLHOOKSTRUCT mouseLLHookStruct = (MSLLHOOKSTRUCT)
                    Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                // Get the mouse WM from the wParam parameter
                MouseMessage wmMouse = (MouseMessage)wParam;

                if (wmMouse == MouseMessage.WM_LBUTTONDOWN || wmMouse == MouseMessage.WM_RBUTTONDOWN)
                {
                    MouseEventHandler?.Invoke(new Tuple<int, int>(mouseLLHookStruct.pt.x, mouseLLHookStruct.pt.y), default(EventArgs));
                }
            }
            // Pass the hook information to the next hook procedure in chain
            return NativeMethods.CallNextHookEx(hGlobalLLMouseHook, nCode, wParam, lParam);
        }

        public static void SetRegistryKey(string registryKeyPath, params RegistryKeyValue[] registryKeyValues)
        {
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(registryKeyPath, true))
            {
                foreach (RegistryKeyValue item in registryKeyValues)
                {
                    registryKey.SetValue(item.Key, item.Value, item.RegistryValueKind);
                }
            }
        }
        public static void DeleteRegistryKey(string registryKeyPath, params string[] keys)
        {
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(registryKeyPath, true))
            {
                foreach (string key in keys)
                {
                    registryKey.DeleteValue(key);
                }
            }
        }
        public static object GetRegistryValue(string registryKeyPath, string key)
        {
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(registryKeyPath))
            {
                return registryKey.GetValue(key);
            }
        }
    }
    public class RegistryKeyValue
    {
        public RegistryKeyValue(string key, object value, RegistryValueKind registryValueKind)
        {
            Key = key;
            Value = value;
            RegistryValueKind = registryValueKind;
        }
        public RegistryKeyValue(string key, object value) : this(key, value, RegistryValueKind.Unknown)
        {
        }
        public string Key { set; get; }
        public object Value { set; get; }
        public RegistryValueKind RegistryValueKind { set; get; }
    }
}
