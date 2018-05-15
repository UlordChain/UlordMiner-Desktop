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
        public static extern IntPtr SetWindowsHookEx(HookType hookType, HookProc callback, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    }

    internal enum HookType
    {
        WH_KEYBOARD = 2,
        WH_MOUSE = 7,
        WH_KEYBOARD_LL = 13,
        WH_MOUSE_LL = 14
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

    public class Win32Native
    {
        [DllImport("User32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO Dummy);
        [DllImport("User32.dll")]
        private static extern bool GetCursorPos(out POINT pt);
        [DllImport("User32.dll", SetLastError = true)]
        public static extern bool BringWindowToTop(IntPtr hWnd);
        internal struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }
        internal struct POINT
        {
            public int X;
            public int Y;
            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }
        public static uint GetIdleTime()
        {
            LASTINPUTINFO LastUserAction = new LASTINPUTINFO();
            LastUserAction.cbSize = (uint)Marshal.SizeOf(LastUserAction);
            GetLastInputInfo(ref LastUserAction);
            uint idleTime = (uint)Environment.TickCount - LastUserAction.dwTime;
            return idleTime;
        }
        public Tuple<int, int> GetCursorPos()
        {
            POINT point;
            GetCursorPos(out point);
            return new Tuple<int, int>(point.X, point.Y);
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
                MouseMessage wmMouse = (MouseMessage)wParam;
                if (wmMouse == MouseMessage.WM_LBUTTONDOWN || wmMouse == MouseMessage.WM_RBUTTONDOWN)
                {
                    MouseEventHandler?.Invoke(default(object), default(EventArgs));
                }
            }
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
