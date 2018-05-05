using Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class Config
    {
        private string url;
        private string user;
        private string id;
        private string pass;
        private int thread;
        private int cpuUsage;
        private bool automatic;

        public string Url { get => url; set => url = value; }
        public string User { get => user; set => user = value; }
        public string Id { get => id; set => id = value; }
        public string Pass { get => pass; set => pass = value; }
        public int Thread { get => thread; set => thread = value; }
        public int CpuUsage { get => cpuUsage; set => cpuUsage = value; }
        public bool Automatic { get => automatic; set => automatic = value; }
    }
    internal class Result
    {
        public double Hashrate { set; get; }
        public int Accept { set; get; }
        public int Total { set; get; }
    }
}
