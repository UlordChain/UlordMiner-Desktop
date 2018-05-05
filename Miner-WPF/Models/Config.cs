using Miner_WPF.Models.ViewModels;

namespace Miner_WPF.Models
{
    public class Config : BindableBase
    {
        private string url;
        private string user;
        private string id;
        private string pass;
        private int thread;
        private int cpuUsage;
        private bool automatic;

        public string Url { get => url; set => SetProperty(ref url, value); }
        public string User { get => user; set => SetProperty(ref user, value); }
        public string Id { get => id; set => SetProperty(ref id, value); }
        public string Pass { get => pass; set => SetProperty(ref pass, value); }
        public int Thread { get => thread; set => SetProperty(ref thread, value); }
        public int CpuUsage { get => cpuUsage; set => SetProperty(ref cpuUsage, value); }
        public bool Automatic { get => automatic; set => SetProperty(ref automatic, value); }

        public void SetConfig(Config config)
        {
            this.Url = config.Url;
            this.User = config.User;
            this.Id = config.Id;
            this.Pass = config.Pass;
            this.Automatic = config.Automatic;
            this.CpuUsage = config.CpuUsage;
            this.Thread = config.Thread;
        }
    }
}
