using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

namespace Miner_WPF.Controls
{
    /// <summary>
    /// HelpWindow.xaml 的交互逻辑
    /// </summary>
    public partial class HelpWindow : Window
    {
        public HelpWindow(double x, double y)
        {
            InitializeComponent();
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
            this.btn_Close.MouseDown += (s, e) =>
            {
                this.Close();
            };
            this.Left = x;
            this.Top = y;
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start((sender as Hyperlink).NavigateUri.AbsoluteUri);
        }
    }
}
