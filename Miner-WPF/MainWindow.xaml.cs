using Contract;
using Miner_WPF.Commons;
using Miner_WPF.Controls;
using Miner_WPF.Models.ViewModels;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Miner_WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("User32.dll")]
        public static extern bool GetCursorPos(out POINT pt);
        public struct POINT
        {
            public int X;
            public int Y;
            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }
        POINT point;
        private bool isHidden = false;

        private WPFNotifyIcon notifyIcon = new WPFNotifyIcon();
        private WindowConfig windowConfig = new WindowConfig();
        private MainWindowViewModel model;
        public MainWindow()
        {
            InitializeComponent();
            #region Get resource
            int automaticTimeout = 30 * 1000;
            int performanceTimeout = 1000;
            double a = 30 - 1, b = 30 - 1, r = ellipse.Width / 2 - 1;
            double rate = 2 * r / 100;
            double bottom = b + r;
            LinearGradientBrush linearGradientBrushUp = (LinearGradientBrush)FindResource("LinearGradientBrushUp");
            LinearGradientBrush linearGradientBrushDown = (LinearGradientBrush)FindResource("LinearGradientBrushDown");
            Color colorWarn = (Color)FindResource("Red");
            Color colorNorm = (Color)FindResource("White");
            #endregion
            #region Binding events
            // Load
            this.Loaded += (s, e) =>
            {
                model = new MainWindowViewModel();
                this.DataContext = model;
                Win32Native.SetGlobalLLMouseHook(Mouse_Down);
                windowConfig.Show();
            };
            // Close
            this.Closed += (s, e) =>
            {
                Win32Native.RemoveGlobalLLMouseHook();
                Configuration.TryTerminate();
                notifyIcon.Dispose();
                Environment.Exit(0);
            };
            // Show
            this.MouseDoubleClick += (s, e) =>
             {
                 MainWindow_LocationChanged(default(object), default(EventArgs));
                 windowConfig.Show();
             };
            // State changed
            this.StateChanged += (s, e) =>
            {
                switch (this.WindowState)
                {
                    case WindowState.Normal:
                        InitializeLocation();
                        break;
                    case WindowState.Minimized:
                    case WindowState.Maximized:
                        this.WindowState = WindowState.Normal;
                        break;
                    default:
                        break;
                }
            };
            // Hidden window
            GetCursorPos(out point);
            this.MouseMove += MainWindow_OnMouseMove;
            this.MouseLeave += MainWidow_OnMouseLeave;
            this.MouseLeftButtonDown += MainWidow_OnMouseLeftButtonDown;
            this.LocationChanged += MainWindow_LocationChanged;
            // Context menu
            Open.Click += (s, e) =>
              {
                  this.WindowState = WindowState.Normal;
                  MainWindow_LocationChanged(default(object), default(EventArgs));
                  windowConfig.Show();
              };
            AutoRun.Click += (s, e) => Configuration.BootStart(!notifyIcon.开机启动ToolStripMenuItem.Checked, f => Configuration.ShowErrMessage($"{(f ? "设置" : "禁止")}程序开机启动失败，需要管理员权限！"));
            About.Click += (s, e) => Process.Start("http://testnet-pool.ulord.one/");
            Exit.Click += (s, e) => this.Close();
            ContextMenu.ContextMenuOpening += (s, e) => notifyIcon.开机启动ToolStripMenuItem.Checked = Configuration.IsBootStart();
            #endregion
            #region Performance
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        bool isRun;
                        IPerformance perfmon = Configuration.GetPerfmon(out isRun);
                        model.IsUP = perfmon.Hashrate > model.ComputeAbility;
                        model.ComputeAbility = perfmon.Hashrate;
                        windowConfig.SetPerformance(isRun, perfmon);
                    }
                    catch
                    {
                    }
                    Thread.Sleep(performanceTimeout);
                }
            });
            #endregion
            #region CPU usage
            Task.Factory.StartNew(() =>
            {
                double lastUsage = 0;
                while (true)
                {
                    try
                    {
                        float usage = Configuration.GetCPUUsage();
                        double y = bottom - usage * rate;
                        Tuple<Geometry, Geometry> geometrys = GetGeometry(a, b, r, y);
                        LinearGradientBrush linearGradientBrush = usage > lastUsage ? linearGradientBrushUp : linearGradientBrushDown;
                        Color color = usage >= 85 ? colorWarn : colorNorm;
                        lastUsage = usage;
                        this.Dispatcher.Invoke(() =>
                        {
                            path1.Data = geometrys.Item1;
                            path2.Data = geometrys.Item2;
                            path1.Fill = linearGradientBrush;
                            path2.Fill = linearGradientBrush;
                            shadow.Color = color;
                        });
                    }
                    catch
                    {
                    }
                    Thread.Sleep(performanceTimeout);
                }
            });
            #endregion
            #region Automatic mining
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        if (Configuration.GetAutomatic())
                        {
                            if (!Configuration.IsRun())
                            {
                                if (Configuration.IsIdle())
                                {
                                    windowConfig.StartMining();
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                    Thread.Sleep(automaticTimeout);
                }
            });
            #endregion
            InitializeNotifyIcon();
            InitializeLocation();
        }
        #region Hidden window
        private void InitializeLocation()
        {
            this.Top = SystemParameters.WorkArea.Height - this.Height;
            this.Left = SystemParameters.WorkArea.Width - this.Width;
        }
        private void MainWindow_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isHidden)
            {
                while (this.Top < 0)
                {
                    ++this.Top;
                }
                while (this.Left < 0)
                {
                    ++this.Left;
                }
                while (this.Left > SystemParameters.WorkArea.Width - this.Width)
                {
                    --this.Left;
                }
                isHidden = false;
            }
        }
        private void MainWidow_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (!isHidden)
            {
                // Distance from mouse to border
                int mouseDirection = 60;
                // Distance from the form to the boundary
                int windowDirection = 0;
                // Width or height displayed after the form is hidden
                int showWidthOrHeight = 10;
                if (point.Y <= mouseDirection && this.Top <= 0)
                {
                    if (windowConfig.Visibility == Visibility.Visible)
                    {
                        this.Top = 0;
                    }
                    else
                    {
                        while (this.Top >= -this.Height + showWidthOrHeight)
                        {
                            --this.Top;
                        }
                        isHidden = true;
                    }
                }
                else if (point.X <= mouseDirection && this.Left <= windowDirection)
                {
                    if (windowConfig.Visibility == Visibility.Visible)
                    {
                        this.Left = 0;
                    }
                    else
                    {
                        while (this.Left >= -this.Width + showWidthOrHeight)
                        {
                            --this.Left;
                        }
                        isHidden = true;
                    }
                }
                else if (point.X >= SystemParameters.WorkArea.Width - mouseDirection && this.Left >= SystemParameters.WorkArea.Width - this.Width - windowDirection)
                {
                    if (windowConfig.Visibility == Visibility.Visible)
                    {
                        this.Left = SystemParameters.WorkArea.Width - this.Width;
                    }
                    else
                    {
                        while (this.Left <= SystemParameters.WorkArea.Width - showWidthOrHeight)
                        {
                            ++this.Left;
                        }
                        isHidden = true;
                    }
                }
            }
        }
        private void MainWidow_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
                GetCursorPos(out point);
            }
            catch
            {
            }
        }
        #endregion
        #region WindowConfig
        private void Mouse_Down(object sender, EventArgs e)
        {
            if (windowConfig != null && windowConfig.IsVisible)
            {
                if (!this.IsMouseOver && !windowConfig.IsMouseOver)
                {
                    windowConfig.Hide();
                }
            }
        }
        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            double sh = SystemParameters.WorkArea.Height;
            double sw = SystemParameters.WorkArea.Width;
            double mt = this.Top;
            double ml = this.Left;
            double mh = this.Height;
            double mw = this.Width;
            double ch = windowConfig.Height;
            double cw = windowConfig.Width;
            if (ml + 0.5 * mw > 0.5 * sw)
            {
                // Left, Top 
                if (mt + 0.5 * mh > 0.5 * sh)
                {
                    windowConfig.Top = mt + 0.5 * mh - ch;
                    windowConfig.Left = ml + 0.5 * mw - cw;
                }
                // Left, Bottom
                else
                {
                    windowConfig.Top = mt + 0.5 * mh;
                    windowConfig.Left = ml + 0.5 * mw - cw;
                }
            }
            else
            {
                // Right, Top 
                if (mt + 0.5 * mh > 0.5 * sh)
                {
                    windowConfig.Top = mt + 0.5 * mh - ch;
                    windowConfig.Left = ml + 0.5 * mw;
                }
                // Right, Bottom
                else
                {
                    windowConfig.Top = mt + 0.5 * mh;
                    windowConfig.Left = ml + 0.5 * mw;
                }
            }
            if (mt > sh - mh)
            {
                this.Top = sh - mh;
            }
        }
        #endregion
        #region Notify icon
        private void InitializeNotifyIcon()
        {
            notifyIcon.notifyIcon.Icon = Properties.Resources.Ulord;
            notifyIcon.notifyIcon.Text = "挖矿程序运行中...";
            notifyIcon.打开ToolStripMenuItem.Click += (s, e) =>
            {
                this.WindowState = WindowState.Normal;
                MainWindow_LocationChanged(default(object), default(EventArgs));
                windowConfig.Show();
            };
            notifyIcon.开机启动ToolStripMenuItem.Click += (s, e) => Configuration.BootStart(!notifyIcon.开机启动ToolStripMenuItem.Checked, f => Configuration.ShowErrMessage($"{(f ? "设置" : "禁止")}程序开机启动失败，需要管理员权限！"));
            notifyIcon.关于我们ToolStripMenuItem.Click += (s, e) => Process.Start("http://testnet-pool.ulord.one/");
            notifyIcon.退出ToolStripMenuItem.Click += (s, e) => this.Close();
            notifyIcon.notifyIcon.MouseDoubleClick += (o, e) => notifyIcon.打开ToolStripMenuItem.PerformClick();
            notifyIcon.contextMenuStrip.Opening += (s, e) => notifyIcon.开机启动ToolStripMenuItem.Checked = Configuration.IsBootStart();
            notifyIcon.notifyIcon.Visible = true;
        }
        #endregion
        #region Get geometry
        private static Tuple<Geometry, Geometry> GetGeometry(double a, double b, double r, double y)
        {
            Random random = new Random(DateTime.Now.Millisecond);

            double sqrt = Math.Sqrt(Math.Pow(r, 2) - Math.Pow(y - b, 2));
            double x1 = a - sqrt, x2 = a + sqrt;

            double sub = (x2 - x1) / 4;
            double minY = b - r;
            double maxY = b + r;
            double realMinY = y - sub;
            double realMaxY = y + sub;

            Point start = new Point(x1, y);
            Point point3 = new Point(x2, y);
            Point point1, point2;

            Tuple<Point, Point> points1 = GetPoints(random, x1, x2, sub, minY, maxY, realMinY, realMaxY, y);
            point1 = points1.Item1;
            point2 = points1.Item2;
            string data1 = $"M {start.X},{start.Y} C {point1.X},{point1.Y} {point2.X},{point2.Y} {point3.X},{point3.Y} A {r},{r} 0,0,1 {a},{b + r} A {r},{r} 0,0,1 {start.X},{start.Y} Z";

            Tuple<Point, Point> points2 = GetPoints(random, x1, x2, sub, minY, maxY, realMinY, realMaxY, y);
            point1 = points2.Item1;
            point2 = points2.Item2;
            string data2 = $"M {start.X},{start.Y} C {point1.X},{point1.Y} {point2.X},{point2.Y} {point3.X},{point3.Y} A {r},{r} 0,0,1 {a},{b + r} A {r},{r} 0,0,1 {start.X},{start.Y} Z";

            Geometry geometry1 = Geometry.Parse(data1);
            Geometry geometry2 = Geometry.Parse(data2);
            return new Tuple<Geometry, Geometry>(geometry1, geometry2);
        }
        private static Tuple<Point, Point> GetPoints(Random random, double x1, double x2, double sub, double minY, double maxY, double realMinY, double realMaxY, double y)
        {
            Point point1 = new Point(x1 + sub, random.Next((int)y, (int)realMaxY));
            if (point1.Y > maxY)
            {
                point1.Y = maxY;
            }
            Point point2 = new Point(x2 - sub, random.Next((int)realMinY, (int)y));
            if (point2.Y < minY)
            {
                point2.Y = minY;
            }
            return new Tuple<Point, Point>(point1, point2);
        }
        #endregion
    }
}
