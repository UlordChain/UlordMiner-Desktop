using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Miner_WPF.Controls
{
    /// <summary>
    /// NotifyWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NotifyWindow : Window
    {
        public NotifyWindow()
        {
            InitializeComponent();
            this.Left = SystemParameters.WorkArea.Width - this.Width - 5;
            this.Top = SystemParameters.WorkArea.Height - this.Height - 5;
            this.Loaded += (s, e) =>
            {
                Storyboard storyboard = (Storyboard)this.Resources["load"];
                storyboard.Begin();
            };
        }
        public void StartTask(Action task)
        {
            Task.Factory.StartNew(() =>
            {
                task.Invoke();
                this.Dispatcher.Invoke(() => this.Visibility = Visibility.Collapsed);
            });
        }
        public void SetMessage(string message)
        {
            text_Msg.Dispatcher.Invoke(() => text_Msg.Text = message);
        }
    }
}
