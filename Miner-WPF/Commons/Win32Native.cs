using Microsoft.Win32;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Miner_WPF.Commons
{
    internal delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

    internal class NativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowsHookEx(HookType hookType,
            HookProc callback, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

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

    [StructLayout(LayoutKind.Sequential)]
    internal struct MOUSEHOOKSTRUCT
    {
        public POINT pt;        // The x and y coordinates in screen coordinates
        public int hwnd;        // Handle to the window that'll receive the mouse message
        public int wHitTestCode;
        public int dwExtraInfo;
    }

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
        
        private static IntPtr hGlobalLLMouseHook = IntPtr.Zero;
        private static HookProc globalLLMouseHookCallback = null;

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
