using Contract;
using Miner_WPF.Commons;
using Miner_WPF.Controls;
using Miner_WPF.Models.ViewModels;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Miner_WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        private bool isHidden = false;

        private WPFNotifyIcon notifyIcon = new WPFNotifyIcon();
        private WindowConfig windowConfig = new WindowConfig() { Left = 0, Top = 0 };
        private bool follow = true;
        private MainWindowViewModel model;
        public MainWindow()
        {
            InitializeComponent();
            #region Binding events
            int automaticTimeout = 30 * 1000;
            int performanceTimeout = 1000;
            SolidColorBrush normBrush = (SolidColorBrush)FindResource("NormBrush");
            SolidColorBrush warnBrush = (SolidColorBrush)FindResource("WarnBrush");
            SolidColorBrush dangerBrush = (SolidColorBrush)FindResource("DangerBrush");
            // Load
            this.Loaded += (s, e) =>
            {
                model = new MainWindowViewModel();
                this.DataContext = model;
                Win32Native.SetGlobalLLMouseHook(Mouse_Down);
                windowConfig.LocationChanged += WindowConfig_LocationChanged;
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
            // Drag
            this.MouseLeftButtonDown += (s, e) =>
            {
                try
                {
                    this.DragMove();
                }
                catch
                {
                }
            };
            // Show config window
            this.MouseRightButtonDown += WindowConfig_Show;
            this.MouseDoubleClick += WindowConfig_Show;
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
            rec.MouseEnter += MainWindow_OnMouseEnter;
            rec.MouseLeave += MainWidow_OnMouseLeave;
            this.LocationChanged += MainWindow_LocationChanged;
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
                        model.ComputeAbility = perfmon.Hashrate;
                        windowConfig.SetPerformance(isRun, perfmon);
                        this.Dispatcher.Invoke(() =>
                        {
                            if (isRun)
                            {
                                panel_Run.Visibility = Visibility.Visible;
                                panel_Stop.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                panel_Run.Visibility = Visibility.Collapsed;
                                panel_Stop.Visibility = Visibility.Visible;
                            }
                        });
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
                while (true)
                {
                    try
                    {
                        float usage = Configuration.GetCPUUsage();
                        double angle = usage * 3.6;
                        SolidColorBrush solidColorBrush = usage >= 90 ? dangerBrush : usage >= 50 ? warnBrush : normBrush;
                        this.Dispatcher.Invoke(() =>
                        {
                            arc.EndAngle = angle;
                            arc.Fill = solidColorBrush;
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
        private void MainWindow_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (isHidden)
            {
                Task.Factory.StartNew(() =>
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    this.Dispatcher.Invoke(() =>
                    {
                        if (this.Top < 0)
                        {
                            while (++this.Top < 0)
                            {
                            }
                            isHidden = false;
                        }
                        if (this.Left < 0)
                        {
                            while (++this.Left < 0)
                            {
                            }
                            isHidden = false;
                        }
                        if (this.Left > SystemParameters.WorkArea.Width - this.Width)
                        {
                            while (--this.Left > SystemParameters.WorkArea.Width - this.Width)
                            {
                            }
                            isHidden = false;
                        }
                    });
                    stopwatch.Stop();
                    Console.WriteLine($"Show: {stopwatch.Elapsed}");
                });
            }
        }

        private void MainWidow_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (!isHidden)
            {
                // Distance from the form to the boundary
                int windowDirection = 0;
                // Width or height displayed after the form is hidden
                int showWidthOrHeight = 10;
                if (this.Top <= 0)
                {
                    Task.Factory.StartNew(() =>
                    {
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        this.Dispatcher.Invoke(() =>
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
                        });
                        stopwatch.Stop();
                        Console.WriteLine($"Top: {stopwatch.Elapsed}");
                    });
                }
                else if (this.Left <= windowDirection)
                {
                    Task.Factory.StartNew(() =>
                    {
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        this.Dispatcher.Invoke(() =>
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
                        });
                        stopwatch.Stop();
                        Console.WriteLine($"Left: {stopwatch.Elapsed}");
                    });
                }
                else if (this.Left >= SystemParameters.WorkArea.Width - this.Width - windowDirection)
                {
                    Task.Factory.StartNew(() =>
                    {
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        this.Dispatcher.Invoke(() =>
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
                        });
                        stopwatch.Stop();
                        Console.WriteLine($"Right: {stopwatch.Elapsed}");
                    });
                }
            }
        }
        private bool PointInWindow(Window window, Tuple<int, int> tuple)
        {
            if (window.Left <= tuple.Item1 && tuple.Item1 <= window.Left + window.Width && window.Top <= tuple.Item2 && tuple.Item2 <= window.Top + window.Height)
            {
                return true;
            }
            return false;
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
        private void WindowConfig_Show(object sender, MouseButtonEventArgs e)
        {
            if (windowConfig.Visibility != Visibility.Visible)
            {
                follow = true;
                bool change = false;
                if (this.Top < 0)
                {
                    this.Top = 0;
                    change = true;
                }
                if (this.Left < 0)
                {
                    this.Left = 0;
                    change = true;
                }
                if (this.Left > SystemParameters.WorkArea.Width - this.Width)
                {
                    this.Left = SystemParameters.WorkArea.Width - this.Width;
                    change = true;
                }
                if (!change)
                {
                    MainWindow_LocationChanged(default(object), default(EventArgs));
                }
                windowConfig.Show();
            }
        }
        private void WindowConfig_LocationChanged(object sender, EventArgs e)
        {
            bool bl = this.Left <= windowConfig.Left && windowConfig.Left <= this.Left + this.Width || windowConfig.Left <= this.Left && this.Left <= windowConfig.Left + windowConfig.Width;
            bool bt = this.Top <= windowConfig.Top && windowConfig.Top <= this.Top + this.Height || windowConfig.Top < this.Top && this.Top <= windowConfig.Top + windowConfig.Height;
            follow = bl && bt;
        }
        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            // windows center point
            double centerML = this.Left + this.Width * 0.5;
            double centerMT = this.Top + this.Height * 0.5;
            double centerCL = windowConfig.Left + windowConfig.Width * 0.5;
            double centerCT = windowConfig.Top + windowConfig.Height * 0.5;
            // config window follow location
            double l = this.Left + 0.5 * this.Width - windowConfig.Width;
            double t = this.Top + 0.5 * this.Height - windowConfig.Height;
            double r = this.Left + 0.5 * this.Width;
            double b = this.Top + 0.5 * this.Height;
            // config window location range
            double minl = 0;
            double mint = 0;
            double maxl = SystemParameters.WorkArea.Width - windowConfig.Width;
            double maxt = SystemParameters.WorkArea.Height - windowConfig.Height;
            // config window really location
            double cl;
            double ct;
            if (!follow)
            {
                WindowConfig_LocationChanged(default(object), default(EventArgs));
            }
            if (follow)
            {
                // calculate config window location
                if (centerML > centerCL)
                {
                    // Left, Top 
                    if (centerMT > centerCT)
                    {
                        cl = l;
                        ct = t;
                    }
                    // Left, Bottom
                    else
                    {
                        cl = l;
                        ct = b;
                    }
                }
                else
                {
                    // Right, Top 
                    if (centerMT > centerCT)
                    {
                        cl = r;
                        ct = t;
                    }
                    // Right, Bottom
                    else
                    {
                        cl = r;
                        ct = b;
                    }
                }
                // limit config window location
                if (cl < minl)
                {
                    cl = r;
                }
                if (cl > maxl)
                {
                    cl = l;
                }
                if (ct < mint)
                {
                    ct = b;
                }
                if (ct > maxt)
                {
                    ct = t;
                }
                windowConfig.Left = cl;
                windowConfig.Top = ct;
            }
            // limit main window top
            if (this.Top > SystemParameters.WorkArea.Height - this.Height)
            {
                this.Top = SystemParameters.WorkArea.Height - this.Height;
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
                Win32Native.BringWindowToTop(new WindowInteropHelper(this).Handle);
                WindowConfig_Show(default(object), default(MouseButtonEventArgs));
            };
            notifyIcon.开机启动ToolStripMenuItem.Click += (s, e) => Configuration.BootStart(!notifyIcon.开机启动ToolStripMenuItem.Checked, f => Configuration.ShowErrMessage($"{(f ? "设置" : "禁止")}程序开机启动失败，需要管理员权限！"));
            notifyIcon.关于我们ToolStripMenuItem.Click += (s, e) => Process.Start("http://testnet-pool.ulord.one/");
            notifyIcon.退出ToolStripMenuItem.Click += (s, e) => this.Close();
            notifyIcon.notifyIcon.MouseDoubleClick += (o, e) => notifyIcon.打开ToolStripMenuItem.PerformClick();
            notifyIcon.contextMenuStrip.Opening += (s, e) => notifyIcon.开机启动ToolStripMenuItem.Checked = Configuration.IsBootStart();
            notifyIcon.notifyIcon.Visible = true;
        }
        #endregion
    }
}
