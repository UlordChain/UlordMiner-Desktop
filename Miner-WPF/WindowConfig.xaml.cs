using Contract;
using Miner_WPF.Commons;
using Miner_WPF.Models;
using Miner_WPF.Models.ViewModels;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;

namespace Miner_WPF
{
    /// <summary>
    /// WindowConfig.xaml 的交互逻辑
    /// </summary>
    public partial class WindowConfig : Window
    {
        private readonly int MinThreadCount = (int)Math.Ceiling(Environment.ProcessorCount / 2.0);
        private ConfigWindowViewModel model;
        public WindowConfig()
        {
            InitializeComponent();
            #region Binding events
            // Load
            this.Loaded += (s, e) =>
            {
                int maximum = Environment.ProcessorCount;
                for (int i = 1; i <= maximum; i++)
                {
                    comboBox_Thread.Items.Add(i);
                }
                Config config = Configuration.GetConfig();
                config.Thread = config.Thread <= 0 ? MinThreadCount : config.Thread > maximum ? maximum : config.Thread;
                model = new ConfigWindowViewModel() { Config = config, IsAlive = false };
                this.DataContext = model;
            };
            // Drag
            this.MouseLeftButtonDown += (s, e) => { try { this.DragMove(); } catch { } };
            // Close
            btn_Close.MouseDown += (s, e) => this.Close();
            // Closing
            this.Closing += (s, e) => { this.Hide(); e.Cancel = true; };
            // State changed
            this.StateChanged += (s, e) =>
            {
                switch (this.WindowState)
                {
                    case WindowState.Minimized:
                        this.WindowState = WindowState.Normal;
                        this.Hide();
                        break;
                    case WindowState.Maximized:
                        this.WindowState = WindowState.Normal;
                        this.Show();
                        break;
                    default:
                        break;
                }
            };
            // View incoming
            btn_Incoming.Click += (s, e) => Process.Start($"http://testnet-pool.ulord.one/miners/{model.Config.User}");
            // Mining
            btn_Mining.Click += Btn_Mining_Click;
            // Help
            btn_Help.MouseDown += (s, e) => Process.Start("http://testnet-pool.ulord.one/");
            #endregion
        }
        #region Set perfomance & Start mining, stop mining
        public void SetPerformance(bool isRun, IPerformance perfmon)
        {
            if (model != null)
            {
                model.IsAlive = isRun;
                model.SetPerformance(perfmon);
            }
        }
        public void StartMining()
        {
            if (model != null)
            {
                if (!TryFormatConfig())
                {
                    return;
                }
                else
                {
                    if (Configuration.TryRun(model.Config))
                    {
                        model.IsAlive = true;
                        Configuration.Save();
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }
        #endregion
        #region Start mining & Stop mining
        private void Btn_Mining_Click(object sender, RoutedEventArgs e)
        {
            if ("开始挖矿".Equals(btn_Mining.Content))
            {
                if (TryFormatConfig((s) => Configuration.ShowErrMessage(s)))
                {
                    if (Configuration.TryRun(model.Config))
                    {
                        model.IsAlive = true;
                        Configuration.Save();
                    }
                    else
                    {
                        Configuration.ShowErrMessage("启动挖矿程序失败！");
                    }
                }
            }
            else
            {
                if (Configuration.TryTerminate())
                {
                    model.IsAlive = false;
                    model.Config.Automatic = false;
                    Configuration.SetAutomatic(false);
                }
                else
                {
                    Configuration.ShowErrMessage("结束挖矿程序失败！");
                }
            }
        }
        #endregion
        #region Format input
        private bool TryFormatConfig(Action<string> action = null)
        {
            if (string.IsNullOrEmpty(model.Config.Url))
            {
                action?.Invoke("请输入地址！");
            }
            else if (string.IsNullOrEmpty(model.Config.User) || !Regex.IsMatch(model.Config.User, "^[uU][a-zA-Z0-9]{33}$"))
            {
                action?.Invoke(@"账户只能为以u\U开头的34个字母和数字字符组合！");
            }
            else if (!string.IsNullOrEmpty(model.Config.Id) && !Regex.IsMatch(model.Config.Id, "^[a-zA-Z0-9]{0,20}$"))
            {
                action?.Invoke("编号只能为不超过20位字符的数字和字母！");
            }
            else if (model.Config.Thread < 0)
            {
                action?.Invoke("线程数输入错误！");
            }
            else
            {
                model.Config.Url = model.Config.Url.Trim();
                model.Config.User = model.Config.User.Trim();
                return true;
            }
            return false;
        }
        #endregion
        #region Automatic
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!TryFormatConfig((s) => Configuration.ShowErrMessage(s)))
            {
                check_Automatic.IsChecked = false;
            }
            else
            {
                Configuration.SetAutomatic(true);
            }
        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Configuration.TryTerminate();
            Configuration.SetAutomatic(false);
        }
        #endregion
    }
}
