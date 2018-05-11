using Contract;
using Miner_WPF.Commons;
using Miner_WPF.Controls;
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Windows;

namespace Miner_WPF
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        [Import]
        public IMiner Miner { set; get; }
        protected override void OnStartup(StartupEventArgs e)
        {
            NotifyWindow notifyWindow = new NotifyWindow();
            notifyWindow.StartTask(() =>
            {
                try
                {
                    notifyWindow.SetMessage("正在加载程序组件...");
                    DirectoryCatalog directoryCatalog = new DirectoryCatalog(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins"));
                    CompositionContainer compositionContainer = new CompositionContainer(directoryCatalog);
                    compositionContainer.ComposeParts(this);
                    Configuration.Init(Miner, notifyWindow.SetMessage);
                }
                catch (Exception ex)
                {
                    Configuration.DebugExceptionHandle(ex);
                    Configuration.ShowErrMessage("加载组件失败，程序即将退出！");
                    Environment.Exit(0);
                }
            });
            notifyWindow.ShowDialog();
            base.OnStartup(e);
        }
    }
}
