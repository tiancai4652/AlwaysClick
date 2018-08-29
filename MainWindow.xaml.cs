using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AlwysClick
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            IsGo = false;

            Thread a = new Thread(RunAlwaysClick);
            a.IsBackground = true;
            a.Start();
        }
        public bool IsGo { get; set; }

        private static Dictionary<string, Keys> _DicKeyStr;
        public static Dictionary<string, Keys> DicKeyStr
        {
            get
            {
                if (_DicKeyStr != null && _DicKeyStr.Count > 0)
                {
                    return _DicKeyStr;
                }
                _DicKeyStr = new Dictionary<string, Keys>();
                foreach (var item in Enum.GetValues(typeof(Keys)))
                {
                    try
                    {
                        _DicKeyStr.Add(item.ToString(), (Keys)item);
                    }
                    catch
                    {
                        continue;
                    }
                }
                return _DicKeyStr;
            }
        }
        public Keys Key { get; set; }
        public Keys OldKey { get; set; }
        private void TextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            box.Text = e.Key.ToString();
            if (DicKeyStr.Keys.Contains(e.Key.ToString()))
            {
                Key = DicKeyStr[e.Key.ToString()];
                Register(Handle, (int)Key, KeyModifiers.None, Key);
                if (OldKey != Keys.None)
                {
                    Unregister(Handle, (int)OldKey);
                }
                OldKey = Key;
            }
        }


        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        const int MOUSEEVENTF_LEFTDOWN = 0x0002; //模拟鼠标左键按下
        const int MOUSEEVENTF_LEFTUP = 0x0004; //模拟鼠标左键抬起
        public void RunAlwaysClick()
        {
            while (true)
            {
                if (IsGo)
                {
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);//鼠标down事件  
                    Thread.Sleep(100);
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);//鼠标up事件  
                    Thread.Sleep(200);

                }
            }
        }

        public IntPtr Handle { get; set; }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x0312)
            {
                int PressKeyValue = wParam.ToInt32();
                if (PressKeyValue == (int)Key)
                {
                    IsGo = !IsGo;
                }
                
            }

            return Handle;
        }

    

        #region
        [DllImport("user32.dll")]
        public static extern int MapVirtualKey(uint Ucode, uint uMapType);
        public void Press(Keys key, int delay)
        {
            keybd_event((byte)key, (byte)(MapVirtualKey((uint)key, 0)), 0, 0);
            Thread.Sleep(delay);
            keybd_event((byte)key, (byte)(MapVirtualKey((uint)key, 0)), 2, 0);
        }


        List<Keys> RegistorList = new List<Keys>();
        public void Register(IntPtr hWnd, int id, KeyModifiers fsModifiers, Keys vk)
        {
            if (!RegistorList.Exists(t => (int)t == ((int)vk)))
            {
                RegistorList.Add(vk);
            }
            RegisterHotKey(hWnd, id, fsModifiers, vk);
        }
        public void Unregister(IntPtr hWnd, int id)
        {
            if (!RegistorList.Exists(t => (int)t == (id)))
            {
                RegistorList.Remove((Keys)id);
            }
            UnregisterHotKey(hWnd, id);
        }
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, KeyModifiers fsModifiers, Keys vk);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [Flags()]
        public enum KeyModifiers
        {
            None = 0,
            Alt = 1,
            Ctrl = 2,
            Shift = 4,
            WindowsKey = 8
        }
        [DllImport("user32.dll", EntryPoint = "keybd_event")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);


        #endregion

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (OldKey != Keys.None)
            {
                Unregister(Handle, (int)OldKey);
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Handle = new WindowInteropHelper(this).Handle;
        }
    }
}
